#!/bin/bash

# cosmosDB Emulator Setup
COSMOS_EMULATOR_NAME="${COSMOS_EMULATOR_NAME:-cosmos-linux-emulator}"
COSMOS_EMULATOR_URL="${COSMOS_EMULATOR_NAME}.documents.azure.com"
NGINX_CONFIG_PATH="${NGINX_CONFIG_PATH:-/workspaces/cosmos-emulator/nginx}"
SSL_CERT_DIR="${NGINX_CONFIG_PATH}/ssl"

mkdir -p "${SSL_CERT_DIR}"

echo_tee() { echo -e "$@" | tee -a ~/status; }

# wait for the cosmos emulator to start
echo_tee "  Waiting for CosmosDB Emulator to start"
timeout=120
while ! test $(docker logs ${COSMOS_EMULATOR_NAME} | grep -vE 'Started ' | grep -E '^Started' | head -n 1)
do
    printf "\rStarting cosmos emulator (timeout remaining: ${timeout}s)"
    sleep 2
    timeout=$((timeout-2))
    [[ $timeout == 0 ]] && break
done
[[ $timeout == 0 ]] && echo_tee "\n  Warning: Cosmos emulator might be stalled." || echo_tee "\n  CosmosDB emulator started"


# # add cosmos-emulator.documents.azure.com as a host DNS
cat /etc/hosts | grep "documents.azure.com" || echo "127.0.0.1  ${COSMOS_EMULATOR_URL}" | sudo tee -a /etc/hosts

# Copy emulator cert to ssl dir
if [[ ! -f ${NGINX_CONFIG_PATH}/emulatorcert.crt || "$(cat ${NGINX_CONFIG_PATH}/emulatorcert.crt | grep 'CERTIFICATE')" == '' ]]; then
    sleep 5
    echo_tee "  Emulator Cert doesn't exist"
    curl -k "https://127.0.0.1:9090/_explorer/emulator.pem" >| "${NGINX_CONFIG_PATH}/emulatorcert.crt"
fi

cp "${NGINX_CONFIG_PATH}/${COSMOS_EMULATOR_NAME}.crt" "${SSL_CERT_DIR}/${COSMOS_EMULATOR_NAME}.pem"
cp "${NGINX_CONFIG_PATH}/emulatorcert.crt" "${SSL_CERT_DIR}/emulatorcert.pem"
