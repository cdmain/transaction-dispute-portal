# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY DisputeService/*.csproj ./DisputeService/
COPY Shared.Infrastructure/*.csproj ./Shared.Infrastructure/
RUN dotnet restore ./DisputeService/DisputeService.csproj

# Copy source and publish
COPY DisputeService/. ./DisputeService/
COPY Shared.Infrastructure/. ./Shared.Infrastructure/
RUN dotnet publish ./DisputeService/DisputeService.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5002
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5002
ENTRYPOINT ["dotnet", "DisputeService.dll"]
