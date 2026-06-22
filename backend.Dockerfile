# 1. Aşama: Derleme (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Tüm klasörleri ve dosyaları tek seferde kopyala
COPY . .

# Projenin ana katmanına gir ve tüm sistemi derle
WORKDIR /src/HabitTracker.API
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# 2. Aşama: Çalıştırma (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HabitTracker.API.dll"]