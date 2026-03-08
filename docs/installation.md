# Installation Guide

## System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| OS | Debian 11+, Ubuntu 20.04+, Windows 10+, macOS 12+ | Debian 12 / Ubuntu 22.04 |
| Runtime | .NET 10 Runtime | .NET 10 SDK (for building from source) |
| RAM | 512 MB | 1 GB+ |
| Disk | 200 MB (application) + ROM storage | SSD for database |

## Method 1: Debian/Ubuntu (Recommended)

The install script sets up Playarr as a systemd service. It handles both fresh installs and updates.

```bash
# Download and run the install script
cd /tmp
curl -sLO https://raw.githubusercontent.com/Psychotonikum/playarr/main/distribution/debian/install.sh
sudo bash install.sh
```

**Options:**

```bash
# Unattended mode (for automation)
sudo bash install.sh --user playarr --group media -u

# Custom user/group
sudo bash install.sh --user myuser --group mygroup

# Help
bash install.sh --help
```

**Managing the service:**

```bash
sudo systemctl start playarr
sudo systemctl stop playarr
sudo systemctl restart playarr
sudo systemctl status playarr
journalctl -u playarr -f    # View logs
```

**Updating:**

Run the install script again. It detects the existing installation, stops the service, replaces the binaries, and restarts.

## Method 2: Docker

```bash
docker run -d \
  --name playarr \
  -p 9797:9797 \
  -v /path/to/config:/config \
  -v /path/to/roms:/roms \
  --restart unless-stopped \
  playarr/playarr:latest
```

**Docker Compose:**

```yaml
services:
  playarr:
    image: playarr/playarr:latest
    container_name: playarr
    ports:
      - "9797:9797"
    volumes:
      - ./config:/config
      - /path/to/roms:/roms
    restart: unless-stopped
```

## Method 3: From Source

Requires .NET 10 SDK, Node.js 20+, and Yarn.

```bash
git clone https://github.com/Psychotonikum/playarr.git
cd playarr

# Automated setup (Debian/Ubuntu)
sudo bash scripts/setup-dev.sh

# Or manual steps:
yarn install && yarn build
dotnet msbuild -restore src/Playarr.sln -p:Configuration=Debug -p:Platform=Posix
./_output/net10.0/Playarr
```

## Post-Installation

1. Open **http://localhost:9797** (or your server's IP)
2. Go to **Settings > Metadata Source** and enter your Twitch/IGDB API credentials
3. Go to **Settings > Media Management** and add a root folder for ROMs
4. Go to **Settings > Game Systems** and add the platforms you manage
5. Click **+ Add Game** to search and add your first game

## Configuration

Configuration is stored in `config.xml`:

| OS | Location |
|----|----------|
| Linux (systemd) | `/var/lib/playarr/config.xml` |
| Linux (manual) | `~/.config/Playarr/config.xml` |
| Docker | `/config/config.xml` |
| Windows | `%AppData%\Playarr\config.xml` |

Key settings:

| Setting | Default | Description |
|---------|---------|-------------|
| `Port` | `9797` | Web UI / API port |
| `BindAddress` | `*` | Network interface |
| `ApiKey` | (auto) | API authentication key |
| `AuthenticationMethod` | `None` | `None`, `Basic`, `Forms`, `External` |

## Reverse Proxy

### Nginx

```nginx
location /playarr {
    proxy_pass http://127.0.0.1:9797;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection $http_connection;
}
```

Set `UrlBase` to `/playarr` in `config.xml` when using a reverse proxy with a sub-path.

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Won't start | Check port availability, review logs, verify .NET 10 runtime |
| Blank UI | Clear browser cache, rebuild frontend with `yarn build` |
| Database locked | Kill duplicate Playarr processes |
| Game search fails | Verify Twitch/IGDB credentials in Settings > Metadata Source |
