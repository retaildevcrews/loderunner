#!/bin/bash

# set Cosmos Emulator Properties
COSMOS_EMULATOR_NAME="${COSMOS_EMULATOR_NAME:-cosmos-linux-emulator}"
COSMOS_EMULATOR_DATA_PATH="${COSMOS_EMULATOR_DATA_PATH:-/workspaces/cosmos-emulator/data}"
COSMOS_EMULATOR_URL="${COSMOS_EMULATOR_NAME}.documents.azure.com"
NGINX_CONFIG_PATH="${NGINX_CONFIG_PATH:-/workspaces/cosmos-emulator/nginx}"

cwd="$( dirname -- "${BASH_SOURCE[0]:-$0}" )"
mkdir -p ${COSMOS_EMULATOR_DATA_PATH}
mkdir -p ${NGINX_CONFIG_PATH}

ipaddr="`ifconfig | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1`"
echo "Emulator data path: ${COSMOS_EMULATOR_DATA_PATH}"

# Build Cosmos Nginx emulator
docker build -t nginx-cosmos-emulator "${cwd}" -f "${cwd}"/Dockerfile

# Generate Cert for three urls including internal k3d network
"${cwd}/gen-multi-domain-cert.bash" \
    -san "${COSMOS_EMULATOR_URL},host.k3d.internal,localhost" \
    --cert-path "${NGINX_CONFIG_PATH}" --cert-prefix "${COSMOS_EMULATOR_NAME}"

# cp "${NGINX_CONFIG_PATH}/nginx_cosmos.crt" "${NGINX_CONFIG_PATH}/nginx_cosmos.pem"

# add <cosmos-emulator>.documents.azure.com as a host DNS
cat /etc/hosts | grep "documents.azure.com" || echo "127.0.0.1  ${COSMOS_EMULATOR_NAME}.documents.azure.com" | sudo tee -a /etc/hosts

# see https://docs.microsoft.com/en-us/azure/cosmos-db/linux-emulator?tabs=ssl-netstd21#run-on-linux
# it will take a minute to finish
docker run -itd --restart=always -m 3g --cpus=2.0 \
  -v "${COSMOS_EMULATOR_DATA_PATH}":/tmp/cosmos/appdata \
  --name="${COSMOS_EMULATOR_NAME}" -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10 \
  -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true \
  -e AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE="${ipaddr}" \
  -e AZURE_COSMOS_EMULATOR_ARGS='/enablepreview /Port=9090' \
  -e DOMAIN_NAME_GLOB='*.documents.azure.com' \
  -e NGINX_SSL_CRT="$(cat ${NGINX_CONFIG_PATH}/${COSMOS_EMULATOR_NAME}.crt )" \
  -e NGINX_SSL_KEY="$(cat ${NGINX_CONFIG_PATH}/${COSMOS_EMULATOR_NAME}.key)" \
  -p 443:443 -p 9090:9090 -p 10251:10251 -p 10252:10252 \
  -p 10253:10253 -p 10254:10254 nginx-cosmos-emulator
