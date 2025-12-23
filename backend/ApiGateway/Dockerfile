# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install CA certificates from host
COPY ca-certificates.crt /usr/local/share/ca-certificates/corporate-ca.crt
RUN update-ca-certificates

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
