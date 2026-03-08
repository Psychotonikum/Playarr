#!/bin/bash
### Description: Playarr .NET Debian install
### Originally written for Radarr by: DoctorArr - doctorarr@the-rowlands.co.uk on 2021-10-01 v1.0
### Updates for servarr suite made by Bakerboy448, DoctorArr, brightghost, aeramor and VP-EN
### Version v1.0.0 2023-12-29 - StevieTV - adapted from servarr script for Sonarr installs
### Version V1.0.1 2024-01-02 - StevieTV - remove UTF8-BOM
### Version V1.0.2 2024-01-03 - markus101 - Get user input from /dev/tty
### Version V1.0.3 2024-01-06 - StevieTV - exit script when it is ran from install directory
### Version V1.0.4 2025-04-05 - kaecyra - Allow user/group to be supplied via CLI, add unattended mode
### Version V1.0.5 2025-07-08 - bparkin1283 - use systemctl instead of service for stopping app
### Version V2.0.0 2026-03-08 - Playarr - Adapted for Playarr, added update support

### Boilerplate Warning
#THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
#EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
#MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
#NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
#LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
#OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
#WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

scriptversion="2.0.0"
scriptdate="2026-03-08"

set -euo pipefail

echo "Running Playarr Install Script - Version [$scriptversion] as of [$scriptdate]"

# Am I root?, need root!
if [ "$EUID" -ne 0 ]; then
    echo "Please run as root."
    exit 1
fi

app="playarr"
app_port="9797"
app_prereq="curl sqlite3 wget jq"
app_umask="0002"
branch="main"
github_repo="Psychotonikum/playarr"

# Constants
installdir="/opt"              # Install Location
bindir="${installdir}/${app^}" # Full Path to Install Location
datadir="/var/lib/$app/"       # AppData directory
app_bin=${app^}                # Binary Name of the app

# Detect if this is an update (existing installation)
is_update=false
if [ -d "$bindir" ] && [ -f "$bindir/$app_bin" ]; then
    is_update=true
fi

# This script should not be ran from installdir
if [ "$installdir" == "$(dirname -- "$( readlink -f -- "$0"; )")" ] || [ "$bindir" == "$(dirname -- "$( readlink -f -- "$0"; )")" ]; then
    echo "You should not run this script from the intended install directory. The script will exit. Please re-run it from another directory"
    exit 1
fi

show_help() {
    cat <<EOF
Usage: $(basename "$0") [OPTIONS]

Installs or updates Playarr. If Playarr is already installed, it will be
stopped, updated in-place, and restarted automatically.

Options:
  --user <name>       What user will ${app^} run under?
                      User will be created if it doesn't already exist.

  --group <name>      What group will ${app^} run under?
                      Group will be created if it doesn't already exist.

  --branch <name>     Which branch to install (Default: main)

  --tarball <path>    Install from a local tar.gz file instead of downloading

  --version <ver>     Install a specific version (e.g., 4.0.0.1). Default: latest

  -u                  Unattended mode
                      The installer will not prompt or pause, making it
                      suitable for automated installations. Requires --user
                      and --group.

  -h, --help          Show this help message and exit
EOF
}

# Default values for command-line arguments
arg_user=""
arg_group=""
arg_unattended=false
arg_tarball=""
arg_version=""

# Parse command-line arguments
while [[ $# -gt 0 ]]; do
    case "$1" in
        --user=*)
            arg_user="${1#*=}"
            shift
            ;;
        --user)
            if [[ -n "${2:-}" && "$2" != -* ]]; then
                arg_user="$2"
                shift 2
            else
                echo "Error: --user requires a value." >&2
                exit 1
            fi
            ;;
        --group=*)
            arg_group="${1#*=}"
            shift
            ;;
        --group)
            if [[ -n "${2:-}" && "$2" != -* ]]; then
                arg_group="$2"
                shift 2
            else
                echo "Error: --group requires a value." >&2
                exit 1
            fi
            ;;
        --branch=*)
            branch="${1#*=}"
            shift
            ;;
        --branch)
            if [[ -n "${2:-}" && "$2" != -* ]]; then
                branch="$2"
                shift 2
            else
                echo "Error: --branch requires a value." >&2
                exit 1
            fi
            ;;
        --tarball=*)
            arg_tarball="${1#*=}"
            shift
            ;;
        --tarball)
            if [[ -n "${2:-}" && "$2" != -* ]]; then
                arg_tarball="$2"
                shift 2
            else
                echo "Error: --tarball requires a value." >&2
                exit 1
            fi
            ;;
        --version=*)
            arg_version="${1#*=}"
            shift
            ;;
        --version)
            if [[ -n "${2:-}" && "$2" != -* ]]; then
                arg_version="$2"
                shift 2
            else
                echo "Error: --version requires a value." >&2
                exit 1
            fi
            ;;
        -u)
            arg_unattended=true
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            echo "Unknown option: $1" >&2
            echo "Use --help to see valid options." >&2
            exit 1
            ;;
    esac
