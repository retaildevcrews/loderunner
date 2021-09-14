#!/bin/sh

echo "on-create start" >> ~/status

# clone repos
pushd ..
git clone https://github.com/retaildevcrews/ngsa
git clone https://github.com/retaildevcrews/ngsa-app
git clone https://github.com/retaildevcrews/loderunner
popd

# install client dependencies
pushd client
npm install
popd

# setup deploy folder
mkdir -p deploy
cd deploy

for dir in ./*
do
  DIR=`echo $dir | sed 's/\.\///g'`
  DEPLOY="../../ngsa/IaC/DevCluster/${DIR}"

  if [ -d $DEPLOY ]
  then
    echo "Copying $DEPLOY to deploy directory..."
    cp -R $DEPLOY .
  fi
done

# remove unnecessary files in loderunner
rm -rf loderunner
mkdir -p loderunner
cp ../../ngsa/IaC/DevCluster/loderunner/loderunner.yaml loderunner

# create local yaml files
rm -rf ngsa-local
cp -R ngsa-memory/ ngsa-local
sed -i s@ghcr.io/retaildevcrews/ngsa-app:beta@k3d-registry.localhost:5000/ngsa-app:local@g ngsa-local/ngsa-memory.yaml

rm -rf loderunner-local
cp -R loderunner/ loderunner-local
sed -i s@ghcr.io/retaildevcrews/ngsa-lr:beta@k3d-registry.localhost:5000/ngsa-lr:local@g loderunner-local/loderunner.yaml

# copy grafana.db to /grafana
sudo mkdir -p /grafana
sudo  cp deploy/grafanadata/grafana.db /grafana
sudo  chown -R 472:472 /grafana

# create local registry
docker network create k3d
k3d registry create registry.localhost --port 5000
docker network connect k3d k3d-registry.localhost

echo "on-create complete" >> ~/status
