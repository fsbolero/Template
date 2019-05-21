#!/bin/bash
set -e

EXE_EXT=
if [ "$OS" = "Windows_NT" ]; then EXE_EXT=.exe; fi

if ! [ -f ".paket/fake$EXE_EXT" ]; then dotnet tool install fake-cli --tool-path .paket; fi
if ! [ -f ".paket/nbgv$EXE_EXT" ]; then dotnet tool install nbgv --tool-path .paket; fi
if ! [ -f ".paket/paket$EXE_EXT" ]; then dotnet tool install paket --tool-path .paket --version 5.206.0

PATH="~/.dotnet:$PATH"
.paket/fake build "$@"
