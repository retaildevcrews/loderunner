#!/bin/bash

if [ -z "$LR_NAME" ]
then
    echo "Please set LR_NAME before running this script"
elif [ -z "$LR_SUBSCRIPTION" ]
then
    echo "Please set LR_SUBSCRIPTION before running this script"
elif [ -z "$LR_RG" ]
then
    echo "Please set LR_RG before running this script"
else
    # Set CosmosDB values
    LR_COSMOS_ACCOUNT="${LR_NAME}-cosmos"
    LR_COSMOS_DB="LodeRunnerDB"
    LR_COSMOS_TEST_DB="LodeRunnerTestDB"
    LR_COSMOS_COL="LodeRunner"
    LR_COSMOS_LEASE="LRAPI"

    # Check if CosmosDB account name already exists
    echo "Checking if CosmosDB account name already exists..."
    ACCOUNT_EXISTS=$(az cosmosdb check-name-exists -n $LR_COSMOS_ACCOUNT --subscription $LR_SUBSCRIPTION)
    if "${ACCOUNT_EXISTS}"
    then
        echo "Using existing CosmosDB account, ${LR_COSMOS_ACCOUNT}..."
    else
        # Create CosmosDB account
        echo "Creating CosmosDB account, ${LR_COSMOS_ACCOUNT}..."
        az cosmosdb create -g $LR_RG -n $LR_COSMOS_ACCOUNT
    fi

    # Create CosmosDB database
    echo "Creating CosmosDB database, ${LR_COSMOS_DB}..."
    az cosmosdb sql database create -a $LR_COSMOS_ACCOUNT -n $LR_COSMOS_DB -g $LR_RG --subscription $LR_SUBSCRIPTION
    # Create lease container for change feed processor
    echo "Creating lease container for database change feed processor, ${LR_COSMOS_LEASE}..."
    az cosmosdb sql container create -a $LR_COSMOS_ACCOUNT -d $LR_COSMOS_DB -n $LR_COSMOS_LEASE -p "/id" -g $LR_RG --subscription $LR_SUBSCRIPTION
    # Create CosmosDB container
    echo "Creating CosmosDB container, ${LR_COSMOS_COL}..."
    az cosmosdb sql container create -a $LR_COSMOS_ACCOUNT -d $LR_COSMOS_DB -n $LR_COSMOS_COL -p "/partitionKey" -g $LR_RG --subscription $LR_SUBSCRIPTION --ttl -1


    # Create CosmosDB test database
    echo "Creating CosmosDB test database, ${LR_COSMOS_TEST_DB}..."
    az cosmosdb sql database create -a $LR_COSMOS_ACCOUNT -n $LR_COSMOS_TEST_DB -g $LR_RG --subscription $LR_SUBSCRIPTION
    # Create lease container for change feed processor
    echo "Creating lease container for test database change feed processor, ${LR_COSMOS_LEASE}..."
    az cosmosdb sql container create -a $LR_COSMOS_ACCOUNT -d $LR_COSMOS_TEST_DB -n $LR_COSMOS_LEASE -p "/id" -g $LR_RG --subscription $LR_SUBSCRIPTION
    # Create CosmosDB container
    echo "Creating CosmosDB test container, ${LR_COSMOS_COL}..."
    az cosmosdb sql container create -a $LR_COSMOS_ACCOUNT -d $LR_COSMOS_TEST_DB -n $LR_COSMOS_COL -p "/partitionKey" -g $LR_RG --subscription $LR_SUBSCRIPTION --ttl -1

    # Get document endpoint
    LR_COSMOS_ENDPOINT=$(az cosmosdb show -n $LR_COSMOS_ACCOUNT -g $LR_RG --subscription $LR_SUBSCRIPTION --query "documentEndpoint")

    echo "Successfully created..."
    echo "Document Endpoint: ${LR_COSMOS_ENDPOINT}"
fi
