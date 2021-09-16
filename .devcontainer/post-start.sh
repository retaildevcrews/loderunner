#!/bin/bash

echo "post-start start" >> ~/status

# this runs in background each time the container starts

# update the base docker images
docker pull mcr.microsoft.com/dotnet/sdk:5.0-alpine
docker pull mcr.microsoft.com/dotnet/aspnet:5.0-alpine
docker pull mcr.microsoft.com/vscode/devcontainers/javascript-node:14
docker pull nginx:stable
docker pull nginx:stable-alpine

echo "post-start complete" >> ~/status
