#!/bin/bash

# job control
# set -m
if [[ -z $DOMAIN_NAME_GLOB || \
    # Throw errors if 3 vars are not set
    # If not all of them are provided
    -z $NGINX_SSL_CRT || \
    -z $NGINX_SSL_KEY ]]; then
    echo "Provide all three env vars (DOMAIN_NAME_GLOB, NGINX_SSL_CRT, NGINX_SSL_KEY)"
    exit 1
fi

# Write the nginx certs to proper location
# Copy the crt and key from NGINX_SSL_CRT and NGINX_SSL_KEY to /config/nginx.crt and nginx.key respectively
__nginx_ssl_crt_path=/config/nginx.crt
__nginx_ssl_key_path=/config/nginx.key
echo "${NGINX_SSL_CRT}" > ${__nginx_ssl_crt_path}
echo "${NGINX_SSL_KEY}" > ${__nginx_ssl_key_path}

# Get possible COSMOS Port from AZURE_COSMOS_EMULATOR_ARGS
if [[ -z $AZURE_COSMOS_EMULATOR_ARGS ]];then
    # If not provided use the default one
    __cosmos_port=8081
else
    __cosmos_port=$(echo $AZURE_COSMOS_EMULATOR_ARGS | grep -oE "/Port=[0-9]+" |  awk -F'Port=' '{print $2}')
fi

echo "Cosmos Port: $__cosmos_port"
sed -e "s#\${__cosmos_port}#${__cosmos_port}#g" \
    -e "s#\${DOMAIN_NAME_GLOB}#${DOMAIN_NAME_GLOB}#g" \
    -e "s#\${__nginx_ssl_crt_path}#${__nginx_ssl_crt_path}#g" \
    -e "s#\${__nginx_ssl_key_path}#${__nginx_ssl_key_path}#g" /config/nginx-http-template.conf > /config/nginx-http.conf 

# timeout=240
# while ! test $(cat /tmp/output.log | grep -vE 'Started ' | grep -E '^Started' | head -n 1);do sleep 5;timeout=$((timeout-5)); [[ $timeout == 0 ]] && echo "Cosmos emulator is stalled. Exiting." && exit 1; done

# sleep 10
# Start Nginx in the background
nginx -c /config/nginx-http.conf
echo "Starting cosmos emulator"
./start.sh # | tee /tmp/output.log &
# fg
