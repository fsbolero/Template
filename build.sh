#!/bin/bash
set -e

EXE_EXT=
if [ "$OS" = "Windows_NT" ]; then EXE_EXT=.exe; fi

if ! [ -f ".paket/fake$EXE_EXT" ]; then dotnet tool install fake-cli --tool-path .paket; fi
if ! [ -f ".paket/paket$EXE_EXT" ]; then dotnet tool install paket --tool-path .paket --version 5.189.1; fi

PATH="~/.dotnet:$PATH"
.paket/fake build "$@"
