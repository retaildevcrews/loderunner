# this image is built and updated weekly
# https://github.com/retaildevcrews/codespaces-images
FROM ghcr.io/cse-labs/k3d:latest

# some images require specific values
ARG USERNAME=vscode
#checkov:skip=CKV_DOCKER_2: No healthcheck is needed 
#checkov:skip=CKV_DOCKER_3: No user is needed 
#checkov:skip=CKV_DOCKER_7: Alright to use "latest" tag

# [Optional] Uncomment this section to install additional OS packages.
# alternatively, you can add commands to on-create.sh and/or post-create.sh
# on-create runs before the Codespaces UI appears
# post-create runs in the background after the UI appears
# RUN apt-get -y install --no-install-recommends <your-package-list-here>

# upgrade packages
# you can improve startup time by running in post-create.sh
# RUN apt upgrade -
