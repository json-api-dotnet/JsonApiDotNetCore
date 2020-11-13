#Requires -Version 7.0

# This script starts a docker container with postgres database, used for running tests.

docker container stop jsonapi-dotnet-core-testing

docker run --rm --name jsonapi-dotnet-core-testing       `
 -e POSTGRES_DB=JsonApiDotNetCoreExample                 `
 -e POSTGRES_USER=postgres                               `
 -e POSTGRES_PASSWORD=postgres                           `
 -p 5432:5432                                            `
 postgres:12.0
