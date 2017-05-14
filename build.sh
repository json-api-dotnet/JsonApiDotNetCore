#!/usr/bin/env bash

#exit if any command fails
set -e

dotnet restore ./src/JsonApiDotNetCore/JsonApiDotNetCore.csproj
dotnet restore ./src/JsonApiDotNetCoreExample/JsonApiDotNetCoreExample.csproj
dotnet restore ./test/JsonApiDotNetCoreExampleTests/JsonApiDotNetCoreExampleTests.csproj
dotnet restore ./test/NoEntityFrameworkTests/NoEntityFrameworkTests.csproj

dotnet test ./test/UnitTests/UnitTests.csproj
dotnet test ./test/JsonApiDotNetCoreExampleTests/JsonApiDotNetCoreExampleTests.csproj
dotnet test ./test/NoEntityFrameworkTests/NoEntityFrameworkTests.csproj