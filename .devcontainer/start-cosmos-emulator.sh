#!/bin/bash

# 1. CosmosDB Emulator
EMUL_NAME="${COSMOS_EMUL_NAME:-cosmos-linux-emulator}"
EMUL_DATA_PATH="${COSMOS_EMUL_DATA_PATH:-/workspaces/cosmos-emulator/data}"
EMUL_URL="${EMUL_NAME}.documents.azure.com"

mkdir -p ${EMUL_DATA_PATH}

ipaddr="`ifconfig | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1`"
echo "Emulator Data Path ${EMUL_DATA_PATH}"

## See https://docs.microsoft.com/en-us/azure/cosmos-db/linux-emulator?tabs=ssl-netstd21#run-on-linux
## It will take a minute to finish
docker run -itd --restart always -v "${EMUL_DATA_PATH}":/tmp/cosmos/appdata -p 9090:9090 -p 10251:10251 -p 10252:10252 -p 10253:10253 -p 10254:10254 -m 3g --cpus=2.0 --name=${EMUL_NAME} -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10 -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true -e AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=${ipaddr} -e AZURE_COSMOS_EMULATOR_ARGS='/enablepreview /Port=9090' mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator

# 2. Setup nginx
NGINX_CONF_PATH=$(dirname "${EMUL_DATA_PATH}")/nginx/
mkdir -p ${NGINX_CONF_PATH}
cp $(dirname $0)/cosmos-emul.conf ${NGINX_CONF_PATH}/

## First add cosmos-emulator.documents.azure.com as a host DNS
cat /etc/hosts | grep "documents.azure.com" || echo "127.0.0.1  ${EMUL_NAME}.documents.azure.com" | sudo tee -a /etc/hosts

## Generate keys for NGINX
openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes \
  -keyout "${NGINX_CONF_PATH}/nginx.key" -out "${NGINX_CONF_PATH}/nginx.crt" -subj "/CN=${EMUL_URL}" -addext "subjectAltName=DNS:${EMUL_URL},DNS:${EMUL_URL},IP:127.0.0.1"

## Copy the crt file to trusted-local-cert-db
sudo cp ${NGINX_CONF_PATH}/nginx.crt /usr/local/share/ca-certificates/
## Update the cert db
sudo update-ca-certificates

docker run --restart=always -itd --name nginx-cosmos-proxy --network=host -v ${NGINX_CONF_PATH}:/config --entrypoint nginx nginx:alpine -c /config/cosmos-emul.conf
