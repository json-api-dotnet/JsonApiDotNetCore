#!/usr/bin/env bash

docker run --rm --name jsonapi-dotnet-core-testing \
 -e POSTGRES_DB=JsonApiDotNetCoreExample \
 -e POSTGRES_USER=postgres \
 -e POSTGRES_PASSWORD=postgres \
 -p 5432:5432 \
 postgres
