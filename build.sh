#!/bin/bash
set -e

cd project
PATH="~/.dotnet:$PATH"
dotnet restore
dotnet fake build "$@"
cd ..
