#!/usr/bin/env bash

#exit if any command fails
set -e

dotnet restore
dotnet build -c Release
dotnet test -c Release --no-build
