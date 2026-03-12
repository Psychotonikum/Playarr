#!/usr/bin/env bash
# provision-local.sh — Auto-configure a fresh Playarr instance from .env.local
# Run after a DB wipe + service restart to pre-fill all settings.
# Reads credentials from .env.local so the user never has to enter them manually.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
ENV_FILE="$PROJECT_ROOT/.env.local"

if [[ ! -f "$ENV_FILE" ]]; then
    echo "ERROR: $ENV_FILE not found"
    exit 1
fi

# shellcheck source=/dev/null
set -a
source "$ENV_FILE"
set +a

# Read API key from config.xml
CONFIG_XML="/var/lib/playarr/config.xml"
if [[ ! -f "$CONFIG_XML" ]]; then
    echo "ERROR: $CONFIG_XML not found — is Playarr installed?"
    exit 1
fi

API_KEY=$(grep -oP '(?<=<ApiKey>)[^<]+' "$CONFIG_XML")
BASE_URL="http://localhost:9797"

api() {
    local method="$1" path="$2"
    shift 2
    curl -sf -X "$method" \
        "${BASE_URL}/api/v3${path}" \
        -H "X-Api-Key: ${API_KEY}" \
        -H "Content-Type: application/json" \
        "$@"
}

echo "Waiting for Playarr to be ready..."
for i in $(seq 1 30); do
    if curl -sf "${BASE_URL}/api/v3/system/status" -H "X-Api-Key: ${API_KEY}" > /dev/null 2>&1; then
        break
    fi
    if [[ $i -eq 30 ]]; then
        echo "ERROR: Playarr not responding after 30s"
        exit 1
    fi
    sleep 1
done
echo "Playarr is up."

