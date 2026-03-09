#!/bin/bash
### Description: Build Playarr release packages for GitHub releases
### Usage: bash scripts/build-release.sh [--runtime linux-x64] [--version 4.0.0.1] [--branch main] [--skip-frontend]
###
### Produces tar.gz (Linux/macOS/FreeBSD) or zip (Windows) archives in _artifacts/
### that can be uploaded as GitHub release assets.
###
### Examples:
###   bash scripts/build-release.sh                           # Build for current platform
###   bash scripts/build-release.sh --runtime linux-x64       # Build for specific runtime
###   bash scripts/build-release.sh --runtime linux-arm64     # Cross-compile for ARM64
###   bash scripts/build-release.sh --all-linux               # Build all Linux runtimes

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

# Defaults
RUNTIME=""
VERSION="4.0.0.1"
BRANCH="main"
SKIP_FRONTEND=false
ALL_LINUX=false
FRAMEWORK="net10.0"

# All supported Linux runtimes
LINUX_RUNTIMES=("linux-x64" "linux-arm" "linux-arm64" "linux-musl-x64" "linux-musl-arm64")

show_help() {
    cat <<EOF
Usage: $(basename "$0") [OPTIONS]

Build Playarr release packages for upload to GitHub releases.

Options:
  --runtime <rid>     .NET Runtime Identifier (e.g., linux-x64, linux-arm64, osx-arm64, win-x64)
                      Defaults to auto-detected platform.
  --all-linux         Build all Linux runtimes: ${LINUX_RUNTIMES[*]}
  --version <ver>     Version string (default: 4.0.0.1)
  --branch <name>     Branch name for archive naming (default: main)
  --skip-frontend     Skip frontend build (use existing UI build)
  -h, --help          Show this help message
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --runtime)
            RUNTIME="$2"; shift 2 ;;
        --runtime=*)
            RUNTIME="${1#*=}"; shift ;;
        --version)
            VERSION="$2"; shift 2 ;;
        --version=*)
            VERSION="${1#*=}"; shift ;;
        --branch)
            BRANCH="$2"; shift 2 ;;
        --branch=*)
            BRANCH="${1#*=}"; shift ;;
        --skip-frontend)
            SKIP_FRONTEND=true; shift ;;
        --all-linux)
            ALL_LINUX=true; shift ;;
        -h|--help)
            show_help; exit 0 ;;
        *)
            echo "Unknown option: $1" >&2; exit 1 ;;
    esac
done

cd "$ROOT_DIR"

# Auto-detect runtime if not specified
if [[ -z "$RUNTIME" && "$ALL_LINUX" == false ]]; then
    ARCH=$(uname -m)
    case "$ARCH" in
        x86_64)  RUNTIME="linux-x64" ;;
        aarch64) RUNTIME="linux-arm64" ;;
        armv7l)  RUNTIME="linux-arm" ;;
        *)       echo "Cannot auto-detect runtime for arch: $ARCH"; exit 1 ;;
    esac
    echo "Auto-detected runtime: $RUNTIME"
fi

# Build list of runtimes to process
if $ALL_LINUX; then
    RUNTIMES=("${LINUX_RUNTIMES[@]}")
else
    RUNTIMES=("$RUNTIME")
fi

# Build frontend if needed
if ! $SKIP_FRONTEND; then
    echo ""
    echo "=== Building Frontend ==="
    if ! command -v yarn &>/dev/null; then
        echo "Error: yarn is not installed. Install it or use --skip-frontend." >&2
        exit 1
    fi
    yarn install --frozen-lockfile 2>/dev/null || yarn install
    yarn build
    echo "Frontend build complete"
fi

# Ensure UI output exists (now inside net10.0 folder)
if [ ! -d "_output/$FRAMEWORK/UI" ]; then
    echo "Error: No UI build found at _output/$FRAMEWORK/UI. Run without --skip-frontend or build the frontend first." >&2
    exit 1
fi

