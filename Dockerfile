# Verwende ein offizielles .NET 8.0 SDK-Image zum Bauen der App
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopiere die Projektdateien und restore .NET Abhängigkeiten
COPY . ./
RUN dotnet restore

# Baue das Projekt
RUN dotnet publish -c Release -o /out

# Verwende ein offizielles .NET 8.0 Runtime-Image zum Ausführen der App
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Starte die Anwendung
ENTRYPOINT ["dotnet", "Chat Server.dll"]
