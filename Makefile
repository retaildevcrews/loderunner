.PHONY: help all create delete deploy check clean lr-local reset-prometheus reset-grafana jumpbox

help :
	@echo "Usage:"
	@echo "   make all              - create a cluster and deploy the apps"
	@echo "   make create           - create a kind cluster"
	@echo "   make delete           - delete the kind cluster"
	@echo "   make deploy           - deploy the apps to the cluster"
	@echo "   make check            - curl endpoints cluster"
	@echo "   make clean            - delete deployments"
	@echo "   make lr-local         - build and deploy all local loderunner images"
	@echo "   make reset-prometheus - reset the Prometheus volume (existing data is deleted)"
	@echo "   make reset-grafana    - reset the Grafana volume (existing data is deleted)"
	@echo "   make jumpbox          - deploy a 'jumpbox' pod"

all : create deploy jumpbox check

delete :
	# delete the cluster (if exists)
	@# this will fail harmlessly if the cluster does not exist
	-k3d cluster delete

create : delete
	# create the cluster and wait for ready
	@# this will fail harmlessly if the cluster exists
	@# default cluster name is k3d

	@k3d cluster create --registry-use k3d-registry.localhost:5000 --config deploy/k3d.yaml --k3s-server-arg "--no-deploy=traefik" --k3s-server-arg "--no-deploy=servicelb"

	# wait for cluster to be ready
	@kubectl wait node --for condition=ready --all --timeout=60s
	@sleep 5
	@kubectl wait pod -A --all --for condition=ready --timeout=60s

deploy :
	# deploy ngsa-app
	@# continue on most errors
	-kubectl apply -f deploy/ngsa

	# Delete LodeRunner.UI node_modules for docker context
	@rm -rf ./src/LodeRunner.UI/node_modules

	# Create LodeRunner image from codebase
	@docker build ./src -t k3d-registry.localhost:5000/ngsa-lr:local -f ./src/LodeRunner/Dockerfile
	# Load new LodeRunner image to k3d registry
	@docker push k3d-registry.localhost:5000/ngsa-lr:local

	# Create LodeRunner.API image from codebase
	@docker build ./src -t k3d-registry.localhost:5000/loderunner-api:local -f ./src/LodeRunner.API/Dockerfile
	# Load new LodeRunner.API image to k3d registry
	@docker push k3d-registry.localhost:5000/loderunner-api:local

	# Create LodeRunner.UI image from codebase
	@docker build ./src/LodeRunner.UI -t k3d-registry.localhost:5000/loderunner-ui:local
	# Load new client image to k3d registry
	@docker push k3d-registry.localhost:5000/loderunner-ui:local

	# deploy local LodeRunner, LodeRunner.API, and LodeRunner.UI after the ngsa-app starts
	@kubectl wait pod -n ngsa --all --for condition=ready --timeout=30s
	-kubectl apply -f deploy/loderunner/local/1-namespace.yaml
	-kubectl create secret generic lr-secrets --namespace=loderunner --from-literal=CosmosCollection=${LR_COL} --from-literal=CosmosDatabase=${LR_DB} --from-literal=CosmosKey=${LR_KEY} --from-literal=CosmosUrl=${LR_URL}
	-kubectl apply -f deploy/loderunner/local

	# deploy prometheus and grafana
	-kubectl apply -f deploy/monitoring
	
	# deploy fluentbit
	-kubectl apply -f deploy/fluentbit/namespace.yaml
	-kubectl create secret generic fluentbit-secrets --namespace fluentbit --from-literal=WorkspaceId=dev --from-literal=SharedKey=dev
	-kubectl apply -f deploy/fluentbit/role.yaml
	-kubectl apply -f deploy/fluentbit/config-log.yaml
	-kubectl apply -f deploy/fluentbit/config.yaml
	-kubectl apply -f deploy/fluentbit/fluentbit-pod.yaml

	# wait for pods to start
	@kubectl wait pod -A --all --for condition=ready --timeout=100s

	# display pod status
	-kubectl get po -A | grep -v "kube-system"

check :
	# curl all of the endpoints
	@echo "\nchecking ngsa-memory..."
	@curl localhost:30080/version
	@echo "\n\nchecking prometheus..."
	@curl localhost:30000
	@echo "\nchecking grafana..."
	@curl localhost:32000
	@echo "\nchecking loderunner ui..."
	@curl localhost:32080
	@echo "\n\nchecking loderunner api..."
	@curl localhost:32088/version

