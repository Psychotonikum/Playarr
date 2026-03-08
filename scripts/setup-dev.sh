#!/bin/bash
### Description: Playarr development environment setup
### Sets up all dependencies needed to build, test, and run Playarr locally.
### Works on Debian/Ubuntu. Run as root or with sudo.

set -euo pipefail

echo "=== Playarr Development Environment Setup ==="
echo ""

# Check for root
if [ "$EUID" -ne 0 ]; then
    echo "Please run as root (sudo)."
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "Repository root: $REPO_ROOT"
echo ""

# ---------------------------------------------------------------------------
# 1. System packages
# ---------------------------------------------------------------------------
echo "--- Installing system dependencies ---"
apt-get update
apt-get install -y \
    curl \
    wget \
    git \
    sqlite3 \
    libsqlite3-dev \
    libicu-dev \
    libssl-dev \
    build-essential \
    ca-certificates \
    gnupg \
    lsb-release

# ---------------------------------------------------------------------------
# 2. .NET SDK 10
# ---------------------------------------------------------------------------
DOTNET_VERSION="10.0"
if command -v dotnet &>/dev/null && dotnet --list-sdks | grep -q "^${DOTNET_VERSION}"; then
    echo ".NET SDK ${DOTNET_VERSION} already installed"
else
    echo "--- Installing .NET SDK ${DOTNET_VERSION} ---"
    # Use Microsoft's install script for latest .NET 10
    if [ ! -f /tmp/dotnet-install.sh ]; then
        curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
        chmod +x /tmp/dotnet-install.sh
    fi
    /tmp/dotnet-install.sh --channel ${DOTNET_VERSION} --install-dir /usr/share/dotnet
    ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet 2>/dev/null || true
fi
echo "dotnet version: $(dotnet --version)"

# ---------------------------------------------------------------------------
# 3. Node.js 20 + Yarn
# ---------------------------------------------------------------------------
NODE_MAJOR=20
if command -v node &>/dev/null && node --version | grep -q "^v${NODE_MAJOR}"; then
    echo "Node.js ${NODE_MAJOR} already installed"
else
    echo "--- Installing Node.js ${NODE_MAJOR} ---"
    if [ ! -f /etc/apt/keyrings/nodesource.gpg ]; then
        mkdir -p /etc/apt/keyrings
        curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg
    fi
    echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_${NODE_MAJOR}.x nodistro main" > /etc/apt/sources.list.d/nodesource.list
    apt-get update
    apt-get install -y nodejs
fi
echo "node version: $(node --version)"

if ! command -v yarn &>/dev/null; then
    echo "--- Installing Yarn ---"
    npm install -g yarn
fi
echo "yarn version: $(yarn --version)"

# ---------------------------------------------------------------------------
# 4. Project dependencies
# ---------------------------------------------------------------------------
echo ""
echo "--- Installing frontend dependencies ---"
cd "$REPO_ROOT"
yarn install

echo ""
echo "--- Building frontend ---"
yarn build

echo ""
echo "--- Restoring .NET packages ---"
dotnet restore "$REPO_ROOT/src/Playarr.sln"

echo ""
echo "--- Building solution ---"
dotnet msbuild -restore "$REPO_ROOT/src/Playarr.sln" \
    -p:GenerateFullPaths=true \
    -p:Configuration=Debug \
    -p:Platform=Posix

# ---------------------------------------------------------------------------
# 5. ROM library directory
# ---------------------------------------------------------------------------
ROMS_DIR="/media/roms"
if [ ! -d "$ROMS_DIR" ]; then
    echo ""
    echo "--- Creating ROM library directory at $ROMS_DIR ---"
    mkdir -p "$ROMS_DIR"
    chmod 775 "$ROMS_DIR"
fi

# ---------------------------------------------------------------------------
# 6. Load IGDB credentials into running config (if .env.local exists)
# ---------------------------------------------------------------------------
ENV_FILE="$REPO_ROOT/.env.local"
if [ -f "$ENV_FILE" ]; then
    echo ""
    echo "--- Found .env.local, IGDB credentials available for testing ---"
    echo "   Credentials will be loaded into Playarr via the Settings > Metadata Source UI"
    echo "   or injected at test time."
    # Source but don't export — just inform the user
    # shellcheck disable=SC1090
    source "$ENV_FILE"
    echo "   Twitch Client ID: ${TWITCH_CLIENT_ID:0:8}..."
fi

# ---------------------------------------------------------------------------
# 7. Summary
# ---------------------------------------------------------------------------
echo ""
echo "=========================================="
echo "  Development environment ready!"
echo "=========================================="
echo ""
echo "Quick commands:"
echo "  Run Playarr:    ./_output/net10.0/Playarr"
echo "  Frontend dev:   yarn start"
echo "  Run tests:      dotnet test src/Playarr.sln --no-build"
echo "  Build:          dotnet msbuild -restore src/Playarr.sln -p:Configuration=Debug -p:Platform=Posix"
echo ""
echo "Web UI:           http://localhost:9797"
echo "ROM Library:      $ROMS_DIR"
echo ""
echo "IMPORTANT: Configure Twitch/IGDB credentials in Settings > Metadata Source"
echo "           or create .env.local with TWITCH_CLIENT_ID and TWITCH_CLIENT_SECRET"
echo ""
