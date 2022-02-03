#!/bin/bash

set -m # Turn on Bash Job control
echo "post-create start" >> ~/status

# this runs in background after UI is available

# (optional) upgrade packages
sudo apt-get update
sudo apt-get upgrade -y
#sudo apt-get autoremove -y
#sudo apt-get clean -y

# CosmosDB Emulator
COSMOS_EMUL_NAME="${COSMOS_EMUL_NAME:-cosmos-linux-emulator}"
COSMOS_EMUL_DATA_PATH="${COSMOS_EMUL_DATA_PATH:-/workspaces/cosmos-emulator-data}"
mkdir -p ${COSMOS_EMUL_DATA_PATH}

# It will take a minute to finish
ipaddr="`ifconfig | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1`"
echo "Emulator Data Path ${COSMOS_EMUL_DATA_PATH}"

# See https://docs.microsoft.com/en-us/azure/cosmos-db/linux-emulator?tabs=ssl-netstd21#run-on-linux
# TODO: Push docker image with modified cosmos-docker-start.sh script
docker run --rmd -it --restart always -v "${COSMOS_EMUL_DATA_PATH}":/tmp/cosmos/appdata -p 9090:8081 -p 10251:10251 -p 10252:10252 -p 10253:10253 -p 10254:10254 -m 3g --cpus=2.0 --name=${COSMOS_EMUL_NAME} -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10 -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true -e AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=${ipaddr}  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator >/dev/null &

# get install script and install node
# [Choice] Node.js version: 16, 14, 12
VARIANT=14
curl -sL https://deb.nodesource.com/setup_${VARIANT}.x | sudo -E bash -
sudo apt-get install -y nodejs

# install client dependencies
pushd src/LodeRunner.UI
npm install
popd

# Install Azure Cosmos SDK
pip3 install azure-cosmos
# Wait on the CosmosDB Emulator docker
fg

echo "post-create complete" >> ~/status
