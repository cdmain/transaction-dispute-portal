# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install CA certificates from host
COPY ca-certificates.crt /usr/local/share/ca-certificates/corporate-ca.crt
RUN update-ca-certificates

# Copy project files for restore
COPY AuthService/*.csproj ./AuthService/
COPY Shared.Infrastructure/*.csproj ./Shared.Infrastructure/
RUN dotnet restore ./AuthService/AuthService.csproj

# Copy source and publish
COPY AuthService/. ./AuthService/
COPY Shared.Infrastructure/. ./Shared.Infrastructure/
RUN dotnet publish ./AuthService/AuthService.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5003
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5003
ENTRYPOINT ["dotnet", "AuthService.dll"]
