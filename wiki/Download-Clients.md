# Download Clients

Playarr supports a wide range of Usenet and torrent download clients.

## Supported Clients

### Usenet
| Client | Notes |
|--------|-------|
| **SABnzbd** | Recommended. Full API support |
| **NZBGet** | Full API support |
| **NZBVortex** | macOS only |
| **Pneumatic** | Strm file generation |

### Torrent
| Client | Notes |
|--------|-------|
| **qBittorrent** | Recommended. Full API support |
| **Transmission** | Full API support |
| **Deluge** | Requires WebUI plugin |
| **rTorrent** | Via XML-RPC |
| **uTorrent** | Legacy support |
| **Vuze** | Requires WebUI plugin |
| **Aria2** | Via JSON-RPC |
| **Flood** | Full API support |
| **Download Station** | Synology NAS |

## Adding a Download Client

1. Go to **Settings > Download Clients**
2. Click the **+** button
3. Select your client from the list
4. Fill in the connection details:

### Common Settings

| Setting | Description |
|---------|-------------|
| **Name** | Friendly name for this client |
| **Enable** | Toggle this client on/off |
| **Host** | IP address or hostname of the client |
| **Port** | Client's API/web port |
| **Use SSL** | Connect via HTTPS |
| **Username** | Authentication username (if required) |
| **Password** | Authentication password (if required) |
| **API Key** | API key (SABnzbd, NZBGet) |
| **Category** | Category/label for Playarr downloads |
| **Priority** | Download priority |
| **Remove Completed** | Remove from client after import |

5. Click **Test** to verify the connection
6. Click **Save**

## SABnzbd Setup

1. In SABnzbd, go to **Config > General** and note the API Key
2. In Playarr, add a new Usenet download client > SABnzbd
3. Enter:
   - Host: `localhost` (or your SABnzbd IP)
   - Port: `8080` (SABnzbd default)
   - API Key: Your SABnzbd API key
   - Category: `playarr` (will be auto-created)

## qBittorrent Setup

1. In qBittorrent, enable the Web UI (**Tools > Preferences > Web UI**)
2. Set a username and password
3. In Playarr, add a new Torrent download client > qBittorrent
4. Enter:
   - Host: `localhost`
   - Port: `8080` (qBittorrent Web UI default)
   - Username/Password: Your Web UI credentials
   - Category: `playarr`

## Remote Path Mappings

If your download client and Playarr see files at different paths (e.g., Docker containers, remote machines), configure remote path mappings:

1. Go to **Settings > Download Clients** (scroll to bottom)
2. Add a mapping:
   - **Host**: The download client's host
   - **Remote Path**: Path as the download client sees it (e.g., `/downloads/`)
   - **Local Path**: Path as Playarr sees it (e.g., `/data/downloads/`)

## Completed Download Handling

Configure in **Settings > Download Clients**:

| Setting | Description |
|---------|-------------|
| **Enable** | Auto-import completed downloads |
| **Remove** | Remove from client after successful import |
| **Check For Finished Downloads** | How often to check (minutes) |

## Failed Download Handling

When a download fails:

1. Playarr marks the release as failed
2. Adds it to the blocklist (won't try the same release again)
3. Searches for an alternative release
4. Downloads the alternative

Configure in **Settings > Download Clients**:
- **Redownload** — Automatically search for alternatives
- **Remove** — Remove failed downloads from the client
