# CosmosDB

## Create CosmosDB via Script

```bash
    # Set a name to reference
    export RR_NAME="[starts with a-z, [az,0-9]]"
    # Set existing subscription name or ID
    export RR_SUBSCRIPTION=""
    # Set existing resource group name
    export RR_RG=""

    # Run script
    ./scripts/cosmos-create.sh
```

## Create CosmosDB Manually

```bash
    # Set a name to reference
    export RR_NAME="[starts with a-z, [az,0-9]]"
    # Set existing subscription name or ID
    export RR_SUBSCRIPTION=""
    # Set existing resource group name
    export RR_RG=""

    # Set CosmosDB values
    export RR_COSMOS_ACCOUNT="${RR_NAME}-cosmos"
    export RR_COSMOS_DB="relayRunner"
    export RR_COSMOS_COL="clientStatus"

    # Create CosmosDB account
    az cosmosdb create -g $RR_RG -n $RR_COSMOS_ACCOUNT
    # Create CosmosDB database
    az cosmosdb sql database create -a $RR_COSMOS_ACCOUNT -n $RR_COSMOS_DB -g $RR_RG --subscription $RR_SUBSCRIPTION
    # Create CosmosDB lease container for change feed processor
    az cosmosdb sql container create -a $RR_COSMOS_ACCOUNT -d $RR_COSMOS_DB -n RRAPI -p "/id" -g $RR_RG --subscription $RR_SUBSCRIPTION
    # Create CosmosDB container
    az cosmosdb sql container create -a $RR_COSMOS_ACCOUNT -d $RR_COSMOS_DB -n $RR_COSMOS_COL -p "/partitionKey" -g $RR_RG --max-throughput 4000 --subscription $RR_SUBSCRIPTION --ttl -1
```

## Delete CosmosDB Manually

```bash
    # Set a name to reference
    export RR_NAME="[starts with a-z, [az,0-9]]"
    # Set existing subscription name or ID
    export RR_SUBSCRIPTION=""
    # Set existing resource group name
    export RR_RG=""
    # Set CosmosDB account name
    export RR_COSMOS_ACCOUNT="${RR_NAME}-cosmos"

    # Delete CosmosDB account
    az cosmosdb delete -n $RR_COSMOS_ACCOUNT -g $RR_RG --subscription $RR_SUBSCRIPTION -y
```
