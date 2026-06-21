# 1. Aşama: Derleme (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve restore et
COPY ["HabitTracker.API/HabitTracker.API.csproj", "HabitTracker.API/"]
COPY ["HabitTracker.Application/HabitTracker.Application.csproj", "HabitTracker.Application/"]
COPY ["HabitTracker.Domain/HabitTracker.Domain.csproj", "HabitTracker.Domain/"]
COPY ["HabitTracker.Infrastructure/HabitTracker.Infrastructure.csproj", "HabitTracker.Infrastructure/"]
RUN dotnet restore "HabitTracker.API/HabitTracker.API.csproj"

# Tüm kodları kopyala ve yayınla (Publish)
COPY . .
WORKDIR "/src/HabitTracker.API"
RUN dotnet publish "HabitTracker.API.csproj" -c Release -o /app/publish

# 2. Aşama: Çalıştırma (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HabitTracker.API.dll"]