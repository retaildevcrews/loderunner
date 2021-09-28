# CosmosDB

## Create CosmosDB via Script

```bash
    # Set a name to reference
    export LR_NAME="[starts with a-z, [az,0-9]]"
    # Set existing subscription name or ID
    export LR_SUBSCRIPTION=""
    # Set existing resource group name
    export LR_RG=""

    # Run script
    ./scripts/cosmos-create.sh
```

## Create CosmosDB Manually

```bash
    # Set a name to reference
    export LR_NAME="[starts with a-z, [az,0-9]]"
    # Set existing subscription name or ID
    export LR_SUBSCRIPTION=""
    # Set existing resource group name
    export LR_RG=""

    # Set CosmosDB values
    export LR_COSMOS_ACCOUNT="${LR_NAME}-cosmos"
    export LR_COSMOS_DB="LodeRunnerDB"
    export LR_COSMOS_TEST_DB="LodeRunnerTestDB"
    export LR_COSMOS_COL="LodeRunner"
    export LR_COSMOS_LEASE="LRAPI"

    # Create CosmosDB account
    az cosmosdb create -g $LR_RG -n $LR_COSMOS_ACCOUNT

    # Create CosmosDB database
    az cosmosdb sql database create -a $LR_COSMOS_ACCOUNT -n $LR_COSMOS_DB -g $LR_RG --subscription $LR_SUBSCRIPTION
    # Create CosmosDB lease container for change feed processor
    az cosmosdb sql container create -a $LR_COSMOS_ACCOUNT -d $LR_COSMOS_DB -n $LR_COSMOS_LEASE -p "/id" -g $LR_RG --subscription $LR_SUBSCRIPTION
    # Create CosmosDB container
    az cosmosdb sql container create -a $LR_COSMOS_ACCOUNT -d $LR_COSMOS_DB -n $LR_COSMOS_COL -p "/partitionKey" -g $LR_RG --subscription $LR_SUBSCRIPTION --ttl -1

    # Create CosmosDB test database
    az cosmosdb sql database create -a $LR_COSMOS_ACCOUNT -n $LR_COSMOS_TEST_DB -g $LR_RG --subscription $LR_SUBSCRIPTION
    # Create CosmosDB lease container for change feed processor
    az cosmosdb sql container create -a $LR_COSMOS_ACCOUNT -d $LR_COSMOS_TEST_DB -n $LR_COSMOS_LEASE -p "/id" -g $LR_RG --subscription $LR_SUBSCRIPTION
    # Create CosmosDB container
    az cosmosdb sql container create -a $LR_COSMOS_ACCOUNT -d $LR_COSMOS_TEST_DB -n $LR_COSMOS_COL -p "/partitionKey" -g $LR_RG --subscription $LR_SUBSCRIPTION --ttl -1
```

## Delete CosmosDB Manually

```bash
    # Set a name to reference
    export LR_NAME="[starts with a-z, [az,0-9]]"
    # Set existing subscription name or ID
    export LR_SUBSCRIPTION=""
    # Set existing resource group name
    export LR_RG=""
    # Set CosmosDB account name
    export LR_COSMOS_ACCOUNT="${LR_NAME}-cosmos"

    # Delete CosmosDB account
    az cosmosdb delete -n $LR_COSMOS_ACCOUNT -g $LR_RG --subscription $LR_SUBSCRIPTION -y
```
