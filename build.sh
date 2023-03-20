#!/bin/bash
set -e

dotnet tool restore
dotnet run --project .build -- "$@"
