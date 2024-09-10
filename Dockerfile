# Verwende ein offizielles .NET SDK-Image zum Bauen der App
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Kopiere die Projektdateien und restore .NET Abhängigkeiten
COPY . ./
RUN dotnet restore

# Baue das Projekt
RUN dotnet publish -c Release -o /out

# Verwende ein schlankeres .NET-Runtime-Image zum Ausführen
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Starte die Anwendung
ENTRYPOINT ["dotnet", "ChatServer.dll"]
