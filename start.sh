#!/bin/bash
# ══════════════════════════════════════════════════════════════════════════════
# Transaction Dispute Portal - Start Script
# ══════════════════════════════════════════════════════════════════════════════

set -e

echo ""
echo "🚀 Starting Transaction Dispute Portal..."
echo ""

# Check if corporate CA setup is needed
if [[ ! -f backend/ca-certificates.crt ]]; then
    echo "💡 Tip: If you're on a corporate network with SSL inspection,"
    echo "   run ./setup-corporate-ca.sh first if builds fail."
    echo ""
fi

# Build and start
echo "📦 Building and starting containers..."
nerdctl compose up -d --build

# Wait for services to be ready
echo ""
echo "⏳ Waiting for services to start..."
sleep 5

# Check if frontend is running
if curl -s http://localhost:3000 > /dev/null 2>&1; then
    echo ""
    echo "═══════════════════════════════════════════════════════════════"
    echo ""
    echo "  ✅ Transaction Dispute Portal is running!"
    echo ""
    echo "  🌐 Frontend:    http://localhost:3000"
    echo "  📡 API Gateway: http://localhost:5050"
    echo "  📚 API Docs:    http://localhost:5050/swagger"
    echo ""
    echo "  📧 Demo Login:  demo@example.com / Demo123!"
    echo ""
    echo "═══════════════════════════════════════════════════════════════"
    echo ""
    
    # Open browser automatically
    if [[ "$OSTYPE" == "darwin"* ]]; then
        echo "🌐 Opening browser..."
        open http://localhost:3000
    elif command -v xdg-open &> /dev/null; then
        echo "🌐 Opening browser..."
        xdg-open http://localhost:3000
    fi
else
    echo ""
    echo "⚠️  Services are starting up. Check logs with:"
    echo "   nerdctl compose logs -f"
    echo ""
    echo "Once ready, open: http://localhost:3000"
fi

echo ""
echo "To stop: nerdctl compose down"
echo ""
