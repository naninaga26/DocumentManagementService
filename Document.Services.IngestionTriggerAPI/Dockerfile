FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Document.Services.IngestionTriggerAPI/Document.Services.IngestionTriggerAPI.csproj", "Document.Services.IngestionTriggerAPI/"]
RUN dotnet restore "Document.Services.IngestionTriggerAPI/Document.Services.IngestionTriggerAPI.csproj"
COPY . .
WORKDIR "/src/Document.Services.IngestionTriggerAPI"
RUN dotnet build "./Document.Services.IngestionTriggerAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Document.Services.IngestionTriggerAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Document.Services.IngestionTriggerAPI.dll"]
