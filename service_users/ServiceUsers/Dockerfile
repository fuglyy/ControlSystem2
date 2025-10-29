# Dockerfile для Service Users

# Этап 1: Сборка (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ServiceUsers.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Этап 2: Запуск (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80

# !!! Имя dll для Сервиса Пользователей !!!
ENTRYPOINT ["dotnet", "ServiceUsers.dll"]