#!/bin/sh

dotnet tool install paket.install --tool-path .paket

.paket/paket.install -fw "netstandard2.0, netcoreapp3.0"

# //#if (nightly)
echo 'source https://ci.appveyor.com/nuget/bolero' >>paket.dependencies
# //#endif

.paket/paket convert-from-nuget --force

rm ./convert-paket.sh
