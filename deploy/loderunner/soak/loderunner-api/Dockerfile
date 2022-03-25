FROM ghcr.io/cse-labs/jumpbox:latest
WORKDIR /app

COPY ["requests.sh", "loadtestconfig-payload.json", "testrun-payload.json", "./"]

ENTRYPOINT ["./requests.sh"]