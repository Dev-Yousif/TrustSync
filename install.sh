#!/bin/bash
set -e

echo "Installing TrustSync..."

INSTALL_DIR="$HOME/.local/bin"
ICON_DIR="$HOME/.local/share/icons"
DESKTOP_DIR="$HOME/.local/share/applications"
RELEASE_URL="https://github.com/Dev-Yousif/TrustSync/releases/download/v1.2.0"
ICON_URL="https://raw.githubusercontent.com/Dev-Yousif/TrustSync/main/assets/trustsync-icon.png"

mkdir -p "$INSTALL_DIR" "$ICON_DIR" "$DESKTOP_DIR"

echo "Downloading TrustSync..."
curl -sL "$RELEASE_URL/TrustSync-linux-x64.tar.gz" | tar -xz -C "$INSTALL_DIR"
chmod +x "$INSTALL_DIR/TrustSync"

echo "Installing icon..."
curl -sL "$ICON_URL" -o "$ICON_DIR/trustsync.png"

cat > "$DESKTOP_DIR/TrustSync.desktop" << EOF
[Desktop Entry]
Name=TrustSync
Comment=Privacy-first personal accounting
Exec=$INSTALL_DIR/TrustSync
Icon=$ICON_DIR/trustsync.png
Terminal=false
Type=Application
Categories=Office;Finance;
StartupWMClass=TrustSync
EOF

echo ""
echo "TrustSync installed successfully!"
echo "You can now launch it from your application menu or run: TrustSync"
echo ""

if ! echo "$PATH" | grep -q "$INSTALL_DIR"; then
    echo "Note: Add $INSTALL_DIR to your PATH:"
    echo "  echo 'export PATH=\"\$HOME/.local/bin:\$PATH\"' >> ~/.bashrc && source ~/.bashrc"
fi
