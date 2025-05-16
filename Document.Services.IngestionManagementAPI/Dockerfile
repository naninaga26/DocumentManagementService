FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Document.Services.IngestionManagementAPI/Document.Services.IngestionManagementAPI.csproj", "Document.Services.IngestionManagementAPI/"]
RUN dotnet restore "Document.Services.IngestionManagementAPI/Document.Services.IngestionManagementAPI.csproj"
COPY . .
WORKDIR "/src/Document.Services.IngestionManagementAPI"
RUN dotnet build "./Document.Services.IngestionManagementAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Document.Services.IngestionManagementAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Document.Services.IngestionManagementAPI.dll"]
