#!/bin/bash
# start-local.sh - Start all services locally for development

set -e

echo "🚀 Starting Transaction Dispute Portal (Local Development)"
echo ""

# Function to cleanup on exit
cleanup() {
    echo ""
    echo "🛑 Stopping all services..."
    pkill -f "dotnet run" 2>/dev/null || true
    pkill -f "bun dev" 2>/dev/null || true
    echo "✅ All services stopped"
    exit 0
}

trap cleanup SIGINT SIGTERM

# Start backend services
echo "Starting backend services..."

cd backend/AuthService
dotnet run --urls="http://localhost:5003" &
echo "  ✓ AuthService starting on :5003"

cd ../TransactionService
dotnet run --urls="http://localhost:5001" &
echo "  ✓ TransactionService starting on :5001"

cd ../DisputeService
dotnet run --urls="http://localhost:5002" &
echo "  ✓ DisputeService starting on :5002"

cd ../ApiGateway
dotnet run --urls="http://localhost:5000" &
echo "  ✓ ApiGateway starting on :5000"

cd ../../frontend
echo ""
echo "Starting frontend..."
bun install --silent
bun dev &
echo "  ✓ Frontend starting on :5173"

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ All services starting!"
echo ""
echo "🌐 Frontend:    http://localhost:5173"
echo "🔌 API Gateway: http://localhost:5000"
echo ""
echo "🔐 Demo Login:"
echo "   Email:    demo@example.com"
echo "   Password: Demo123!"
echo ""
echo "Press Ctrl+C to stop all services"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Wait for all background processes
wait
