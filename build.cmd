@echo off

cd project

dotnet restore
if errorlevel 1 exit /b %errorlevel%

dotnet fake build %*
if errorlevel 1 exit /b %errorlevel%

cd ..
