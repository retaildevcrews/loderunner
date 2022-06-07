#!/bin/bash

set -m # Turn on Bash Job control
echo "post-create start" >> ~/status

# this runs in background after UI is available

# (optional) upgrade packages
sudo apt-get update
# sudo apt-get upgrade -y
#sudo apt-get autoremove -y
#sudo apt-get clean -y

# start CosmosDB Emulator & setup nginx
echo "  build CosmosDB Emulator & nginx" | tee -a ~/status
sudo ./$(dirname $0)/cosmos-emulator/start-cosmos-emulator.sh

# get install script and install node
# [Choice] Node.js version: 16, 14, 12
echo "  Install NodeJS" | tee -a ~/status
VARIANT=14
curl -sL https://deb.nodesource.com/setup_${VARIANT}.x | sudo -E bash -
sudo apt-get install -y nodejs

# install client dependencies
pushd src/LodeRunner.UI
echo "  Install NodeJS packages for LodeRunner.UI" | tee -a ~/status
npm install
detach
popd

# install Azure Cosmos SDK
pip3 install azure-cosmos
# wait on the CosmosDB Emulator script
echo "post-create complete" >> ~/status
