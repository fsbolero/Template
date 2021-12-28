@echo off

dotnet tool restore
set PAKET_SKIP_RESTORE_TARGETS=true
dotnet fake run --fsiargs --define:UTILITY_FROM_PAKET build.fsx %*
