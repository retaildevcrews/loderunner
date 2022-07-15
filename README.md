# loderunner

- [LodeRunner](./src/LodeRunner/README.md) is the load client that waits for TestRun entries in CosmosDB, and execute those load tests
- [LodeRunner.API](./src/LodeRunner.API/README.md) is a service that creates LoadTestConfig and TestRun entries in CosmosDB for LodeRunner to use in order to execute load tests
- [LodeRunner.UI](./src/LodeRunner.UI/README.md) utilizes the LodeRunner.API endpoints to provide a user-friendly interface to create LoadTestConfigs and TestRun entries in CosmosDB

**NOTE** All components of the loderunner ecosystem have additional functionality that are not listed here

## Prerequisites

- Bash shell
  - Tested on: Visual Studio Codespaces, Mac, Ubuntu, Windows with WSL2)
  - Not supported: WSL1 or Cloud Shell
- jq: `sudo apt-get install jq`
- tr: `sudo apt-get install translate`
- Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Docker CLI ([download](https://docs.docker.com/install/))
- .NET 5.0 ([download](https://docs.microsoft.com/en-us/dotnet/core/install/))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Running the System via Codespaces

1. Open codespaces in <https://github.com/retaildevcrews/loderunner>
2. Verify in the loderunner directory `pwd`
3. Set the endpoint LodeRunner.UI makes API calls to
   - In Codespaces, navigate to the `PORTS` terminal
   - Identify port `LodeRunner API (32088)` and hover over the `Local Address`
   - Click on the clipboard icon to copy the local address
   - Open `deploy/loderunner/local/4-loderunner-ui.yaml`
   - Under `env` > `name: "LRAPI_DNS"`, set the `value` to copied LodeRunner.API URL
   - Prevent accidental commits with `git update-index --assume-unchanged deploy/loderunner/local/4-loderunner-ui.yaml`
4. Set environmental variables with CosmosDB values for K8S generic secret
   - Set CosmosDB: `export LR_DB=LodeRunnerDB`
   - Set CosmosDB Collection: `export LR_COL=LodeRunner`
   - Setup either **shared dev DB** or **CosmosDB Emulator**
     - Set CosmosDB using **shared dev DB**:
        - Set CosmosDB URL: `export LR_URL=https://ngsa-asb-dev-cosmos.documents.azure.com:443/`
        - Add Your IP Address To CosmosDB Firewall Allow List: [LodeRunner.Data](./src/LodeRunner.Data/README.md#solution)
        - Set Command to Get CosmosDB Key with Read-Write permissions
        - Log Into Azure: `az login --use-device-code`
        - Set Subscription: `az account set -s COSMOSDB_SUBSCRIPTION_NAME_OR_ID`
        - Set Command: `export LR_KEY=$(eval az cosmosdb keys list -n ngsa-asb-dev-cosmos -g rg-ngsa-asb-dev-cosmos --query primaryMasterKey -o tsv)`
     - Or Set CosmosDB using **CosmosDB Emulator** running locally:
        - Set CosmosDB URL: `export LR_URL=https://${COSMOS_EMULATOR_NAME}.documents.azure.com`
        - Set CosmosDB Key:

          ```bash
          export LR_KEY=$(docker top ${COSMOS_EMULATOR_NAME} |grep  -oP '/Key=(\w.*) '|head -n 1 | awk -F' ' '{print $1}' | awk -F 'Key=' '{print $2}')
          ```

5. Save environmental variables for future re-run via `./deploy/loderunner/local/saveenv.sh`
6. Start the k3d cluster `make create`
7. Deploy pods
   - ~~`make all`: LodeRunner, LodeRunner.API, LodeRunner.UI, ngsa-app, prometheus, grafana, fluentbit, jumpbox~~ Note: monitoring and fluentbit namespaces are not ready
   - `make lr-local`: LodeRunner, LodeRunner.API, LodeRunner.UI with Cosmos in Azure
   - `make lr-local-emul`: LodeRunner, LodeRunner.API, LodeRunner.UI with Cosmos Emulator
   - `make lr-local-emul-api-only`: Only LodeRunner.API with Cosmos Emulator
8. Set LodeRunner.API port visibility to public
   - In Codespaces, navigate to the `PORTS` terminal
   - Identify port `LodeRunner API (32088)` and right-click on the `Visibility`
   - Hover over `Port Visibility` and select `Public`

## Developing in Codespaces

For better development environment we have added:

- C\# Extension (OmniSharp) on create
- Test Explorer UI extension
- Editorconfig extension for `.editorconfig` support
- YAML Extension for deployment files
- Settings for several IDE features (e.g. `using` sort on save)
- Debuggger profile for LodeRunner.API and LodeRunner in `launch.json`

### Change C\# Project/Solution

Since we have multiple projects and solutions, intellisense will not work
if a file is not included the selected project. For example, if you open `src/LodeRunner/src/Program.cs`
file and your selected solution is `LodeRunner.API.sln`, then for obvious reason it won't have intellisense.

Hence, selecting correct project/solution is necessary.

To select a project/solution:

- There should already be a project selected in the `Status Bar` on the bottom of VSCode
- Clicking on the selected project should present a list of available projects

### To debug

- Go to `Run and Debug` tab on the left side of VSCode
- At the top of that pane, select the prjoect you want to debug from the dropdown
- Click the `Run` &#9658; button or press `F5` to start debugging

### To run unit/integration tests from GUI

- Setup: Add Cosmos Key to secrets and copy to local tmp directory

  ```bash
    # add cosmos key to secrets
    echo $(eval az cosmosdb keys list -n ngsa-asb-dev-cosmos -g rg-ngsa-asb-dev-cosmos --query primaryMasterKey -o tsv) > src/LodeRunner/secrets/CosmosKey

    # copy all secrets to /tmp/secrets
    cp -R src/LodeRunner/secrets /tmp/
  ```

- Click on `Testing` tab on the left side of VSCode.
  - Or on the status bar click on the `# tests` icon to go to the test explorer
- Click `Run` &#9658; button to start any test

### To run unit/integration tests from  command line

```bash
# cd into test directory
cd src/LodeRunner.API.Test  # or cd src/LodeRunner.Test if you want to run the LodeRunner tests and not LodeRunner.API

# add cosmos key to secrets
echo $(eval az cosmosdb keys list -n ngsa-asb-dev-cosmos -g rg-ngsa-asb-dev-cosmos --query primaryMasterKey -o tsv) > secrets/CosmosKey

# copy all secrets to /tmp/secrets
cp -R secrets /tmp/

# run all tests (in both src/LodeRunner.Test and src/LodeRunner.API.Test)
dotnet test

# run integration tests only (only in src/LodeRunner.API.Test))
dotnet test --filter='Category=Integration'

# run unit tests only (in both src/LodeRunner.Test and src/LodeRunner.API.Test)
dotnet test --filter='Category=Unit'
```

## Development of individual loderunner components

- [Running and debugging LodeRunner via Visual Studio 2019](./src/LodeRunner/README.md#running-and-debugging-loderunner-via-visual-studio-2019)
- [Running LodeRunner.API](./src/LodeRunner.API/README.md#running-the-api)
- [LodeRunner.UI Development Experience](./src/LodeRunner.UI/README.md#development-experience)

## Running an Example Load Test

- [Open LodeRunner UI](https://github.com/retaildevcrews/wcnp/blob/main/docs/ResourceList.md#loderunner-ui-endpoints)
- Create a Load Test Config
  - Select `+` next to the `Load Test Configs` header at the top of the page
  - Fill the `Name` input with an easily identifiable name
  - Fill the `Servers` input with [NGSA endpoints](https://github.com/retaildevcrews/wcnp/blob/main/docs/ResourceList.md#nsga-endpoints)
    - Take note of the cluster name (Pre, Dev)
  - Fill the `Names` input with filenames from [LodeRunner project](https://github.com/retaildevcrews/loderunner/tree/main/src/LodeRunner)
    - benchmark.json
    - baseline.json
  - Select `Save` at the bottom of the modal
- Create a Test Run
  - Select `LodeRunner Client Mode` clients from the left-hand panel
    - NOTE: the checkmark indicates a client has been selected
  - Select the `Play Icon` on the right-hand side of the created Load Test Config
  - Fill the `Name` input with an easily identifiable name
  - Select `Submit` at the bottom of the modal
  - Verify a successful message in the browser alert
- See completion status of Test Run
  - Navigate to Test Runs page
    - Select `See All Test Runs` from the left-hand panel, between clients and incomplete test runs
  - Select the `Pencil Icon` in created Test Run with the easily identifiable name
  - Read `Load Test Completion Time` and `Requests` to determine status of Test Run
  - Select the `Refresh Icon` at the top left-hand side of the page to view most up-to-date information
- View spike in Grafana dashboard
  - [Open Grafana](https://github.com/retaildevcrews/wcnp/blob/main/docs/ResourceList.md#dashboards)
    - NOTE: Open the Grafana that is in the same cluster as the NGSA endpoints (Pre, Dev)
  - Navigate to the NGSA Azure Monitor dashboard
    - Select the `Home` in the top left-hand corner
    - Select the `NGSA` folder
    - Select the `NGSA {DEV|PRE} - Azure Monitor` dashboard
  - Hover over the graph with cursor to find the Test Run start and complete time
  - Identify traffic spike
  - NOTE: can select the `Refresh Icon` on the top,right-hand side of the page to refresh graph
- View spike in Prometheus dashboard
  - [Open Grafana](https://github.com/retaildevcrews/wcnp/blob/main/docs/ResourceList.md#dashboards)
    - NOTE: Open the Grafana that is in the same cluster as the NGSA endpoints (Pre, Dev)
  - Navigate to the NGSA Prometheus dashboard
    - Select the `Home` in the top left-hand corner
    - Select the `NGSA` folder
    - Select the `NGSA {DEV|PRE} - Prometheus` dashboard
  - Select the appropriate `service` filter for the NGSA instance
  - Select `read` or `write` mode filter for the expected Loderunner operations based on the test files
  - Hover over the graph with cursor to find the Test Run start and complete time
  - Identify traffic spike
  - NOTE: can select the `Refresh Icon` on the top,right-hand side of the page to refresh graph
- View Test Run logs from LodeRunner in client mode
  - Open [Log Analytics](https://github.com/retaildevcrews/wcnp/blob/main/docs/ResourceList.md#shared-resources) in the same cluster the test run executed in (Pre, Dev)
  - Navigate to Logs by selecting `General` > `Logs` in the left-hand panel
  - Filter LodeRunner custom logs with the following query: `loderunner_CL
| where k_app_s == "loderunner-clientmode";`
  - Select the `Run` button to execute the query
  - NOTE: Logs include `ClientStatusId_g` and `LoadClientId_g` properties which map to LodeRunner client statud ID and load client Id, respectively
