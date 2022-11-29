#!/bin/sh

echo "on-create start" >> ~/status

# Permanently change shell to zsh for vscode user
sudo chsh --shell /bin/zsh vscode

# create local registry
docker network create k3d
k3d registry create registry.localhost --port 5000
docker network connect k3d k3d-registry.localhost

# update packages
sudo apt-get update

# Install dotnet-sdk-6.0, since by default sdk is dotnet-sdk-7.0
sudo apt install dotnet-sdk-6.0

# start CosmosDB Emulator & setup nginx
echo "  Build CosmosDB Emulator & nginx" | tee -a ~/status
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

# Setup omnisharp global configuration
mkdir -p $HOME/.omnisharp
ln -s /workspaces/loderunner/omnisharp.json $HOME/.omnisharp

echo "on-create complete" >> ~/status
