#!/usr/bin/env python3

# -*- coding: utf-8 -*-
import argparse
from azure.cosmos import CosmosClient, PartitionKey, exceptions
import sys
import urllib3

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
connection_verify=True

# For local emulator
if args.emulate is True:
    
    warning="BEAWARE: SSL verification and warnings are disabled for emulator"
    
    print(warning,file=sys.stderr)
    connection_verify=False
    urllib3.disable_warnings()

client = CosmosClient(URL, credential=KEY, connection_verify=connection_verify)

# Create LodeRunner db and container
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
