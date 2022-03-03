#!/bin/bash

# execute this script to switch LodeRunner applications to use Cosmos Emulator as Data Source

COSMOS_EMULATOR_URL="${COSMOS_EMULATOR_NAME}.documents.azure.com"
COSMOS_KEY_CMD="docker top ${COSMOS_EMULATOR_NAME} |grep  -oP '\/Key=(\w.*) '|head -n 1 | awk -F' ' '{print \$1}' | awk -F 'Key=' '{print \$2}'"
echo "Updating LodeRunner secrets to run with emulator"

echo  "https://$COSMOS_EMULATOR_URL" > src/LodeRunner/secrets/CosmosUrl
echo "$(eval $COSMOS_KEY_CMD)" > src/LodeRunner/secrets/CosmosKey

echo "https://$COSMOS_EMULATOR_URL" > src/LodeRunner.API/secrets/CosmosUrl
echo "$(eval $COSMOS_KEY_CMD)" > src/LodeRunner.API/secrets/CosmosKey

### updating certs and host files again since post-start.sh doesn't kick off on restart
### remove once postStartCommand work as expected
source .devcontainer/cosmos-emulator/setup-cosmos-emulator.sh
