# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install CA certificates from host
COPY ca-certificates.crt /usr/local/share/ca-certificates/corporate-ca.crt
RUN update-ca-certificates

# Copy project files for restore
COPY TransactionService/*.csproj ./TransactionService/
COPY Shared.Infrastructure/*.csproj ./Shared.Infrastructure/
RUN dotnet restore ./TransactionService/TransactionService.csproj

# Copy source and publish
COPY TransactionService/. ./TransactionService/
COPY Shared.Infrastructure/. ./Shared.Infrastructure/
RUN dotnet publish ./TransactionService/TransactionService.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5001
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "TransactionService.dll"]
