#!/bin/bash

# Use default dir for logs if not provided
[ -z "${LOG_PATH}" ] && LOG_PATH="/logs/"
# Error out if REGION is not provided
[ -z "${REGION}" ] && echo "REGION env variable is not set" && exit 1

LOG_FILE="${LOG_PATH}/log-$(date +'%Y-%m-%d').txt"
TIMESTAMP="$(date '+%Y-%m-%d-%H:%M:%S')"
COSMOS_NAME="LR API SOAK ${TIMESTAMP}"
ENDPOINT="https://loderunner-${REGION}-dev.cse.ms/api/api"

printf "%s\n{\"datetime\": \"$TIMESTAMP\",%s\n\"actions\": [" >> $LOG_FILE

make_api_call () {
    HTTP_CODE="${RES:${#RES}-3}"
    RES_BODY="${RES:0:${#RES}-3}"

    if [ $HTTP_CODE -eq $2 ]
    then
        printf "%s\n{\"event\": \"$1\", \"success\": \"true\", \"code\": $HTTP_CODE}" >> $LOG_FILE
    else
        printf "%s\n{\"event\": \"$1\", \"success\": \"false\", \"code\": \"$HTTP_CODE\", \"message\": $RES_BODY}%s\n]}" >> $LOG_FILE
        exit 0;
    fi

    BODY=$RES_BODY
}

# GET CLIENTS
RES=$(curl -sw '%{http_code}' $ENDPOINT/clients)
make_api_call "GET /clients" 200
printf "," >> $LOG_FILE

# FILTER BY READY, THEN TESTING, THEN STARTING
CLIENT_STATUSES=("Ready" "Testing" "Starting")

for status in "${CLIENT_STATUSES}"
do
    CLIENTS_AVAILABLE=$(echo $BODY | jq "map(select(.status == \"$status\"))")
    if (($(echo $CLIENTS_AVAILABLE | jq length) > 0))
    then
        CLIENT=$(echo $CLIENTS_AVAILABLE | jq '.[0]')
        break
    fi
done

# IF NO CLIENTS, LOG DID NOT EXECUTE TEST BECAUSE NO CLIENTS
if [[ -z "$CLIENT" ]]
then
    printf ",%s\n{\"event\": \"use LoadClient for POST /testruns\", \"success\": \"false\", \"code\": null, \"message\": \"Unable to find a LoadClient with a status of 'Ready', 'Testing', or 'Starting'\"}%s\n]}" >> $LOG_FILE
    exit 0
fi

# CREATE LOAD TEST CONFIG
CONFIG_PAYLOAD=$(cat loadtestconfig-payload.json | jq --arg name "$COSMOS_NAME" '.Name |= $name')
RES=$(curl -sw '%{http_code}' --data @loadtestconfig-payload.json --header "Content-Type: application/json" --header "Accept: application/json" $ENDPOINT/loadtestconfigs)
make_api_call "POST /loadtestconfigs" 201
printf "," >> $LOG_FILE

# GET LOAD TEST CONFIG
CONFIG_ID=$(echo $BODY | jq '.id' | tr -d '"')
RES=$(curl -sw '%{http_code}' $ENDPOINT/loadtestconfigs/$CONFIG_ID)
make_api_call "GET /loadtestconfigs/:id" 200
printf "," >> $LOG_FILE

# USE RETURNED PAYLOAD TO HELP CREATE TESTRUN PAYLOAD
TESTRUN_CONFIG=$(echo $BODY | jq --arg name "$COSMOS_NAME" '.Name |= $name')
CLIENT_ID=$(echo $CLIENT | jq '.loadClientId' | tr -d '"')
TESTRUN_CLIENT=$(echo $CLIENT | jq --arg id "$CLIENT_ID" '.id |= $id')
TESTRUN_PAYLOAD=$(cat testrun-payload.json | jq ".loadTestConfig = $TESTRUN_CONFIG" | jq ".loadClients[0] = ${TESTRUN_CLIENT}" | jq --arg name "$COSMOS_NAME" '.Name |= $name')

RES=$(curl -sw '%{http_code}' --data "$TESTRUN_PAYLOAD" --header "Content-Type: application/json" --header "Accept: application/json" $ENDPOINT/testruns)
make_api_call "POST /testruns" 201
printf "," >> $LOG_FILE

TESTRUN_ID=$(echo $BODY | jq '.id' | tr -d '"')

# CLEAN UP: DELETE CONFIG
RES=$(curl -sw '%{http_code}' -X DELETE $ENDPOINT/loadtestconfigs/$CONFIG_ID)
make_api_call "DELETE /loadtestconfigs/:id" 204
printf "," >> $LOG_FILE

# CLEAN UP: DELETE TESTRUN
RES=$(curl -sw '%{http_code}' -X DELETE $ENDPOINT/testruns/$TESTRUN_ID)
make_api_call "DELETE /testruns/:id" 204
printf "%s\n]}" >> $LOG_FILE
