#!/bin/bash
# ══════════════════════════════════════════════════════════════════════════════
# Corporate CA Certificate Setup Script
# ══════════════════════════════════════════════════════════════════════════════
# 
# This script exports your system's CA certificates for use in Docker builds.
# Required for corporate networks with SSL inspection/proxy.
#
# Usage: ./setup-corporate-ca.sh
# ══════════════════════════════════════════════════════════════════════════════

set -e

echo "🔐 Exporting CA certificates for Docker builds..."

# Export all system CA certificates (macOS)
if [[ "$OSTYPE" == "darwin"* ]]; then
    echo "📦 Detected macOS - exporting from System Keychain..."
    
    # Export all certificates from System Keychain
    security find-certificate -a -p /System/Library/Keychains/SystemRootCertificates.keychain > /tmp/system-ca.pem 2>/dev/null || true
    security find-certificate -a -p /Library/Keychains/System.keychain >> /tmp/system-ca.pem 2>/dev/null || true
    
    # Also try to get user certificates
    security find-certificate -a -p ~/Library/Keychains/login.keychain-db >> /tmp/system-ca.pem 2>/dev/null || true
    
    CA_BUNDLE="/tmp/system-ca.pem"
    
elif [[ -f /etc/ssl/certs/ca-certificates.crt ]]; then
    echo "📦 Detected Linux - using system CA bundle..."
    CA_BUNDLE="/etc/ssl/certs/ca-certificates.crt"
    
elif [[ -f /etc/pki/tls/certs/ca-bundle.crt ]]; then
    echo "📦 Detected RHEL/CentOS - using system CA bundle..."
    CA_BUNDLE="/etc/pki/tls/certs/ca-bundle.crt"
    
else
    echo "⚠️  Could not find system CA certificates"
    echo "Creating empty placeholder file..."
    touch /tmp/system-ca.pem
    CA_BUNDLE="/tmp/system-ca.pem"
fi

# Copy to backend directory (shared by all services)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$SCRIPT_DIR/backend"

if [[ -d "$BACKEND_DIR" ]]; then
    cp "$CA_BUNDLE" "$BACKEND_DIR/ca-certificates.crt"
    echo "✅ Copied CA certificates to backend/"
else
    echo "⚠️  backend/ directory not found at $BACKEND_DIR"
    exit 1
fi

echo ""
echo "🎉 CA certificates exported successfully!"
echo ""
echo "You can now run:"
echo "  nerdctl compose up -d --build"
echo ""
