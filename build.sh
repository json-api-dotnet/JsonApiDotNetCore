#!/usr/bin/env bash

#exit if any command fails
set -e

dotnet restore

dotnet test ./test/UnitTests/UnitTests.csproj
dotnet test ./test/JsonApiDotNetCoreExampleTests/JsonApiDotNetCoreExampleTests.csproj
dotnet test ./test/NoEntityFrameworkTests/NoEntityFrameworkTests.csproj
# dotnet test ./test/OperationsExampleTests/OperationsExampleTests.csproj
# dotnet test ./test/ResourceEntitySeparationExampleTests/ResourceEntitySeparationExampleTests.csproj
