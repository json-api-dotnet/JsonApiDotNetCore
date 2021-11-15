#!/bin/bash

# Determine the IP address of the local machine.
# This step is required when Direct mode setting is configured using Cosmos DB SDKs.
ipaddr="`ifconfig | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1`"

# Run the image, creating a container called "azure-cosmos-emulator-linux".
docker run \
  -p 8081:8081 \
  -p 10251:10251 \
  -p 10252:10252 \
  -p 10253:10253 \
  -p 10254:10254 \
  -m 3g \
  --cpus=2.0 \
  --name=azure-cosmos-emulator-linux \
  -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=3 \
  -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true \
  -e AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=$ipaddr \
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator
