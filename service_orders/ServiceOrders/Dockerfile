# Dockerfile для Service Orders

# Этап 1: Сборка (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ServiceOrders.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Этап 2: Запуск (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5074

# !!! Имя dll для Сервиса Заказов !!!
ENTRYPOINT ["dotnet", "ServiceOrders.dll"]