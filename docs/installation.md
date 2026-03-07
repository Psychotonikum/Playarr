# Installation Guide

## System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| OS | Linux (Debian 11+), Windows 10+, macOS 12+ | Debian 12 / Ubuntu 22.04 |
| Runtime | .NET 10 Runtime | .NET 10 SDK (for building) |
| RAM | 512 MB | 1 GB+ |
| Disk | 200 MB (app) + space for ROMs | SSD recommended |
| Browser | Any modern browser | Chrome, Firefox, Safari |

## Installation Methods

### 1. From Source (Recommended for Development)

```bash
# Clone the repository
git clone https://github.com/Psychotonikum/playarr.git
cd playarr

# Install .NET 10 SDK (if not installed)
wget https://dot.net/v1/dotnet-install.sh
bash dotnet-install.sh --channel 10.0
export PATH="$HOME/.dotnet:$PATH"

# Install Node.js 20 and Yarn
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo bash -
sudo apt-get install -y nodejs
npm install -g yarn

# Build frontend
yarn install
yarn build

# Build backend
dotnet restore src/Playarr.sln
dotnet build src/Playarr.sln

# Run
dotnet run --project src/Playarr/Playarr.csproj
```

The web UI will be available at `http://localhost:9797`.

### 2. Docker

```bash
docker run -d \
  --name playarr \
  -p 9797:9797 \
  -v /path/to/config:/config \
  -v /path/to/roms:/roms \
  -e PUID=1000 \
  -e PGID=1000 \
  -e TZ=America/New_York \
  playarr/playarr:latest
```

#### Docker Compose

```yaml
version: "3"
services:
  playarr:
    image: playarr/playarr:latest
    container_name: playarr
    ports:
      - "9797:9797"
    volumes:
      - ./config:/config
      - /path/to/roms:/roms
    environment:
      - PUID=1000
      - PGID=1000
      - TZ=America/New_York
    restart: unless-stopped
```

### 3. Debian/Ubuntu (systemd)

```bash
# From the project root
cd distribution/debian
sudo bash install.sh

# The service is now running on port 9797
sudo systemctl status playarr

# Manage the service
sudo systemctl start playarr
sudo systemctl stop playarr
sudo systemctl restart playarr

# View logs
journalctl -u playarr -f
```

### 4. Manual (start.sh)

For quick testing or debugging:

```bash
cd playarr
bash start.sh
```

## Post-Installation

1. Open `http://localhost:9797` in your browser
2. Set up your root folder (where ROMs are stored)
3. Configure a download client (Settings > Download Clients)
4. Add an indexer (Settings > Indexers)
5. Add your first game (Games > Add New)

## Changing the Port

Edit `config.xml` in your config directory:

```xml
<Config>
  <Port>9797</Port>
</Config>
```

Or pass it as a command-line argument:

```bash
dotnet run --project src/Playarr/Playarr.csproj -- --port=9797
```

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

### Apache

```apache
<Location /playarr>
    ProxyPass http://127.0.0.1:9797/playarr
    ProxyPassReverse http://127.0.0.1:9797/playarr
</Location>
```

## Updating

### From Source

```bash
cd playarr
git pull
yarn install && yarn build
dotnet build src/Playarr.sln
# Restart the application
```

### Docker

```bash
docker pull playarr/playarr:latest
docker stop playarr && docker rm playarr
# Re-run with the same docker run command
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Port already in use | Change port in config.xml or kill the conflicting process |
| Permission denied | Check file ownership. Use PUID/PGID in Docker |
| Database locked | Only one instance of Playarr can run at a time |
| White screen on UI | Clear browser cache or rebuild frontend with `yarn build` |
| Can't connect to download client | Verify the client's host, port, and API key in Settings |
