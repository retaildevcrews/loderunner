#!/usr/bin/env sh

set -e # error
set -m # jobs

[ -z $LRAPI_DNS ] && echo "LRAPI_DNS variable is not set" && exit 1

for i in $(grep -rl '$LRAPI_DNS' /usr/share/nginx/html/*);
do
    envsubst '$LRAPI_DNS' < $i > /tmp/envsubst
    mv /tmp/envsubst $i
done

nginx -g "daemon off;"
