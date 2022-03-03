#!/bin/bash

# set Cosmos Emulator Properties
COSMOS_EMULATOR_NAME="${COSMOS_EMULATOR_NAME:-cosmos-linux-emulator}"
COSMOS_EMULATOR_DATA_PATH="${COSMOS_EMULATOR_DATA_PATH:-/workspaces/cosmos-emulator/data}"
COSMOS_EMULATOR_URL="${COSMOS_EMULATOR_NAME}.documents.azure.com"
NGINX_CONFIG_PATH="${NGINX_CONFIG_PATH:-/workspaces/cosmos-emulator/nginx}"

mkdir -p ${COSMOS_EMULATOR_DATA_PATH}

ipaddr="`ifconfig | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1`"
echo "Emulator Data Path ${COSMOS_EMULATOR_DATA_PATH}"

# see https://docs.microsoft.com/en-us/azure/cosmos-db/linux-emulator?tabs=ssl-netstd21#run-on-linux
# it will take a minute to finish
docker run -itd --restart always -v /workspaces/loderunner/workspaces/cosmos-emulator/data:/tmp/cosmos/appdata -p 9090:9090 -p 10251:10251 -p 10252:10252 -p 10253:10253 -p 10254:10254 -m 3g --cpus=2.0 --name=${COSMOS_EMULATOR_NAME} -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10 -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true -e AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=${ipaddr} -e AZURE_COSMOS_EMULATOR_ARGS='/enablepreview /Port=9090' mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator >/dev/null &

# setup nginx
mkdir -p ${NGINX_CONFIG_PATH}
cp $(dirname $0)/cosmos-emulator/cosmos-emul.conf ${NGINX_CONFIG_PATH}/

# add <cosmos-emulator>.documents.azure.com as a host DNS
cat /etc/hosts | grep "documents.azure.com" || echo "127.0.0.1  ${COSMOS_EMULATOR_NAME}.documents.azure.com" | sudo tee -a /etc/hosts

# generate keys for NGINX
openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes \
  -keyout "${NGINX_CONFIG_PATH}/nginx.key" -out "${NGINX_CONFIG_PATH}/nginx.crt" -subj "/CN=${COSMOS_EMULATOR_URL}" -addext "subjectAltName=DNS:${COSMOS_EMULATOR_URL},DNS:${COSMOS_EMULATOR_URL},IP:127.0.0.1"

# copy the crt file to trusted-local-cert-db
sudo cp ${NGINX_CONFIG_PATH}/nginx.crt /usr/local/share/ca-certificates/
# update the cert db
sudo update-ca-certificates

docker run --restart=always -itd --name nginx-cosmos-proxy --network=host -v /workspaces/loderunner/workspaces/cosmos-emulator/nginx:/config --entrypoint nginx nginx:alpine -c /config/cosmos-emul.conf