# Build and package each runtime
for RID in "${RUNTIMES[@]}"; do
    echo ""
    echo "=== Building Backend for $RID ==="

    # Determine platform (Windows vs Posix)
    IFS='-' read -ra SPLIT <<< "$RID"
    if [ "${SPLIT[0]}" == "win" ]; then
        PLATFORM=Windows
    else
        PLATFORM=Posix
    fi

    # Use the same msbuild approach as CI: builds entire solution with PublishAllRids
    dotnet msbuild -restore src/Playarr.sln \
        -p:SelfContained=true \
        -p:Configuration=Release \
        -p:Platform="$PLATFORM" \
        -p:RuntimeIdentifiers="$RID" \
        -p:EnableWindowsTargeting=true \
        -t:PublishAllRids

    echo "Backend build complete for $RID"

    # Package the output (mirrors CI packaging logic)
    publishDir="_output/$FRAMEWORK/$RID/publish"
    updatePublishDir="_output/Playarr.Update/$FRAMEWORK/$RID/publish"
    packageDir="_release/$RID/Playarr"

    rm -rf "$packageDir"
    mkdir -p "$packageDir"

    echo "Copying published files..."
    cp -r "$publishDir"/* "$packageDir"/

    # Copy updater
    if [ -d "$updatePublishDir" ]; then
        mkdir -p "$packageDir/Playarr.Update"
        cp -r "$updatePublishDir"/* "$packageDir/Playarr.Update/"
    fi

    # Copy license
    cp LICENSE.md "$packageDir/"

    # Platform-specific cleanup
    case "${SPLIT[0]}" in
        linux|freebsd)
            # Remove Windows-specific files
            rm -f "$packageDir"/Playarr.Windows.*
            rm -f "$packageDir"/ServiceUninstall.*
            rm -f "$packageDir"/ServiceInstall.*

            # Add Mono files to UpdatePackage
            if [ -d "$packageDir/Playarr.Update" ]; then
                cp "$packageDir"/Playarr.Mono.* "$packageDir/Playarr.Update/" 2>/dev/null || true
                cp "$packageDir"/Mono.Posix.NETStandard.* "$packageDir/Playarr.Update/" 2>/dev/null || true
                cp "$packageDir"/libMonoPosixHelper.* "$packageDir/Playarr.Update/" 2>/dev/null || true
            fi
            ;;
        osx)
            rm -f "$packageDir"/Playarr.Windows.*
            rm -f "$packageDir"/ServiceUninstall.*
            rm -f "$packageDir"/ServiceInstall.*
            if [ -d "$packageDir/Playarr.Update" ]; then
                cp "$packageDir"/Playarr.Mono.* "$packageDir/Playarr.Update/" 2>/dev/null || true
                cp "$packageDir"/Mono.Posix.NETStandard.* "$packageDir/Playarr.Update/" 2>/dev/null || true
                cp "$packageDir"/libMonoPosixHelper.* "$packageDir/Playarr.Update/" 2>/dev/null || true
            fi
            ;;
        win)
            rm -f "$packageDir"/Playarr.Mono.*
            rm -f "$packageDir"/Mono.Posix.NETStandard.*
            rm -f "$packageDir"/libMonoPosixHelper.*
            if [ -d "$packageDir/Playarr.Update" ]; then
                cp "$packageDir"/Playarr.Windows.* "$packageDir/Playarr.Update/" 2>/dev/null || true
            fi
            ;;
    esac

    # Copy UI into the package (from the framework folder)
    echo "Copying UI..."
    cp -r _output/$FRAMEWORK/UI "$packageDir/UI"

    # Set permissions
    find "$packageDir" -name "ffprobe" -exec chmod a+x {} \; 2>/dev/null || true
    find "$packageDir" -name "Playarr" -type f -exec chmod a+x {} \; 2>/dev/null || true
    find "$packageDir" -name "Playarr.Update" -type f -exec chmod a+x {} \; 2>/dev/null || true

    # Create artifacts directory
    mkdir -p _artifacts

    ARCHIVE_NAME="Playarr.${BRANCH}.${VERSION}.${RID}"

    echo "Packaging $ARCHIVE_NAME..."
    if [[ "$RID" == win-* ]]; then
        (cd "_release/$RID" && zip -rq "../../_artifacts/${ARCHIVE_NAME}.zip" Playarr)
        echo "Created _artifacts/${ARCHIVE_NAME}.zip"
    else
        tar -czf "_artifacts/${ARCHIVE_NAME}.tar.gz" -C "_release/$RID" Playarr
        echo "Created _artifacts/${ARCHIVE_NAME}.tar.gz"
    fi
done

echo ""
echo "=== Build Complete ==="
echo "Artifacts in _artifacts/:"
ls -lh _artifacts/
echo ""
echo "To create a GitHub release, upload these files:"
echo "  gh release create v\${VERSION} _artifacts/Playarr.* --title \"\${VERSION}\" --notes \"Release \${VERSION}\""
