# LodeRunner

TODO: Add LodeRunner Overview and link to subfolders for project specific readme files

## Prerequisites

- Bash shell
  - Tested on: Visual Studio Codespaces, Mac, Ubuntu, Windows with WSL2)
  - Not supported: WSL1 or Cloud Shell
- Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Docker CLI ([download](https://docs.docker.com/install/))
- .NET 5.0 ([download](https://docs.microsoft.com/en-us/dotnet/core/install/))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Running the System

```bash
make all
```

This will...

- Use k3d to start a new cluster
- Create a local docker registry
- Start with the ngsa-app in in-memory mode as the load test application
- Start with loderunner as the load test client
- Start with a local build of relayrunner backend to manage load test configs and activities
- Start with a local build of relayrunner client to display load test configs and activites
- Start monitoring: prometheus, grafana (not configured yet)
- Start fluentbit
- Start a jumpbox
- Check the available endpoints
