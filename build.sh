#!/usr/bin/env bash

#exit if any command fails
set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

dotnet restore

dotnet test ./test/JsonApiDotNetCoreExampleTests -c Release -f netcoreapp1.0

revision=${TRAVIS_JOB_ID:=1}
revision=$(printf "%04d" $revision)

dotnet pack ./src/JsonApiDotNetCore -c Release -o ./artifacts --version-suffix=$revision