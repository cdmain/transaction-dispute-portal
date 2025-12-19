#!/bin/bash
# stop-local.sh - Stop all locally running services

echo "🛑 Stopping all services..."

pkill -f "dotnet run" 2>/dev/null && echo "  ✓ Backend services stopped" || echo "  - No backend services running"
pkill -f "bun dev" 2>/dev/null && echo "  ✓ Frontend stopped" || echo "  - No frontend running"
pkill -f "vite" 2>/dev/null || true

echo ""
echo "✅ All services stopped"
