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

    # Create CosmosDB account
    az cosmosdb create -g $LR_RG -n $LR_COSMOS_ACCOUNT

    # Create CosmosDB database
    az cosmosdb sql database create -a $LR_COSMOS_ACCOUNT -n $LR_COSMOS_DB -g $LR_RG --subscription $LR_SUBSCRIPTION

    # Create CosmosDB container
    az cosmosdb sql container create -a $LR_COSMOS_ACCOUNT -d $LR_COSMOS_DB -n $LR_COSMOS_COL -p "/partitionKey" -g $LR_RG --subscription $LR_SUBSCRIPTION --ttl -1

    # Create CosmosDB test database
    az cosmosdb sql database create -a $LR_COSMOS_ACCOUNT -n $LR_COSMOS_TEST_DB -g $LR_RG --subscription $LR_SUBSCRIPTION

    # Create CosmosDB container
    az cosmosdb sql container create -a $LR_COSMOS_ACCOUNT -d $LR_COSMOS_TEST_DB -n $LR_COSMOS_COL -p "/partitionKey" -g $LR_RG --subscription $LR_SUBSCRIPTION --ttl -1
```

Note: After the Cosmos DB and containers have been set up using the steps above, go to the Cosmos DB account's home page in Azure Portal. Under ***Settings --> Networking***, ensure that ***Accept connections from within public Azure datacenters*** is selected.

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

## Run Stored Procedure for Data Cleanup

1. Navigate to the Data Explorer in the Azure Cosmos DB account (ngsa-asb-dev-cosmos)
2. In the NoSQL API pane near the left of the screen, locate the `bulkDelete` Stored Procedure under the following path

    > DATA / LodeRunnerDB / LodeRunner / Stored Procedures / bulkDelete

3. Open the `bulkDelete` Stored Procedure and then click on its respective tab in the ribbon near the center of the screen
4. Click on the `Execute` button in the tool bar above the tab ribbon
    - This should reveal an `Input Parameters` panel near the right side of the screen
5. In the `Input Parameters` panel, enter the parameters for which the `bulkDelete` Stored Procedure should receive. Refer to the following explanations of each field:
    - **Partition Key Value**
        - The `Partition Key Value` field should be populated with one of the two following values:
            1. LoadTestConfig
            2. TestRun
        - To do a full data cleanup, the Stored Procedure must be run three separate times - one for each `Partition Key Value`
    - **Enter input parameters (if any)**
        - The `Enter input parameters (if any)` field requires a SQL select query that the `bulkDelete` Stored Procedure will use to delete values
            - All values returned by the provided SQL query will be hard deleted from the database
        - The following query will return all records of the provided `Partition Key Value` type that were created before a specified date (modify date as necessary)

        ```bash
            SELECT * FROM c where TimestampToDateTime(c._ts*1000) < "YYYY-MM-DD"
        ```

        - *Note that the TOP keyword cannot be used to restrict deletion to a select number of records, the `bulkDelete` Stored Procedure will always return the maximum number of records*
6. Click the `Execute` button in the bottom left corner of the `Input Parameters` panel
