# LodeRunner (Client Mode), LodeRunner.API, and LodeRunner.UI

## Local DevCluster Setup

```bash

# Update REACT_APP_SERVER value in src/LodeRunner.UI/.env.production
REACT_APP_SERVER=https://{GH_USERNAME}-{GH_ORG}-{GH_REPO}-{GH_CODESPACESID}-32088.githubpreview.dev

### Create secrets if necessary
###   loderunner won't run without these secrets
###   skip these step if already set

### set env variable
export LR_COL=
export LR_DB=
export LR_KEY=
export LR_URL=

### From the repo, save env variables
./deploy/loderunner/local/saveenv.sh

### If terminal environment gets cleared, reload env variables
source ~/.lr.env

# create the loderunner (client mode) deployment and service
kubectl apply -f ./deploy/loderunner/local
```

In Ports, update the LodeRunner.API visibility to "Public".

## Local LodeRunner.UI Development (not using DevCluster)

```bash
# Update REACT_APP_SERVER value in src/LodeRunner.UI/.env.development
REACT_APP_SERVER=https://{GH_USERNAME}-{GH_ORG}-{GH_REPO}-{GH_CODESPACESID}-32088.githubpreview.dev

# Install LodeRunner.UI Dependencies
cd src/LodeRunner.UI
npm install

# Start LodeRunner.UI locally
npm start
```
