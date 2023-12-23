
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source


# copy csproj and restore as distinct layers
COPY ../src/SqlSyncLib.sln /source
COPY ../src/SqlSyncDbService/*.csproj /source/SqlSyncDbService/
RUN dotnet sln /source/SqlSyncLib.sln remove /source/SqlSyncDbService.Tests/SqlSyncDbService.Tests.csproj
RUN dotnet restore 

# copy everything else and build app
COPY ../src/SqlSyncDbService /source/SqlSyncDbService/
WORKDIR /source/SqlSyncDbService
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "SqlSyncDbService.dll"]