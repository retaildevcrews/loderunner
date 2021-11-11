# Fluent Bit Setup

Setup Fluent Bit on a dev cluster by sending everything to stdout and then to Azure Log Analytics

```bash
# start in the repo directory
cd deploy/fluentbit

# create fluentbit namespace
kubectl apply -f namespace.yaml

### Create secrets if necessary
###   fluentbit won't run without these secrets
###   skip this step if already set
kubectl create secret generic fluentbit-secrets \
  --namespace=fluentbit \
  --from-literal=WorkspaceId=dev \
  --from-literal=SharedKey=dev

# create the fluentbit service account and cluster role binding
kubectl apply -f role.yaml

# apply env vars
kubectl apply -f config-log.yaml

# apply fluentbit config to log to stdout
kubectl apply -f config.yaml

# deploy ngsa app
kubectl apply -f ../ngsa

# check pods
kubectl get pods -A

# wait for pod to show Running
kubectl logs -n ngsa -l app=ngsa-memory -c app

# start fluentbit pod
kubectl apply -f fluentbit-pod.yaml

# check pods
kubectl get pods -A

# wait for pod to show Running
kubectl logs -n fluentbit -l app.kubernetes.io/name=fluentbit

# save the cluster IP
export ngsa=http://$(kubectl get service -n ngsa ngsa-memory -o jsonpath="{.spec.clusterIP}"):30080

# check the version and genres endpoints
http $ngsa/version
http $ngsa/api/genres

# check the logs again
kubectl logs -n fluentbit -l app.kubernetes.io/name=fluentbit

# delete fluentb
kubectl delete -f fluentbit-pod.yaml

# delete ngsa-memory
kubectl delete -f ../ngsa/ngsa-memory.yaml

# check pods
kubectl get pods -A

# Result - No resources found in fluentbit and ngsa namespace.

```

## Test sending to Log Analytics

### Login to Azure

```bash

# login to Azure
az login

az account list -o table

# select subscription (if necesary)
az account set -s YourSubscriptionName

```
