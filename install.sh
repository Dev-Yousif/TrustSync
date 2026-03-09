#!/bin/bash
set -e

echo "Installing TrustSync..."

INSTALL_DIR="$HOME/.local/bin"
DESKTOP_DIR="$HOME/.local/share/applications"
URL="https://github.com/Dev-Yousif/TrustSync/releases/download/v1.2.0/TrustSync-linux-x64.tar.gz"

mkdir -p "$INSTALL_DIR"

echo "Downloading TrustSync..."
curl -sL "$URL" | tar -xz -C "$INSTALL_DIR"
chmod +x "$INSTALL_DIR/TrustSync"

mkdir -p "$DESKTOP_DIR"
cat > "$DESKTOP_DIR/TrustSync.desktop" << EOF
[Desktop Entry]
Name=TrustSync
Comment=Privacy-first personal accounting
Exec=$INSTALL_DIR/TrustSync
Type=Application
Categories=Office;Finance;
EOF

echo ""
echo "TrustSync installed successfully!"
echo "Run it with: TrustSync"
echo ""

if ! echo "$PATH" | grep -q "$INSTALL_DIR"; then
    echo "Note: Add $INSTALL_DIR to your PATH:"
    echo "  echo 'export PATH=\"\$HOME/.local/bin:\$PATH\"' >> ~/.bashrc && source ~/.bashrc"
fi
