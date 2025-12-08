# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY HealthSync.Backend.sln .
COPY HealthSync.Domain/HealthSync.Domain.csproj HealthSync.Domain/
COPY HealthSync.Application/HealthSync.Application.csproj HealthSync.Application/
COPY HealthSync.Infrastructure/HealthSync.Infrastructure.csproj HealthSync.Infrastructure/
COPY HealthSync.Presentation/HealthSync.Presentation.csproj HealthSync.Presentation/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build the application
WORKDIR /src/HealthSync.Presentation
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# Thêm dòng này để chạy với quyền user thường (an toàn hơn)
USER app
# Sửa cổng từ 80 -> 8080 và 443 -> 8081 (vì user thường dùng cổng này)
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthSync.Presentation.dll"]
