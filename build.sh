#!/bin/bash
set -e

dotnet tool restore
dotnet run --project src/build -- "$@"
