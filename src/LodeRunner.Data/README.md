# LodeRunner.Data

## CosmosDB Accounts and Databases

 Account          | Database         |  Notes                   |
| :-------------- | :-------         | :----------------------- |
| ngsa-dev-cosmos | LodeRunnerDB     |                          |
| ngsa-dev-cosmos | LodeRunnerTestDB | Used for GitHub workflow |
| lr-pre-cosmos   | LodeRunnerDB     |                          |
| lr-pre-cosmos   | LodeRunnerTestDB | Used for GitHub workflow |

## CosmosDB Collection: LodeRunner

> partitionKey uses the entityType

### Item: clientStatus

Conveys the current status, time of that status, and the associated LoadClient's initial start-up configuration.

- TTL for container is set to no default (-1)
- TTL for clientStatus items is set to 60 seconds

```javascript
{
    "id": "5",
    "partitionKey": "ClientStatus",
    "entityType": "ClientStatus",
    "lastUpdated": "2021-08-17T14:36:37.0897032Z", // in UTC
    "statusDuration": 20, // seconds since last status change
    "status": "Ready", // Starting | Ready | Testing | Terminating
    "message": "Additional status update notes",
    "loadClient": {
        "id": "2",
        "entityType": "LoadClient",
        "version": "0.3.0-717-1030",
        "name": "Central-az-central-us-2",
        "region": "Central",
        "zone": "az-central-us",
        "prometheus": false,
        "startupArgs": "--delay-start -1 --secrets-volume secrets",
        "startTime": "2021-08-17T14:36:37.0897032Z" // in UTC
    }
}
```

## CosmosDB Collection: LRAPI

Used as the lease container for ChangeFeed

- A lease container acts as state storage and coordinates processing the change feed across multiple workers. ([Microsoft Documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/change-feed-processor#components-of-the-change-feed-processor))

Lease container requirements

- Partion key definition must be `/id`. ([Microsoft Documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/change-feed-functions#requirements))
- The connection string to Azure Cosmos DB account with lease collection must have write permissions. ([Microsoft Documentation](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-trigger?tabs=csharp#configuration))

## CosmosDB Key

Certain loderunner applications (i.e. LodeRunner, LodeRunner.API) require a Read-Write key from CosmosDB.

The rest of the required files in the `/secrets` folder are already included (i.e. `CosmosCollection`, `CosmosDatabase`, `CosmosTestDatabase`, `CosmosUrl`).

1. In the loderunner application's directory, create `secrets/CosmosKey`
2. Save the Read-Write key from CosmosDB in the CosmosKey file

## CosmosDB Firewall IP Ranges

Certain loderunner applications (i.e. LodeRunner, LodeRunner.API) add your IP address to allow access to CosmosDB

### Example of issues not setting this will cause

- `"ExceptionMessage": "Repository test for LodeRunnerDB:LodeRunner failed."

### Solution

1. Navigate to the Azure Portal
2. Go to the associated `Azure Cosmos DB account`
3. Under `Settings`, go to `Firewall and virtual networks`
4. Add your IP address under `Firewall` > `IP`
5. Save
6. A notification with "Updating Firewall configuration" should appear at the top
7. It will take a little time to update. Once the firewall is updated you will be able to run the app.

TODO - Replace the Add IP instructions with CLI command if possible
