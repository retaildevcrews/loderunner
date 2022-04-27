#!/bin/bash
sudo rm /usr/local/share/ca-certificates/${COSMOS_EMULATOR_URL}.* /usr/share/ca-certificates/${COSMOS_EMULATOR_URL}.* /etc/ssl/certs/${COSMOS_EMULATOR_URL}.*

sudo update-ca-certificates

openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes -keyout "${COSMOS_EMULATOR_URL}.key" -out "${COSMOS_EMULATOR_URL}.crt" -subj "/CN=${COSMOS_EMULATOR_URL}" -addext "subjectAltName=DNS:${COSMOS_EMULATOR_URL},DNS:localhost,DNS:${COSMOS_EMULATOR_URL}"

echo Copying CRT to ca-certificates
sudo cp ${COSMOS_EMULATOR_URL}.crt /usr/local/share/ca-certificates/
sudo cp ${COSMOS_EMULATOR_URL}.crt /usr/share/ca-certificates/

echo Updating ca-certificates
sudo update-ca-certificates

