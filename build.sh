#!/bin/bash
# build.sh - Build all services for Docker deployment

set -e

echo "🔨 Building Transaction Dispute Portal..."
echo ""

# Build backend services
echo "📦 Building backend services..."
cd backend

echo "  → AuthService"
dotnet publish AuthService/AuthService.csproj -c Release -o AuthService/publish --nologo -v q

echo "  → TransactionService"
dotnet publish TransactionService/TransactionService.csproj -c Release -o TransactionService/publish --nologo -v q

echo "  → DisputeService"
dotnet publish DisputeService/DisputeService.csproj -c Release -o DisputeService/publish --nologo -v q

echo "  → ApiGateway"
dotnet publish ApiGateway/ApiGateway.csproj -c Release -o ApiGateway/publish --nologo -v q

cd ..

# Build frontend
echo ""
echo "📦 Building frontend..."
cd frontend
bun install --silent
bun run build
cd ..

echo ""
echo "✅ Build complete!"
echo ""
echo "Next steps:"
echo "  docker compose up -d"
echo "  Open http://localhost:3000"
