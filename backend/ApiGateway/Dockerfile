# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Optional: Corporate CA certificates (for SSL inspection proxies)
# Copy ca-certificates.crt to backend/ folder if behind corporate proxy
COPY ca-certificate[s].crt /usr/local/share/ca-certificates/corporate-ca.crt
RUN update-ca-certificates 2>/dev/null || true

# Copy project files for restore
COPY ApiGateway/*.csproj ./ApiGateway/
COPY Shared.Infrastructure/*.csproj ./Shared.Infrastructure/
RUN dotnet restore ./ApiGateway/ApiGateway.csproj

# Copy source and publish
COPY ApiGateway/. ./ApiGateway/
COPY Shared.Infrastructure/. ./Shared.Infrastructure/
RUN dotnet publish ./ApiGateway/ApiGateway.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5000
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
