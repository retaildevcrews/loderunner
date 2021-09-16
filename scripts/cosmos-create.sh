#!/bin/bash

if [ -z "$RR_NAME" ]
then
    echo "Please set RR_NAME before running this script"
elif [ -z "$RR_SUBSCRIPTION" ]
then
    echo "Please set RR_SUBSCRIPTION before running this script"
elif [ -z "$RR_RG" ]
then
    echo "Please set RR_RG before running this script"
else
    # Set CosmosDB values
    RR_COSMOS_ACCOUNT="${RR_NAME}-cosmos"
    RR_COSMOS_DB="relayRunner"
    RR_COSMOS_LEASE="clientStatusLease"
    RR_COSMOS_COL="clientStatus"

    # Check if CosmosDB account name already exists
    echo "Checking if CosmosDB account name already exists..."
    ACCOUNT_EXISTS=$(az cosmosdb check-name-exists -n $RR_COSMOS_ACCOUNT --subscription $RR_SUBSCRIPTION)
    if "${ACCOUNT_EXISTS}"
    then
        echo "Using existing CosmosDB account, ${RR_COSMOS_ACCOUNT}..."
    else
        # Create CosmosDB account
        echo "Creating CosmosDB account, ${RR_COSMOS_ACCOUNT}..."
        az cosmosdb create -g $RR_RG -n $RR_COSMOS_ACCOUNT
    fi

    # Create CosmosDB database
    echo "Creating CosmosDB database, ${RR_COSMOS_DB}..."
    az cosmosdb sql database create -a $RR_COSMOS_ACCOUNT -n $RR_COSMOS_DB -g $RR_RG --subscription $RR_SUBSCRIPTION
    # Create lease container for change feed processor
    echo "Creating lease container for change feed processor, ${RR_COSMOS_LEASE}..."
    az cosmosdb sql container create -a $RR_COSMOS_ACCOUNT -d $RR_COSMOS_DB -n $RR_COSMOS_LEASE -p "/id" -g $RR_RG --subscription $RR_SUBSCRIPTION
    # Create CosmosDB container
    echo "Creating CosmosDB container, ${RR_COSMOS_COL}..."
    az cosmosdb sql container create -a $RR_COSMOS_ACCOUNT -d $RR_COSMOS_DB -n $RR_COSMOS_COL -p "/partitionKey" -g $RR_RG --max-throughput 4000 --subscription $RR_SUBSCRIPTION --ttl -1
    # Get document endpoint
    RR_COSMOS_ENDPOINT=$(az cosmosdb show -n $RR_COSMOS_ACCOUNT -g $RR_RG --subscription $RR_SUBSCRIPTION --query "documentEndpoint")

    echo "Successfully created..."
    echo "Document Endpoint: ${RR_COSMOS_ENDPOINT}"
fi
