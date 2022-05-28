#!/bin/sh

echo "on-create start" >> ~/status

# Permanently change shell to zsh for vscode user
sudo chsh --shell /bin/zsh vscode

# create local registry
docker network create k3d
k3d registry create registry.localhost --port 5000
docker network connect k3d k3d-registry.localhost

echo "on-create complete" >> ~/status
