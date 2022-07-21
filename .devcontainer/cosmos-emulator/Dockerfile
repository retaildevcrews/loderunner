FROM mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator as base-cosmos
#checkov:skip=CKV_DOCKER_2: No healthcheck is needed 
#checkov:skip=CKV_DOCKER_3: No user is needed 
#checkov:skip=CKV_DOCKER_7: Alright to use "latest" tag
#checkov:skip=CKV_DOCKER_9: APT runs with -y option

WORKDIR /usr/local/bin/cosmos/

RUN apt update -y \
    && apt install -y nginx --no-install-recommends\
    && apt clean -y \
    && apt autoremove --yes \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /config

# COPY cosmos-emul.conf /usr/local/bin/cosmos/
COPY pre-startup.bash /usr/local/bin/cosmos/
COPY nginx-http-template.conf /config/

ENTRYPOINT [ "./pre-startup.bash" ]
