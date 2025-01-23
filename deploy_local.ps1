dotnet publish -c Release --self-contained -r linux-x64 ./src/mvc_server/mvc_server.csproj
scp -r .\src\mvc_server\bin\Release\net8.0\linux-x64\publish yura@homeserver:HOMESERVER/latest/