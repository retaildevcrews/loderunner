#!/bin/bash

# Use default dir for logs if not provided
[ -z "${LOG_PATH}" ] && LOG_PATH="/tmp/logs/" && mkdir -p ${LOG_PATH}

LOG_FILE="${LOG_PATH}/log-$(date +'%Y-%m-%d').txt"
TIMESTAMP="$(date '+%Y-%m-%d-%H:%M:%S')"
NGSA_ENDPOINT="https://ngsa-memory-westus2-dev.cse.ms/version"

printf "%s{\"datetime\": \"$TIMESTAMP\",%s\"actions\": [" > $LOG_FILE

# CALL NGSA BURST FILTER
RES=$(curl -sI $NGSA_ENDPOINT -o out.txt)
HTTP_CODE=$(cat out.txt | head -n 1 | tr -d '\r\n')
HEADERS=$(cat out.txt | grep x-load-feedback | tr -d '\r\n')
if [[ -z "$HEADERS" ]]
then
    printf '{"event": "GET '$NGSA_ENDPOINT'", "success": "false", "code": "%s", "message": "cannot find burstheader on ngsa-mem-westus2"}' "$HTTP_CODE" >> $LOG_FILE
else
    printf '{"event": "GET '$NGSA_ENDPOINT'", "success": "true", "code": "%s", "message": "%s"}' "$HTTP_CODE" "$HEADERS" >> $LOG_FILE
fi

printf "%s]}" >> $LOG_FILE

cat $LOG_FILE
