#!/bin/bash

echo "post-start start" >> ~/status

# this runs in background each time the container starts

# update the base docker images
docker pull mcr.microsoft.com/dotnet/sdk:5.0-alpine
docker pull mcr.microsoft.com/dotnet/aspnet:5.0-alpine
docker pull mcr.microsoft.com/vscode/devcontainers/javascript-node:14
docker pull nginx:stable
docker pull nginx:stable-alpine
COSMOS_EMUL_NAME="${COSMOS_EMUL_NAME:-cosmos-linux-emulator}"

# Create LodeRunnerDB and LodeRunnerTestDB containers
wait_on_cosmos_emulator() {
    echo "Waiting on CosmosDB Emulator"
    while ! test $(docker logs ${COSMOS_EMUL_NAME} | grep -vE 'Started ' | grep 'Started');do sleep 5;done
}
wait_on_cosmos_emulator
export COSMOS_KEY_CMD="docker top ${COSMOS_EMUL_NAME} |grep  -oP '\/Key=(\w.*) '|head -n 1 | awk -F 'Key=' '{print \$2}'"
echo "Populating CosmosDB with LodeRunner DB and Container"
# Install the Azure Cosmos SDK
$(dirname $0)/cosmosdb-emulator/cosmos-emulator-init.py -k $(eval $COSMOS_KEY_CMD) -u https://localhost:9090 --emulate

echo "post-start complete" >> ~/status
