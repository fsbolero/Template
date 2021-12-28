#!/bin/bash
set -e

dotnet tool restore
PAKET_SKIP_RESTORE_TARGETS=true dotnet fake run --fsiargs --define:UTILITY_FROM_PAKET build.fsx "$@"
