#!/bin/bash

# cosmosDB Emulator Setup
COSMOS_EMULATOR_NAME="${COSMOS_EMULATOR_NAME:-cosmos-linux-emulator}"
COSMOS_EMULATOR_URL="${COSMOS_EMULATOR_NAME}.documents.azure.com"

# wait for the cosmos emulator to start
echo "Waiting for CosmosDB Emulator to start"
while ! test $(docker logs ${COSMOS_EMULATOR_NAME} | grep -vE 'Started ' | grep -E '^Started' | head -n 1);do sleep 5;done
echo "CosmosDB Emulator started"

# update SSL Certs
# copy Self-Signed nginx certs to trusted ssl cert dir
sudo cp ${NGINX_CONFIG_PATH}/nginx.crt /usr/local/share/ca-certificates/

# copy the Cosmos Emulator self-signed cert to trusted ssl cert dir
curl -k "https://${COSMOS_EMULATOR_URL}/_explorer/emulator.pem" > /tmp/emulatorcert.crt
sudo cp /tmp/emulatorcert.crt /usr/local/share/ca-certificates/
# update the CA Certs DB
sudo update-ca-certificates

# add cosmos-emulator.documents.azure.com as a host DNS
cat /etc/hosts | grep "documents.azure.com" || echo "127.0.0.1  ${COSMOS_EMULATOR_URL}" | sudo tee -a /etc/hosts
