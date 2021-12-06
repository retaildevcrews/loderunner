# loderunner

- [LodeRunner](./LodeRunner/README.md) is the load client that waits for TestRun entries in CosmosDB, and execute those load tests
- [LodeRunner.API](./LodeRunner.API/README.md) is a service that creates LoadTestConfig and TestRun entries in CosmosDB for LodeRunner to use in order to execute load tests
- [LodeRunner.UI](./LodeRunner.UI/README.md) utilizes the LodeRunner.API endpoints to provide a user-friendly interface to create LoadTestConfigs and TestRun entries in CosmosDB

**NOTE** All components of the loderunner ecosystem have additional functionality that are not listed here

## Prerequisites

- Bash shell
  - Tested on: Visual Studio Codespaces, Mac, Ubuntu, Windows with WSL2)
  - Not supported: WSL1 or Cloud Shell
- Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Docker CLI ([download](https://docs.docker.com/install/))
- .NET 5.0 ([download](https://docs.microsoft.com/en-us/dotnet/core/install/))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Running the System via Codespaces

1. Open codespaces in https://github.com/retaildevcrews/loderunner
2. Verify in the loderunner directory `pwd`
3. Set `src/LodeRunner.UI/.env.production` REACT_APP_SERVER to LodeRunner.API URL
   - To prevent accidental commits `git update-index --assume-unchanged .env.production`
4. Set environmental variables for K8S generic secret
   - Set `LR_COL` (collection), `LR_DB`, `LR_KEY`, and `LR_URL` with CosmosDB values
   - Note: `LR_KEY` needs Read-Write permissions
5. Save environmental variables for future re-run via `./deploy/loderunner/local/saveenv.sh`
6. Start the k3d cluster `make create`
7. Deploy pods
    - `make all`: LodeRunner, LodeRunner.API, LodeRunner.UI, ngsa-app, prometheus, grafana, fluentbit, jumpbox
    - `make lr-local`: LodeRunner, LodeRunner.API, LodeRunner.UI
8. In ports, set LodeRunner.API port visibility to public

## Development of a loderunner Component

- [LodeRunner Unning and Debugging LodeRunner via Visual Studio 2019](./src/LodeRunner/README.md#running-and-debugging-loderunner-via-visual-studio-2019)
- [LodeRunner.API Running the API](./src/LodeRunner.API/README.md#running-the-api)
- [LodeRunner.UI Development Experience](./src/LodeRunner.UI/README.md#development-experience)
