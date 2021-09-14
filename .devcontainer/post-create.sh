#!/bin/bash

echo "post-create start" >> ~/status

# this runs in background after UI is available

# (optional) upgrade packages
#sudo apt-get update
sudo apt-get upgrade -y
#sudo apt-get autoremove -y
#sudo apt-get clean -y

# get install script and install node
# [Choice] Node.js version: 16, 14, 12
ARG VARIANT=14
RUN curl -sL https://deb.nodesource.com/setup_${VARIANT}.x | sudo -E bash - && \
    sudo apt-get install -y nodejs && \
    sudo apt-get install -y npm

echo "post-create complete" >> ~/status
