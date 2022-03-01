#!/bin/bash

echo "post-start start" >> ~/status

# this runs in background each time the container starts

# update the base docker images
docker pull mcr.microsoft.com/dotnet/sdk:5.0-alpine
docker pull mcr.microsoft.com/dotnet/aspnet:5.0-alpine
docker pull mcr.microsoft.com/vscode/devcontainers/javascript-node:14
docker pull nginx:stable
docker pull nginx:stable-alpine

COSMOS_EMULATOR_NAME="${COSMOS_EMULATOR_NAME:-cosmos-linux-emulator}"
COSMOS_EMULATOR_URL="${COSMOS_EMULATOR_NAME}.documents.azure.com"

# update certs and host files for CosmosDB Emulator
source $(dirname $0)/cosmos-emulator/setup-cosmos-emulator.sh 

export COSMOS_KEY_CMD="docker top ${COSMOS_EMULATOR_NAME} |grep  -oP '\/Key=(\w.*) '|head -n 1 | awk -F' ' '{print \$1}' | awk -F 'Key=' '{print \$2}'"
echo "Populating CosmosDB with LodeRunner DB and Container"
# create LodeRunnerDB and LodsdoeRunnerTestDB containers
python3 $(dirname $0)/cosmos-emulator/cosmos-emulator-init.py -k $(eval $COSMOS_KEY_CMD) -u "https://${COSMOS_EMULATOR_URL}" --emulate

echo "post-start complete" >> ~/status
