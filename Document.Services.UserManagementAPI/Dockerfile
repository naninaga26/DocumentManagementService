FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Document.Services.UserManagementAPI/Document.Services.UserManagementAPI.csproj", "Document.Services.UserManagementAPI/"]
RUN dotnet restore "Document.Services.UserManagementAPI/Document.Services.UserManagementAPI.csproj"
COPY . .
WORKDIR "/src/Document.Services.UserManagementAPI"
RUN dotnet build "./Document.Services.UserManagementAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Document.Services.UserManagementAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Document.Services.UserManagementAPI.dll"]
