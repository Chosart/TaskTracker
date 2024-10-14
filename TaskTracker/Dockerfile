# Базовий образ для ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Образ для збору проекту
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskTracker/TaskTracker.csproj", "TaskTracker/"]
RUN dotnet restore "./TaskTracker/TaskTracker.csproj"
COPY . .
WORKDIR "/src/TaskTracker"
RUN dotnet build "./TaskTracker.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

# Образ для публікації
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskTracker.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

# Фінальний образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskTracker.dll"]