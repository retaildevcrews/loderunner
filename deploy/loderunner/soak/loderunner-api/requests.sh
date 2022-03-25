#!/bin/bash 
LOG_FILE="log-$(date +'%Y-%m-%d').txt"
TIMESTAMP="$(date '+%Y-%m-%d')-$(date '+%H:%M:%S')"
COSMOS_NAME="LR API SOAK ${TIMESTAMP}"
REGION="eastus"
ENDPOINT="https://loderunner-$REGION-dev.cse.ms/api/api"

printf ",%s\n{\"datetime\": $TIMESTAMP" >> $LOG_FILE

# GET CLIENTS
RES=$(curl -sw '%{http_code}' $ENDPOINT/clients)
HTTP_CODE="${RES:${#RES}-3}"

if [ $HTTP_CODE -eq 200 ]
then
    printf ",%s\n\"actions\": [%s\n{\"event\": \"GET /clients\", \"success\": \"true\", \"code\": $HTTP_CODE}" >> $LOG_FILE
else
    printf ",%s\n\"actions\": [%s\n{\"event\": \"GET /clients\", \"success\": \"false\", \"code\": \"$HTTP_CODE\", \"message\": ${RES:0:${#RES}-3}}%s\n]}" >> $LOG_FILE
    exit 0;
fi

printf "${RES:0:${#RES}-3}" > clients.json

# FILTER BY READY, THEN TESTING, THEN STARTING
CLIENT_STATUSES=("Ready" "Testing" "Starting")

for status in "${CLIENT_STATUSES}"
do
    CLIENTS_AVAILABLE=$(cat clients.json | jq "map(select(.status == \"$status\"))")
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
HTTP_CODE="${RES:${#RES}-3}"

if [ $HTTP_CODE -eq 201 ]
then
    printf ",%s\n{\"event\": \"POST /loadtestconfigs\", \"success\": \"true\", \"code\": $HTTP_CODE}" >> $LOG_FILE
else
    printf ",%s\n{\"event\": \"POST /loadtestconfigs\", \"success\": \"false\", \"code\": \"$HTTP_CODE\", \"message\": ${RES:0:${#RES}-3}}%s\n]}" >> $LOG_FILE
    exit 0;
fi

printf "${RES:0:${#RES}-3}" > loadtestconfig.json

# GET LOAD TEST CONFIG
CONFIG_ID=$(cat loadtestconfig.json | jq '.id' | tr -d '"')
RES=$(curl -sw '%{http_code}' $ENDPOINT/loadtestconfigs/$CONFIG_ID)
HTTP_CODE="${RES:${#RES}-3}"

if [ $HTTP_CODE -eq 200 ]
then
    printf ",%s\n{\"event\": \"GET /loadtestconfigs/:id\", \"success\": \"true\", \"code\": $HTTP_CODE}" >> $LOG_FILE
else
    printf ",%s\n{\"event\": \"GET /loadtestconfigs/:id\", \"success\": \"false\", \"code\": \"$HTTP_CODE\", \"message\": ${RES:0:${#RES}-3}}%s\n]}" >> $LOG_FILE
    exit 0;
fi

printf "${RES:0:${#RES}-3}" > loadtestconfig.json

# USE RETURNED PAYLOAD TO HELP CREATE TESTRUN PAYLOAD
TESTRUN_CONFIG=$(cat loadtestconfig.json | jq --arg name "$COSMOS_NAME" '.Name |= $name')
CLIENT_ID=$(echo $CLIENT | jq '.loadClientId' | tr -d '"')
TESTRUN_CLIENT=$(echo $CLIENT | jq --arg id "$CLIENT_ID" '.id |= $id')
TESTRUN_PAYLOAD=$(cat testrun-payload.json | jq ".loadTestConfig = $TESTRUN_CONFIG" | jq ".loadClients[0] = ${TESTRUN_CLIENT}" | jq --arg name "$COSMOS_NAME" '.Name |= $name')

RES=$(curl -sw '%{http_code}' --data "$TESTRUN_PAYLOAD" --header "Content-Type: application/json" --header "Accept: application/json" $ENDPOINT/testruns)
HTTP_CODE="${RES:${#RES}-3}"
#curl -o testrun.json --write-out '%{json}' --data "$TESTRUN_PAYLOAD" --header "Content-Type: application/json" --header "Accept: application/json" $ENDPOINT/testruns >> $LOG_FILE

if [ $HTTP_CODE -eq 201 ]
then
    printf ",%s\n{\"event\": \"POST /testruns\", \"success\": \"true\", \"code\": $HTTP_CODE}" >> $LOG_FILE
else
    printf ",%s\n{\"event\": \"POST /testruns\", \"success\": \"false\", \"code\": \"$HTTP_CODE\", \"message\": ${RES:0:${#RES}-3}}%s\n]}" >> $LOG_FILE
    exit 0;
fi

printf "${RES:0:${#RES}-3}" > testrun.json

# CLEAN UP: DELETE CONFIG
RES=$(curl -sw '%{http_code}' -X DELETE $ENDPOINT/loadtestconfigs/$CONFIG_ID)
HTTP_CODE="${RES:${#RES}-3}"

if [ $HTTP_CODE -eq 204 ]
then
    printf ",%s\n{\"event\": \"DELETE /loadtestconfigs/:id\", \"success\": \"true\", \"code\": $HTTP_CODE}" >> $LOG_FILE
else
    printf ",%s\n{\"event\": \"DELETE /loadtestconfigs/:id\", \"success\": \"false\", \"code\": \"$HTTP_CODE\", \"message\": ${RES:0:${#RES}-3}}%s\n]}" >> $LOG_FILE
    exit 0;
fi

# CLEAN UP: DELETE TESTRUN
TESTRUN_ID=$(cat testrun.json | jq '.id' | tr -d '"')
RES=$(curl -sw '%{http_code}' -X DELETE $ENDPOINT/testruns/$TESTRUN_ID)
HTTP_CODE="${RES:${#RES}-3}"

if [ $HTTP_CODE -eq 204 ]
then
    printf ",%s\n{\"event\": \"DELETE /testruns/:id\", \"success\": \"true\", \"code\": $HTTP_CODE}%s\n]}" >> $LOG_FILE
else
    printf ",%s\n{\"event\": \"DELETE /testruns/:id\", \"success\": \"false\", \"code\": \"$HTTP_CODE\", \"message\": ${RES:0:${#RES}-3}}%s\n]}" >> $LOG_FILE
    exit 0;
fi
