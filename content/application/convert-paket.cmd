dotnet tool install paket.install --tool-path .paket
if errorlevel 1 exit /b %errorlevel%

.paket\paket.install -fw "netstandard2.0, netcoreapp3.0"
if errorlevel 1 exit /b %errorlevel%

REM //#if (nightly)
echo source https://ci.appveyor.com/nuget/bolero >>paket.dependencies
REM //#endif

.paket\paket convert-from-nuget --force
if errorlevel 1 exit /b %errorlevel%

del .\convert-paket.cmd