done

# If unattended mode is requested, require user and group
if $arg_unattended; then
    if [[ -z "$arg_user" || -z "$arg_group" ]]; then
        echo "Error: --user and --group are required when using -u (unattended mode)." >&2
        exit 1
    fi
fi

if $is_update; then
    echo ""
    echo "Existing ${app^} installation detected at [$bindir]"
    echo "This script will update it in-place."
    echo ""
fi

# Prompt User if necessary
if [ -n "$arg_user" ]; then
    app_uid="$arg_user"
elif $is_update; then
    # Try to detect the current user from the systemd service
    existing_user=$(grep -oP '(?<=^User=).*' /etc/systemd/system/"$app".service 2>/dev/null || echo "")
    if [ -n "$existing_user" ]; then
        app_uid="$existing_user"
        echo "Using existing service user: $app_uid"
    else
        read -r -p "What user should ${app^} run as? (Default: $app): " app_uid < /dev/tty
    fi
else
    read -r -p "What user should ${app^} run as? (Default: $app): " app_uid < /dev/tty
fi
app_uid=$(echo "$app_uid" | tr -d ' ')
app_uid=${app_uid:-$app}

# Prompt Group if necessary
if [ -n "$arg_group" ]; then
    app_guid="$arg_group"
elif $is_update; then
    existing_group=$(grep -oP '(?<=^Group=).*' /etc/systemd/system/"$app".service 2>/dev/null || echo "")
    if [ -n "$existing_group" ]; then
        app_guid="$existing_group"
        echo "Using existing service group: $app_guid"
    else
        read -r -p "What group should ${app^} run as? (Default: media): " app_guid < /dev/tty
    fi
else
    read -r -p "What group should ${app^} run as? (Default: media): " app_guid < /dev/tty
fi
app_guid=$(echo "$app_guid" | tr -d ' ')
app_guid=${app_guid:-media}

if $is_update; then
    echo "Updating [${app^}] at [$bindir] using [$datadir] for the AppData Directory"
else
    echo "This will install [${app^}] to [$bindir] and use [$datadir] for the AppData Directory"
fi
echo "${app^} will run as the user [$app_uid] and group [$app_guid]."
if ! $is_update; then
    echo "By continuing, you've confirmed that the selected user and group will have READ and WRITE access to your ROM Library and Download Client Completed Download directories"
fi
if ! $arg_unattended; then
    read -n 1 -r -s -p $'Press enter to continue or ctrl+c to exit...\n' < /dev/tty
fi

# Create User / Group as needed
if [ "$app_guid" != "$app_uid" ]; then
    if ! getent group "$app_guid" >/dev/null; then
        groupadd "$app_guid"
    fi
fi
if ! getent passwd "$app_uid" >/dev/null; then
    adduser --system --no-create-home --ingroup "$app_guid" "$app_uid"
    echo "Created and added User [$app_uid] to Group [$app_guid]"
fi
if ! getent group "$app_guid" | grep -qw "$app_uid"; then
    echo "User [$app_uid] did not exist in Group [$app_guid]"
    usermod -a -G "$app_guid" "$app_uid"
    echo "Added User [$app_uid] to Group [$app_guid]"
fi

# Stop the App if running
if systemctl is-active --quiet "$app" 2>/dev/null; then
    systemctl stop "$app"
    echo "Stopped existing $app"
fi

# Create Appdata Directory
mkdir -p "$datadir"
chown -R "$app_uid":"$app_guid" "$datadir"
chmod 775 "$datadir"
echo "Directories created"

# Download and install the App
echo ""
echo "Installing pre-requisite Packages"
# shellcheck disable=SC2086
apt-get update && apt-get install -y $app_prereq
echo ""
ARCH=$(dpkg --print-architecture)

# Use a temporary directory for download
tmpdir=$(mktemp -d)
trap 'rm -rf "$tmpdir"' EXIT

