#!/usr/bin/env python3

# -*- coding: utf-8 -*-
import subprocess
import sys

from azure.cosmos import CosmosClient, PartitionKey, exceptions
import argparse
import urllib3
from contextlib import contextmanager

@contextmanager
def disable_ssl_warnings():
    import warnings
    import urllib3

    with warnings.catch_warnings():
        urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
        yield None

"""
The code below disables SSL Cert verification
It's only meant to be used with Cosmos Emulator
Careful when using it with Azure Cosmos in production
"""  

def arg_parser():
    parser = argparse.ArgumentParser("cosmos-emulator-init")
    parser.add_argument("--key", "-k", required=True)
    parser.add_argument("--url", "-u", required=True)
    parser.add_argument("--emulate","-e", default=False, action="store_true")
    return parser

args = arg_parser().parse_args()

KEY=args.key
URL=args.url
connection_verify=not args.emulate

# For local emulator
if connection_verify is False:
    
    warning="BEAWARE: SSL verification and warnings are disabled for emulator"
    print(warning,file=sys.stderr)
    urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

client = CosmosClient(URL, credential=KEY, connection_verify=connection_verify)

# Create LodeRunner db and container
# CosmosDB values should be in sync with cosmos-create.sh
db_cont = {"LodeRunnerDB": "LodeRunner", "LodeRunnerTestDB": "LodeRunner"}

for db_name in db_cont:
    cont_name = db_cont[db_name]
    try:
        db = client.create_database(id=db_name)
        cont = db.create_container(
            id=cont_name, partition_key=PartitionKey(path="/partitionKey")
        )
        print(f"Created DB: {db_name}, Container: {cont_name}")
    except exceptions.CosmosResourceExistsError:
        print(f'{db_name}/{cont_name} Already exists')
    except :
        raise
