FROM mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator as base-cosmos

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