if [ -n "$arg_tarball" ]; then
    # Install from local tarball
    if [ ! -f "$arg_tarball" ]; then
        echo "Error: Tarball not found: $arg_tarball" >&2
        exit 1
    fi
    echo ""
    echo "Installing from local tarball: $arg_tarball"
    tar -xzf "$arg_tarball" -C "$tmpdir"
else
    # Download from GitHub releases
    case "$ARCH" in
    "amd64") rid="linux-x64" ;;
    "armhf") rid="linux-arm" ;;
    "arm64") rid="linux-arm64" ;;
    *)
        echo "Arch not supported: $ARCH"
        exit 1
        ;;
    esac

    if [ -n "$arg_version" ]; then
        # Download specific version
        DLURL="https://github.com/${github_repo}/releases/download/v${arg_version}/Playarr.${branch}.${arg_version}.${rid}.tar.gz"
    else
        # Get latest release URL from GitHub API
        echo "Fetching latest release info from GitHub..."
        release_json=$(curl -sL "https://api.github.com/repos/${github_repo}/releases/latest")

        # Check if the API returned a valid response with assets
        if ! echo "$release_json" | jq -e '.assets' &>/dev/null; then
            echo "Error: Could not find any releases at https://github.com/${github_repo}/releases" >&2
            echo "The repository may be private, or no releases have been published yet." >&2
            echo "" >&2
            echo "You can build from source and install with --tarball:" >&2
            echo "  bash scripts/build-release.sh --runtime ${rid}" >&2
            echo "  sudo bash distribution/debian/install.sh --tarball _artifacts/Playarr.*.${rid}.tar.gz" >&2
            exit 1
        fi

        DLURL=$(echo "$release_json" | jq -r ".assets[] | select(.name | contains(\"${rid}\")) | select(.name | endswith(\".tar.gz\")) | .browser_download_url" | head -1)

        if [ -z "$DLURL" ] || [ "$DLURL" = "null" ]; then
            echo "Error: Could not find a release asset for ${rid} at https://github.com/${github_repo}/releases" >&2
            echo "" >&2
            echo "You can build from source and install with --tarball:" >&2
            echo "  bash scripts/build-release.sh --runtime ${rid}" >&2
            echo "  sudo bash distribution/debian/install.sh --tarball _artifacts/Playarr.*.${rid}.tar.gz" >&2
            exit 1
        fi
    fi

    echo ""
    echo "Downloading from: $DLURL"
    wget -q --show-progress -O "$tmpdir/playarr.tar.gz" "$DLURL"
    tar -xzf "$tmpdir/playarr.tar.gz" -C "$tmpdir"
fi
echo ""
echo "Installation files downloaded and extracted"

# Remove existing installation (config data is in datadir, not bindir)
if [ -d "$bindir" ]; then
    echo "Removing existing installation"
    rm -rf "$bindir"
fi

echo "Installing..."
mv "$tmpdir/${app^}" "$installdir"
chown "$app_uid":"$app_guid" -R "$bindir"
chmod 775 "$bindir"

# Ensure we check for an update in case user installs older version or different branch
touch "$datadir"/update_required
chown "$app_uid":"$app_guid" "$datadir"/update_required
echo "App Installed"

# Configure Autostart
# Remove any previous app .service
echo "Removing old service file"
rm -f /etc/systemd/system/"$app".service

# Create app .service with correct user startup
echo "Creating service file"
cat <<EOF | tee /etc/systemd/system/"$app".service >/dev/null
[Unit]
Description=${app^} Daemon
After=syslog.target network.target

[Service]
User=$app_uid
Group=$app_guid
UMask=$app_umask
Type=simple
ExecStart=$bindir/$app_bin -nobrowser -data=$datadir
TimeoutStopSec=20
KillMode=process
Restart=on-failure

[Install]
WantedBy=multi-user.target
EOF

# Start the App
echo "Service file created. Attempting to start the app"
systemctl -q daemon-reload
systemctl enable --now -q "$app"

# Finish Update/Installation
host=$(hostname -I)
ip_local=$(grep -oP '^\S*' <<<"$host")
echo ""
if $is_update; then
    echo "Update complete"
else
    echo "Install complete"
fi
sleep 10
STATUS="$(systemctl is-active "$app")"
if [ "${STATUS}" = "active" ]; then
    echo "Browse to http://$ip_local:$app_port for the ${app^} GUI"
else
    echo "${app^} failed to start"
fi

# Exit
exit 0
