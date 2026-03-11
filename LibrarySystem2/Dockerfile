# --- Build stage ---------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopiera allt till build-miljön
COPY . .

# Restore + Publish
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# --- Runtime stage -------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Kopiera publicerade filer från build-steget
COPY --from=build /app/publish .

# Render använder port 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Starta appen
ENTRYPOINT ["dotnet", "LibrarySystem2.dll"]