# ── 1. Set authentication (username/password) ──────────────────────────
echo "Setting authentication credentials..."
HOST_CONFIG=$(api GET /config/host)
UPDATED_HOST=$(echo "$HOST_CONFIG" | python3 -c "
import sys, json
cfg = json.load(sys.stdin)
import os
cfg['username'] = os.environ.get('DEFAULT_USERNAME', 'admin')
cfg['password'] = os.environ.get('DEFAULT_PASSWORD', 'admin')
cfg['passwordConfirmation'] = os.environ.get('DEFAULT_PASSWORD', 'admin')
cfg['authenticationMethod'] = 'forms'
cfg['authenticationRequired'] = 'disabledForLocalAddresses'
print(json.dumps(cfg))
")
api PUT /config/host -d "$UPDATED_HOST" > /dev/null
echo "  ✓ Auth: ${DEFAULT_USERNAME:-admin} / ****"

# ── 2. Set Twitch/IGDB credentials ─────────────────────────────────────
echo "Setting IGDB/Twitch credentials..."
META_CONFIG=$(api GET /config/metadatasource)
UPDATED_META=$(echo "$META_CONFIG" | python3 -c "
import sys, json, os
cfg = json.load(sys.stdin)
cfg['twitchClientId'] = os.environ['TWITCH_CLIENT_ID']
cfg['twitchClientSecret'] = os.environ['TWITCH_CLIENT_SECRET']
print(json.dumps(cfg))
")
api PUT /config/metadatasource -d "$UPDATED_META" > /dev/null
echo "  ✓ IGDB: client_id=${TWITCH_CLIENT_ID:0:8}..."

# ── 3. Add root folders ────────────────────────────────────────────────
echo "Adding root folders..."
EXISTING_ROOTS=$(api GET /rootfolder)
add_root() {
    local path="$1"
    if [[ ! -d "$path" ]]; then
        echo "  ⚠ Skipping $path (does not exist)"
        return
    fi
    # Check if already added
    if echo "$EXISTING_ROOTS" | python3 -c "
import sys, json
roots = json.load(sys.stdin)
if any(r['path'] == '$path' for r in roots):
    sys.exit(0)
sys.exit(1)
" 2>/dev/null; then
        echo "  - $path (already exists)"
        return
    fi
    api POST /rootfolder -d "{\"path\": \"$path\"}" > /dev/null
    echo "  ✓ $path"
}

add_root "/media/roms"
add_root "/media/testroms"

# ── 4. Add SABnzbd download client ─────────────────────────────────────
if [[ -n "${SABNZBD_URL:-}" && -n "${SABNZBD_API:-}" ]]; then
    echo "Adding SABnzbd download client..."
    EXISTING_DL=$(api GET /downloadclient)
    HAS_SAB=$(echo "$EXISTING_DL" | python3 -c "
import sys, json
clients = json.load(sys.stdin)
print('yes' if any(c['implementation'] == 'Sabnzbd' for c in clients) else 'no')
")
    if [[ "$HAS_SAB" == "no" ]]; then
        # Parse host and port from URL
        SAB_HOST=$(echo "$SABNZBD_URL" | python3 -c "from urllib.parse import urlparse; import sys; u=urlparse(sys.stdin.read().strip()); print(u.hostname)")
        SAB_PORT=$(echo "$SABNZBD_URL" | python3 -c "from urllib.parse import urlparse; import sys; u=urlparse(sys.stdin.read().strip()); print(u.port or 8080)")

        SAB_PAYLOAD="{
            \"enable\": true,
            \"protocol\": \"usenet\",
            \"priority\": 1,
            \"removeCompletedDownloads\": true,
            \"removeFailedDownloads\": true,
            \"name\": \"SABnzbd\",
            \"implementationName\": \"SABnzbd\",
            \"implementation\": \"Sabnzbd\",
            \"configContract\": \"SabnzbdSettings\",
            \"tags\": [],
            \"fields\": [
                {\"name\": \"host\", \"value\": \"${SAB_HOST}\"},
                {\"name\": \"port\", \"value\": ${SAB_PORT}},
                {\"name\": \"useSsl\", \"value\": false},
                {\"name\": \"apiKey\", \"value\": \"${SABNZBD_API}\"},
                {\"name\": \"gameCategory\", \"value\": \"games\"},
                {\"name\": \"recentTvPriority\", \"value\": -100},
                {\"name\": \"olderTvPriority\", \"value\": -100}
            ]
        }"
        # Try enabled first; if validation fails (e.g. SABnzbd category missing), add disabled
        if api POST "/downloadclient?forceSave=true" -d "$SAB_PAYLOAD" > /dev/null 2>&1; then
            echo "  ✓ SABnzbd @ ${SAB_HOST}:${SAB_PORT}"
        else
            DISABLED_PAYLOAD=$(echo "$SAB_PAYLOAD" | python3 -c "import sys,json; p=json.load(sys.stdin); p['enable']=False; print(json.dumps(p))")
            api POST /downloadclient -d "$DISABLED_PAYLOAD" > /dev/null
            echo "  ✓ SABnzbd @ ${SAB_HOST}:${SAB_PORT} (disabled — enable after fixing SABnzbd category)"
        fi
    else
        echo "  - SABnzbd (already exists)"
    fi
fi

# ── 5. Add Prowlarr/Newznab indexer ────────────────────────────────────
if [[ -n "${PROWLARR_URL:-}" && -n "${PROWLARR_API:-}" ]]; then
    echo "Adding Prowlarr indexer..."
    EXISTING_IDX=$(api GET /indexer)
    HAS_PROWL=$(echo "$EXISTING_IDX" | python3 -c "
import sys, json
idxs = json.load(sys.stdin)
print('yes' if any('newznab' in i.get('implementation','').lower() or 'prowlarr' in i.get('name','').lower() for i in idxs) else 'no')
")
    if [[ "$HAS_PROWL" == "no" ]]; then
        PROWL_BASE=$(echo "$PROWLARR_URL" | sed 's:/*$::')

        IDX_PAYLOAD="{
            \"enableRss\": true,
            \"enableAutomaticSearch\": true,
            \"enableInteractiveSearch\": true,
            \"protocol\": \"usenet\",
            \"priority\": 25,
            \"downloadClientId\": 0,
            \"name\": \"Prowlarr\",
            \"implementationName\": \"Newznab\",
            \"implementation\": \"Newznab\",
            \"configContract\": \"NewznabSettings\",
            \"tags\": [],
            \"fields\": [
                {\"name\": \"baseUrl\", \"value\": \"${PROWL_BASE}\"},
                {\"name\": \"apiPath\", \"value\": \"/api\"},
                {\"name\": \"apiKey\", \"value\": \"${PROWLARR_API}\"},
                {\"name\": \"categories\", \"value\": [1000]},
                {\"name\": \"additionalParameters\", \"value\": \"\"},
                {\"name\": \"multiLanguages\", \"value\": []}
            ]
        }"
        if api POST "/indexer?forceSave=true" -d "$IDX_PAYLOAD" > /dev/null 2>&1; then
            echo "  ✓ Prowlarr @ ${PROWL_BASE}"
        else
            DISABLED_IDX=$(echo "$IDX_PAYLOAD" | python3 -c "
import sys,json; p=json.load(sys.stdin)
p['enableRss']=False; p['enableAutomaticSearch']=False; p['enableInteractiveSearch']=False
print(json.dumps(p))
")
            api POST /indexer -d "$DISABLED_IDX" > /dev/null
            echo "  ✓ Prowlarr @ ${PROWL_BASE} (disabled — enable after verifying connectivity)"
        fi
    else
        echo "  - Prowlarr (already exists)"
    fi
fi

echo ""
echo "══════════════════════════════════════════"
echo "  Provisioning complete!"
echo "  Web UI: ${BASE_URL}"
echo "  Login:  ${DEFAULT_USERNAME:-admin} / ${DEFAULT_PASSWORD:-admin}"
echo "══════════════════════════════════════════"
