#!/bin/bash
set -e

dotnet tool restore
dotnet fake run --fsiargs --define:UTILITY_FROM_PAKET build.fsx "$@"
