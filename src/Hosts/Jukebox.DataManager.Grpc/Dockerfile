# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG GITHUB_TOKEN
WORKDIR /src

# Copy solution and project files for layer-cached restore
COPY Jukebox.DataManager.slnx ./
COPY src/Hosts/Jukebox.DataManager.Grpc/Jukebox.DataManager.Grpc.csproj src/Hosts/Jukebox.DataManager.Grpc/
COPY src/Manager/Jukebox.DataManager.Managers/Jukebox.DataManager.Managers.csproj src/Manager/Jukebox.DataManager.Managers/
COPY src/Manager/Jukebox.DataManager.Contracts/Jukebox.DataManager.Contracts.csproj src/Manager/Jukebox.DataManager.Contracts/

# Configure GitHub Packages NuGet source and restore
RUN dotnet nuget add source https://nuget.pkg.github.com/jonwong84/index.json \
      --name github \
      --username jonwong84 \
      --password $GITHUB_TOKEN \
      --store-password-in-clear-text \
  && dotnet restore src/Hosts/Jukebox.DataManager.Grpc/Jukebox.DataManager.Grpc.csproj

# Copy remaining source and publish
COPY src/ src/
RUN dotnet publish src/Hosts/Jukebox.DataManager.Grpc/Jukebox.DataManager.Grpc.csproj \
      --no-restore \
      --configuration Release \
      --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

# gRPC ports (matches appsettings.Development.json)
EXPOSE 5037
EXPOSE 7109

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Jukebox.DataManager.Grpc.dll"]
