FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JobPlatform.API/JobPlatform.API.csproj", "JobPlatform.API/"]
COPY ["JobPlatform.BLL/JobPlatform.BLL.csproj", "JobPlatform.BLL/"]
COPY ["JobPlatform.DAL/JobPlatform.DAL.csproj", "JobPlatform.DAL/"]
COPY ["JobPlatform.Core/JobPlatform.Core.csproj", "JobPlatform.Core/"]
RUN dotnet restore "JobPlatform.API/JobPlatform.API.csproj"
COPY . .
WORKDIR "/src/JobPlatform.API"
RUN dotnet build "./JobPlatform.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./JobPlatform.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobPlatform.API.dll"]
