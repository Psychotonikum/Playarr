# Docker Setup

## Quick Start

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

## Docker Compose

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
      - /path/to/downloads:/downloads  # Download client output
    environment:
      - PUID=1000
      - PGID=1000
      - TZ=America/New_York
    restart: unless-stopped
```

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `PUID` | `1000` | User ID for file permissions |
| `PGID` | `1000` | Group ID for file permissions |
| `TZ` | `UTC` | Timezone (e.g., `America/New_York`, `Europe/Berlin`) |

## Volumes

| Container Path | Description |
|----------------|-------------|
| `/config` | Application data, database, logs |
| `/roms` | ROM file storage (root folder) |
| `/downloads` | Download client completed folder (if needed) |

## Full Stack with Docker Compose

Run Playarr alongside download clients and Prowlarr:

```yaml
version: "3"
services:
  playarr:
    image: playarr/playarr:latest
    container_name: playarr
    ports:
      - "9797:9797"
    volumes:
      - ./playarr-config:/config
      - /data/roms:/roms
      - /data/downloads:/downloads
    environment:
      - PUID=1000
      - PGID=1000
      - TZ=America/New_York
    restart: unless-stopped

  prowlarr:
    image: linuxserver/prowlarr:latest
    container_name: prowlarr
    ports:
      - "9696:9696"
    volumes:
      - ./prowlarr-config:/config
    environment:
      - PUID=1000
      - PGID=1000
      - TZ=America/New_York
    restart: unless-stopped

  qbittorrent:
    image: linuxserver/qbittorrent:latest
    container_name: qbittorrent
    ports:
      - "8080:8080"
      - "6881:6881"
    volumes:
      - ./qbit-config:/config
      - /data/downloads:/downloads
    environment:
      - PUID=1000
      - PGID=1000
      - TZ=America/New_York
    restart: unless-stopped

  sabnzbd:
    image: linuxserver/sabnzbd:latest
    container_name: sabnzbd
    ports:
      - "8081:8080"
    volumes:
      - ./sab-config:/config
      - /data/downloads:/downloads
    environment:
      - PUID=1000
      - PGID=1000
      - TZ=America/New_York
    restart: unless-stopped
```

## Networking Tips

### Container-to-Container Communication

When services are in the same Docker Compose file, use container names as hostnames:

- In Playarr's download client settings, use `qbittorrent` as the host (not `localhost`)
- In Prowlarr, use `playarr` as the Playarr host

### Host Network Mode

If you have trouble with container networking:

```yaml
services:
  playarr:
    image: playarr/playarr:latest
    network_mode: host
    # No 'ports' needed with host networking
```

## Updating

```bash
# Pull latest image
docker pull playarr/playarr:latest

# Recreate container
docker stop playarr && docker rm playarr
# Re-run your docker run command

# Or with Docker Compose:
docker compose pull
docker compose up -d
```

## Backup

Your config is stored in the `/config` volume. Back it up regularly:

```bash
# Stop the container first for consistency
docker stop playarr
tar -czf playarr-backup-$(date +%Y%m%d).tar.gz /path/to/config
docker start playarr
```
