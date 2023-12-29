#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SqlSyncDbService/src/SqlSyncDbService/SqlSyncDbService.csproj", "SqlSyncDbService/src/SqlSyncDbService/"]
RUN dotnet restore "SqlSyncDbService/src/SqlSyncDbService/SqlSyncDbService.csproj"
COPY . .
WORKDIR "/src/SqlSyncDbService/src/SqlSyncDbService"
RUN dotnet build "SqlSyncDbService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SqlSyncDbService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SqlSyncDbService.dll"]