#!/bin/sh
dotnet publish -c Release --self-contained -r linux-x64 ./src/mvc_server/mvc_server.csproj
scp -r ./src/mvc_server/bin/Release/net9.0/linux-x64/publish yura@10.42.0.1:HOMESERVER/latest/
