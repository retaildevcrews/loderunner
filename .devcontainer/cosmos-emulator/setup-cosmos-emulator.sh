#!/bin/bash

# CosmosDB Emulator Setup
COSMOS_EMULATOR_NAME="${COSMOS_EMULATOR_NAME:-cosmos-linux-emulator}"
COSMOS_EMULATOR_URL="${COSMOS_EMULATOR_NAME}.documents.azure.com"

# Update SSL Certs
## Copy Self-Signed nginx certs to trusted ssl cert dir
sudo cp ${NGINX_CONFIG_PATH}/nginx.crt /usr/local/share/ca-certificates/

# Copy the Cosmos Emulator self-signed cert to trusted ssl cert dir
curl -k "https://${COSMOS_EMULATOR_URL}/_explorer/emulator.pem" > /tmp/emulatorcert.crt
sudo cp /tmp/emulatorcert.crt /usr/local/share/ca-certificates/
# Update the CA Certs DB
sudo update-ca-certificates

# Add cosmos-emulator.documents.azure.com as a host DNS
cat /etc/hosts | grep "documents.azure.com" || echo "127.0.0.1  ${COSMOS_EMULATOR_URL}" | sudo tee -a /etc/hosts