clean :
	# delete the deployment
	@# continue on error
	-kubectl delete -f deploy/loderunner/local --ignore-not-found=true
	-kubectl delete -f deploy/loderunner/local-local --ignore-not-found=true
	-kubectl delete -f deploy/loderunner --ignore-not-found=true
	-kubectl delete secret lr-secrets --namespace loderunner --ignore-not-found=true
	-kubectl delete -f deploy/ngsa --ignore-not-found=true
	-kubectl delete -f deploy/monitoring --ignore-not-found=true
	-kubectl delete ns monitoring --ignore-not-found=true
	-kubectl delete -f deploy/fluentbit/fluentbit-pod.yaml --ignore-not-found=true
	-kubectl delete -f deploy/fluentbit/config.yaml --ignore-not-found=true
	-kubectl delete -f deploy/fluentbit/config-log.yaml --ignore-not-found=true
	-kubectl delete -f deploy/fluentbit/role.yaml --ignore-not-found=true
	-kubectl delete -f deploy/fluentbit/namespace.yaml --ignore-not-found=true
	-kubectl delete secret fluentbit-secrets --namespace fluentbit --ignore-not-found=true

	# show running pods
	@kubectl get po -A

lr-local-emul-api-only:
	# Create LodeRunner.API image from codebase
	@#docker build ./src -t k3d-registry.localhost:5000/loderunner-api:local -f ./src/LodeRunner.API/Dockerfile
	# Load new LodeRunner.API image to k3d registry
	@#docker push k3d-registry.localhost:5000/loderunner-api:local

	# Delete previous deployed LodeRunner apps
	-kubectl delete -f deploy/loderunner/local-comos/3-loderunner-api.yaml --ignore-not-found=true

	-kubectl apply -f deploy/loderunner/local-cosmos/1-namespace.yaml
	# Create lr-secrets for Cosmos Emulator
	-kubectl delete secret lr-secrets cosmos-emul-secrets -n loderunner --ignore-not-found=true
	@kubectl create secret generic lr-secrets -n loderunner --from-literal=CosmosCollection=${LR_COL} --from-literal=CosmosDatabase=${LR_DB} --from-literal=CosmosKey=${shell docker top ${COSMOS_EMULATOR_NAME} | grep  -oP '/Key=(\w.*) '| head -n 1 | awk -F' ' '{print $$1}' | awk -F 'Key=' '{print $$2}'} --from-literal=CosmosUrl=https://host.k3d.internal

	# Create NGINX secrets for Cosmos Emulator
	@kubectl create secret generic cosmos-emul-secrets --namespace=loderunner --from-file=nginx_cosmos_cert=${NGINX_CONFIG_PATH}/cosmos-linux-emulator.crt --from-file=cosmos_emul_cert=${NGINX_CONFIG_PATH}/emulatorcert.crt
	# Deploy loderunner API with Cosmos Emulator Secrets
	-kubectl apply -f deploy/loderunner/local-cosmos/3-loderunner-api.yaml

