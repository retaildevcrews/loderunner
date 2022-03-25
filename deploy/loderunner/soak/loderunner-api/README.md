# Apply Cron Jobs to K8s Cluster

- First deploy configmap:

```bash
# Regions westus2, eastus
export REGION=westus2; export LOG_PATH=/logs/
# Substitute variables and apply
envsubst < soak-config.yaml | kubectl apply -f -
```

- Then apply soak cron job

```bash
kubectl apply -f soak-lrapi.yaml
```

With this there should be a cronjob in `loderunner` namespace.

It also creates a jumpbox which we can use to debug/capture logs.
