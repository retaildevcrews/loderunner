#!/bin/bash

echo "post-start start" >> ~/status

# this runs in background each time the container starts

# update the base docker images
docker pull mcr.microsoft.com/dotnet/sdk:5.0-alpine
docker pull mcr.microsoft.com/dotnet/aspnet:5.0-alpine
docker pull mcr.microsoft.com/vscode/devcontainers/javascript-node:14
docker pull nginx:stable
docker pull nginx:stable-alpine

# CosmosDB Emulator Setup
COSMOS_EMUL_NAME="${COSMOS_EMUL_NAME:-cosmos-linux-emulator}"
EMUL_URL="${COSMOS_EMUL_NAME}.documents.azure.com"

## Wait for the cosmos emulator to start
echo "Waiting for CosmosDB Emulator to start"
while ! test $(docker logs ${COSMOS_EMUL_NAME} | grep -vE 'Started ' | grep 'Started');do sleep 5;done
echo "CosmosDB Emulator started"

# Update SSL Certs
## First Copy Self-Signed nginx certs to trusted ssl cert dir
sudo cp ${NGINX_CONF_PATH}/nginx.crt /usr/local/share/ca-certificates/

# Copy the Cosmos Emulator self-signed cert to trusted ssl cert dir
curl -k "https://${EMUL_URL}/_explorer/emulator.pem" > /tmp/emulatorcert.crt
sudo cp /tmp/emulatorcert.crt /usr/local/share/ca-certificates/
# Update the CA Certs DB
sudo update-ca-certificates

## First add cosmos-emulator.documents.azure.com as a host DNS
cat /etc/hosts | grep "documents.azure.com" || echo "127.0.0.1  ${EMUL_URL}" | sudo tee -a /etc/hosts

export COSMOS_KEY_CMD="docker top ${COSMOS_EMUL_NAME} |grep  -oP '\/Key=(\w.*) '|head -n 1 | awk -F' ' '{print \$1}' | awk -F 'Key=' '{print \$2}'"
echo "Populating CosmosDB with LodeRunner DB and Container"
# Create LodeRunnerDB and LodsdoeRunnerTestDB containers
python3 $(dirname $0)/cosmos-emulator-init.py -k $(eval $COSMOS_KEY_CMD) -u "https://${EMUL_URL}" --emulate

echo "post-start complete" >> ~/status