lr-local-emul:
	# Create LodeRunner image from codebase
	@docker build ./src -t k3d-registry.localhost:5000/ngsa-lr:local -f ./src/LodeRunner/Dockerfile
	# Load new LodeRunner image to k3d registry
	@docker push k3d-registry.localhost:5000/ngsa-lr:local

	# Create LodeRunner.API image from codebase
	@docker build ./src -t k3d-registry.localhost:5000/loderunner-api:local -f ./src/LodeRunner.API/Dockerfile
	# Load new LodeRunner.API image to k3d registry
	@docker push k3d-registry.localhost:5000/loderunner-api:local

	# Create LodeRunner.UI image from codebase
	@docker build ./src/LodeRunner.UI -t k3d-registry.localhost:5000/loderunner-ui:local
	# Load new client image to k3d registry
	@docker push k3d-registry.localhost:5000/loderunner-ui:local

	# Delete previous deployed LodeRunner apps
	-kubectl delete -f deploy/loderunner --ignore-not-found=true
	-kubectl delete -f deploy/loderunner/local-cosmos --ignore-not-found=true

	# deploy local LodeRunner, LodeRunner.API, and LodeRunner.UI
	-kubectl apply -f deploy/loderunner/local-cosmos/1-namespace.yaml
	# Create LodeRunner and NGNX for Cosmos Emulator
	@kubectl create secret generic lr-secrets --namespace=loderunner --from-literal=CosmosCollection=${LR_COL} --from-literal=CosmosDatabase=${LR_DB} --from-literal=CosmosKey=${shell docker top ${COSMOS_EMULATOR_NAME} | grep  -oP '/Key=(\w.*) '| head -n 1 | awk -F' ' '{print $$1}' | awk -F 'Key=' '{print $$2}'} --from-literal=CosmosUrl=https://host.k3d.internal
	@kubectl create secret generic cosmos-emul-secrets --namespace=loderunner --from-file=nginx_cosmos_cert=${NGINX_CONFIG_PATH}/cosmos-linux-emulator.crt --from-file=cosmos_emul_cert=${NGINX_CONFIG_PATH}/emulatorcert.crt
	-kubectl apply -f deploy/loderunner/local-cosmos

	# wait for pod to be ready
	@sleep 5
	@kubectl wait pod -n loderunner --all --for condition=ready --timeout=60s

	@kubectl get po -n loderunner

	# display the current LodeRunner.API version
	-http localhost:32088/version
	# hit the LodeRunner.UI endpoint
	-http -h localhost:32080

lr-local:
	# Create LodeRunner image from codebase
	@docker build ./src -t k3d-registry.localhost:5000/ngsa-lr:local -f ./src/LodeRunner/Dockerfile
	# Load new LodeRunner image to k3d registry
	@docker push k3d-registry.localhost:5000/ngsa-lr:local

	# Create LodeRunner.API image from codebase
	@docker build ./src -t k3d-registry.localhost:5000/loderunner-api:local -f ./src/LodeRunner.API/Dockerfile
	# Load new LodeRunner.API image to k3d registry
	@docker push k3d-registry.localhost:5000/loderunner-api:local

	# Create LodeRunner.UI image from codebase
	@docker build ./src/LodeRunner.UI -t k3d-registry.localhost:5000/loderunner-ui:local
	# Load new client image to k3d registry
	@docker push k3d-registry.localhost:5000/loderunner-ui:local

	# Delete previous deployed LodeRunner apps
	-kubectl delete -f deploy/loderunner --ignore-not-found=true
	-kubectl delete -f deploy/loderunner/local --ignore-not-found=true

	# deploy local LodeRunner, LodeRunner.API, and LodeRunner.UI
	-kubectl apply -f deploy/loderunner/local/1-namespace.yaml
	-kubectl create secret generic lr-secrets --namespace=loderunner --from-literal=CosmosCollection=${LR_COL} --from-literal=CosmosDatabase=${LR_DB} --from-literal=CosmosKey=${LR_KEY} --from-literal=CosmosUrl=${LR_URL}
	-kubectl apply -f deploy/loderunner/local

	# wait for pod to be ready
	@sleep 5
	@kubectl wait pod -n loderunner --all --for condition=ready --timeout=60s

	@kubectl get po -n loderunner

	# display the current LodeRunner.API version
	-http localhost:32088/version
	# hit the LodeRunner.UI endpoint
	-http -h localhost:32080

reset-prometheus :
	# remove and create the /prometheus volume
	@sudo rm -rf /prometheus
	@sudo mkdir -p /prometheus
	@sudo chown -R 65534:65534 /prometheus

reset-grafana :
	# remove and copy the data to /grafana volume
	@sudo rm -rf /grafana
	@sudo mkdir -p /grafana
	@sudo cp -R deploy/grafanadata/grafana.db /grafana
	@sudo chown -R 472:472 /grafana

jumpbox :
	# start a jumpbox pod
	@-kubectl delete pod jumpbox --ignore-not-found=true

	@kubectl run jumpbox --image=ghcr.io/retaildevcrews/alpine --restart=Always -- /bin/sh -c "trap : TERM INT; sleep 9999999999d & wait"
	@kubectl wait pod jumpbox --for condition=ready --timeout=30s

	# Run an interactive bash shell in the jumpbox
	# kj
	# use kje <command>
	# kje http ngsa-memory:8080/version
