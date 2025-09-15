FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TwinApp.ProjectService.API/TwinApp.ProjectService.API.csproj", "TwinApp.ProjectService.API/"]
RUN dotnet restore "TwinApp.ProjectService.API/TwinApp.ProjectService.API.csproj"
COPY . .
WORKDIR "/src/TwinApp.ProjectService.API"
RUN dotnet build "./TwinApp.ProjectService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TwinApp.ProjectService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TwinApp.ProjectService.API.dll"]
