# Pre-built Dockerfile - requires local build first:
# dotnet publish -c Release -o publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5002
COPY publish/ .
ENV ASPNETCORE_URLS=http://+:5002
ENTRYPOINT ["dotnet", "DisputeService.dll"]
