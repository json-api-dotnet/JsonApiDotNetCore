#!/usr/bin/env bash

#exit if any command fails
set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

dotnet restore ./src/JsonApiDotNetCore/JsonApiDotNetCore.csproj
dotnet restore ./src/JsonApiDotNetCoreExample/JsonApiDotNetCoreExample.csproj
dotnet restore ./test/JsonApiDotNetCoreExampleTests/JsonApiDotNetCoreExampleTests.csproj

dotnet test ./test/JsonApiDotNetCoreExampleTests/JsonApiDotNetCoreExampleTests.csproj

revision=${TRAVIS_JOB_ID:=1}
revision=$(printf "%04d" $revision)

dotnet pack ./src/JsonApiDotNetCore -c Release -o ./artifacts --version-suffix=$revision