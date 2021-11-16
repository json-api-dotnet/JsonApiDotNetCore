#!/bin/bash

# Run the Docker image that was previously pulled from the Docker repository, creating a container called
# "azure-cosmos-emulator-linux". Do not (!) set
#
#   AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=$ipaddr
#
# as suggested in Microsoft's documentation at https://docs.microsoft.com/en-us/azure/cosmos-db/linux-emulator.
# We would not be able to connect to the emulator at all on appveyor, regardless of the connection mode.
# To connect to the emulator, we must use Gateway mode. Direct mode will not work.

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
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator
