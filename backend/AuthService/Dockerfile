# Pre-built Dockerfile - requires local build first:
# dotnet publish -c Release -o publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5003
COPY publish/ .
ENV ASPNETCORE_URLS=http://+:5003
ENTRYPOINT ["dotnet", "AuthService.dll"]
