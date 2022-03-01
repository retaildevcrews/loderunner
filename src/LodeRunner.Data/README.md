# LodeRunner.Data

## CosmosDB Accounts and Databases

 Account              | Database         |  Notes                   |
| :--------------     | :-------         | :----------------------- |
| ngsa-asb-dev-cosmos | LodeRunnerDB     |                          |
| ngsa-asb-dev-cosmos | LodeRunnerTestDB | Used for GitHub workflow |
| ngsa-asb-pre-cosmos | LodeRunnerDB     |                          |
| ngsa-asb-pre-cosmos | LodeRunnerTestDB |                          |

## Configuing CosmosDB Secrets for Local Use

### Using Cosmos DB Emulator

If running in codespaces, the emulator should be running in devcontainer on default port `9090`. To run loderunner applications against the emaulator in a self contained development environment:

- Update `LodeRunner` and `LodeRunner.API` secrets to use Cosmos Emulator running in codespaces:

    > Note: Upon restart of an existing codespaces, you will be required to run this script again to make sure host files and certs are up to date.

    ```bash
        cd ../..
        ./scripts/cosmos-emulator-mode.sh
    ```

- Access Cosmos Emulator UI by appending `/_explorer/index.html` to CosmosDB Emulator port address:

  - For Codespaces in broswer: `https://<local-address-for-port-9090>/_explorer/index.html`

  - For Codespaces in VSCode: `https://localhost:9090/_explorer/index.html`

### Using shared Cosmos DB

Certain loderunner applications (i.e. LodeRunner, LodeRunner.API) require a Read-Write key from CosmosDB.

The rest of the required files in the `/secrets` folder are already included (i.e. `CosmosCollection`, `CosmosDatabase`, `CosmosTestDatabase`, `CosmosUrl`).  You may set them up manually by:

1. In the loderunner application's directory, create `secrets/CosmosKey`
2. Save the Read-Write key from CosmosDB in the CosmosKey file

Alternatively, you can use the following shell commands:
|Step | Description        | Command |
| --- |:-----------        |:------- |
|1    | Log Into Azure     | `az login --use-device-code`   |
|2    | Set subscription   |  `az account set -s COSMOSDB_SUBSCRIPTION_NAME_OR_ID`        |
|3    | Set CosmosDB environment variables |         |
|     | **CosmosDB_RG**    | `export CosmosDB_RG=rg-ngsa-asb-dev-cosmos`         |
|     | **CosmosDB_ACCT**  | `export CosmosDB_ACCT=ngsa-asb-dev-cosmos`|
|     | **LR_DB**             | `export LR_DB=LodeRunnerDB`   |
|     | **TEST_DB**           | `export Test_DB=LodeRunnerTestDB` |
|     | **LR_COL**            | `export LR_COL=LodeRunner`        |
|     | **CosmosDB_URL**      | `export CosmosDB_URL=https://ngsa-asb-dev-cosmos.documents.azure.com:443/`|
|     | **COSMOS_RW_KEY**     | `export COSMOS_RW_KEY=$(eval az cosmosdb keys list -n $CosmosDB_ACCT -g $CosmosDB_RG --query primaryMasterKey -o tsv)`         |
|4    | Create secret file    | `echo $COSMOS_RW_KEY > ./src/LodeRunner.API/secrets/CosmosKey` |

The following block has all of the environment variable assignments in one place to copy and paste:

```bash
export CosmosDB_RG=rg-ngsa-asb-dev-cosmos
export CosmosDB_ACCT=ngsa-asb-dev-cosmos
export LR_DB=LodeRunnerDB
export TEST_DB=LodeRunnerTestDB
export LR_COL=LodeRunner
export CosmosDB_URL=https://ngsa-asb-dev-cosmos.documents.azure.com:443/
export COSMOS_RW_KEY=$(eval az cosmosdb keys list -n $CosmosDB_ACCT -g $CosmosDB_RG --query primaryMasterKey -o tsv)
echo $COSMOS_RW_KEY > ./src/LodeRunner.API/secrets/CosmosKey
```

## CosmosDB Firewall IP Ranges

Certain loderunner applications (i.e. LodeRunner, LodeRunner.API) add your IP address to allow access to CosmosDB.  If you run the app and cannot access the CosmosDB due to firewall you may see the error output:

> `"ExceptionMessage": "Repository test for LodeRunnerDB:LodeRunner failed."

To solve this we need allow your IP through the firewall.

### Allowing IP Access via the Portal

1. Navigate to the Azure Portal
2. Go to the associated `Azure Cosmos DB account`
3. Under `Settings`, go to `Firewall and virtual networks`
4. Add your IP address under `Firewall` > `IP`
5. Save
6. A notification with "Updating Firewall configuration" should appear at the top
7. It will take a little time to update. Once the firewall is updated you will be able to run the app.

### Allowing IP Access via CLI
<!-- markdownlint-disable MD033 -->
You will need to have set the environment variables from the previous section [CosmosDB Secrets](#cosmosdb-secrets).  The following table has the values that you are setting and the command.

|Step | Description        | Command |
| --- |:-----------        |:------- |
|   1  | **YOUR_IP**           |  `export YOUR_IP=$(eval dig +short myip.opendns.com @resolver1.opendns.com)` |
|   2  | **IPS_FOR_FIREWALL**  | `export IPS_FOR_FIREWALL=az cosmosdb show -g $CosmosDB_RG -n $CosmosDB_ACCT | jq -r '.ipRules | map(.ipAddressOrRange) | @csv' | tr -d '"'` |
|   3  | Check if your IP already allowed. <br> **Skip to step 6 if your IP is in the list** highlighted in <font color=red>red</font> by default. If you **do not see your address** then you will need to add it by proceeding to step 5.           | `grep "$YOUR_IP" <<< "$IPS_FOR_FIREWALL"` |
|  4 | Create **ALL_IPS** list | `export IPS_FOR_FIREWALL=$(eval az cosmosdb show -g $CosmosDB_RG -n $CosmosDB_ACCT | jq -r '.ipRules | map(.ipAddressOrRange) | @csv' | tr -d '"')` |
|  5    | Update the allowed IPs for CosmosDB | `az cosmosdb update -n $CosmosDB_ACCT -g $CosmosDB_RG --ip-range-filter $ALL_IPS` |

The following block is all of the shell commands pulled together with a conditional to check if you're IP is already allowed.  If not it will add it.

```bash
export YOUR_IP=$(eval dig +short myip.opendns.com @resolver1.opendns.com)
export IPS_FOR_FIREWALL=$(eval az cosmosdb show -g $CosmosDB_RG -n $CosmosDB_ACCT | jq -r '.ipRules | map(.ipAddressOrRange) | @csv' | tr -d '"')
export ALL_IPS="\"${IPS_FOR_FIREWALL},${YOUR_IP}\""
if [[ $IPS_FOR_FIREWALL != *$YOUR_IP* ]]; then
   echo "Your IP was not found. Adding to CosmosDB firewall..."
   az cosmosdb update -n $CosmosDB_ACCT -g $CosmosDB_RG --ip-range-filter $ALL_IPS
fi
```
<!-- markdownlint-enable MD033 -->
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
