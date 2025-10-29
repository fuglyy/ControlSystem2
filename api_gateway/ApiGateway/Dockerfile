# Dockerfile для API Gateway

# Этап 1: Сборка (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Копируем csproj файл
COPY ApiGateway.csproj .
# Восстанавливаем зависимости
RUN dotnet restore
# Копируем весь код
COPY . .
# Публикуем приложение (собираем готовые файлы)
RUN dotnet publish -c Release -o /app/publish

# Этап 2: Запуск (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# Копируем опубликованные файлы с этапа Build
COPY --from=build /app/publish .

EXPOSE 80

# !!! САМОЕ ВАЖНОЕ: Указываем имя dll-файла !!!
ENTRYPOINT ["dotnet", "ApiGateway.dll"]