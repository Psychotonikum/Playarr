# Playarr

**A ROM management system for retro gaming enthusiasts.** Playarr helps you organize, rename, and manage your game ROM collections across platforms. Built on the Servarr stack (Sonarr/Radarr architecture), it provides a familiar, powerful web UI for managing ROMs.

## Overview

Playarr monitors your ROM sources, automatically organizes files by game and platform, renames them to a clean standard, and keeps your library up to date. If you've used Sonarr or Radarr, you'll feel right at home.

| | |
|---|---|
| **Default Port** | `9797` |
| **Web UI** | `http://localhost:9797` |
| **API Base** | `http://localhost:9797/api/v3` |
| **Tech Stack** | .NET 10, ASP.NET Core, SQLite, React/TypeScript |

## Features

- **Multi-Platform ROM Management** — Organize ROMs by game and platform (NES, SNES, Genesis, PS1, etc.)
- **Automatic File Renaming** — Configurable naming schemes for consistent library organization
- **Quality Upgrades** — Automatically replace ROMs when better dumps become available
- **Download Client Integration** — Full integration with SABnzbd, NZBGet, and torrent clients
- **Media Server Integration** — Notifications and library updates for Kodi, Plex, Emby, Jellyfin
- **Import Lists** — Bulk-import games from external lists or other Playarr instances
- **Calendar View** — Track upcoming game releases and new ROM availability
- **Custom Formats** — Define quality profiles and format preferences for ROM files
- **Beautiful Web UI** — Responsive, full-featured SPA that works on desktop and mobile
- **REST API** — Complete API for automation and third-party integrations
- **Cross-Platform** — Runs on Windows, Linux, macOS, Raspberry Pi, Docker

## Quick Start

### Prerequisites

- [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) (for building from source)
- [Yarn](https://yarnpkg.com/) (for building from source)

### Run from Source

```bash
git clone https://github.com/Psychotonikum/playarr.git
cd playarr

# Install frontend dependencies and build
yarn install
yarn build

# Build and run the backend
dotnet build src/Playarr.sln
dotnet run --project src/Playarr/Playarr.csproj
```

Open `http://localhost:9797` in your browser.

### Docker

```bash
docker run -d \
  --name playarr \
  -p 9797:9797 \
  -v /path/to/config:/config \
  -v /path/to/roms:/roms \
  playarr/playarr:latest
```

### Debian/Ubuntu (systemd)

```bash
cd distribution/debian
sudo bash install.sh
# Service runs on port 9797, managed via: sudo systemctl {start|stop|status} playarr
```

## Configuration

Configuration file is located at:
- **Linux**: `~/.config/Playarr/config.xml`
- **Windows**: `%AppData%\Playarr\config.xml`
- **macOS**: `~/.config/Playarr/config.xml`

Key settings:

| Setting | Default | Description |
|---------|---------|-------------|
| `Port` | `9797` | Web UI and API port |
| `BindAddress` | `*` | Network interface to bind to |
| `EnableSsl` | `false` | Enable HTTPS |
| `ApiKey` | (auto-generated) | API authentication key |

## Domain Model

Playarr maps the Servarr concepts to the ROM/gaming domain:

| Servarr Concept | Playarr Concept | Description |
|-----------------|-----------------|-------------|
| Series | **Game** | A game title (e.g., "Super Mario Bros.") |
| Season | **Platform** | A gaming platform (e.g., NES, SNES, Genesis) |
| Episode | **ROM** | An individual ROM file |
| Episode File | **ROM File** | Physical ROM file on disk |

## API

Playarr exposes a full REST API at `/api/v3`. All requests require the `X-Api-Key` header.

```bash
# List all games
curl -H "X-Api-Key: YOUR_API_KEY" http://localhost:9797/api/v3/game

# Get a specific game
curl -H "X-Api-Key: YOUR_API_KEY" http://localhost:9797/api/v3/game/1

# System status
curl -H "X-Api-Key: YOUR_API_KEY" http://localhost:9797/api/v3/system/status
```

See [docs/api.md](docs/api.md) for the full API reference.

## Project Structure

```
playarr/
 src/
   ├── Playarr/                  # Main application entry point
   ├── Playarr.Core/             # Business logic, domain models, data access
   │   ├── Games/                # Game, ROM, Platform, RomFile models
   │   ├── Datastore/            # SQLite database, migrations (FluentMigrator)
   │   ├── Download/             # Download client integrations
   │   ├── ImportLists/          # Import list providers
   │   └── Notifications/        # Notification providers
   ├── Playarr.Api.V3/           # REST API controllers (v3)
   ├── Playarr.Api.V5/           # REST API controllers (v5)
   ├── Playarr.Host/             # ASP.NET Core hosting, startup
   ├── Playarr.Http/             # HTTP middleware, authentication
   ├── Playarr.Common/           # Shared utilities, exceptions
   ├── Playarr.SignalR/          # Real-time push notifications
   └── Playarr.Update/           # Self-update mechanism
 frontend/
   └── src/                      # React/TypeScript SPA
       ├── Game/                 # Game management views
       ├── AddGame/              # Add new game workflow
       ├── Rom/                  # ROM detail views
       ├── Platform/             # Platform management
       ├── Calendar/             # Calendar view
       ├── Settings/             # Settings pages
       └── System/               # System status and logs
 distribution/                 # Packaging (Debian, macOS, Windows)
 docker/                       # Docker build files
 docs/                         # Documentation
```

## Development

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup.

```bash
# Backend: build & run
dotnet build src/Playarr.sln
dotnet run --project src/Playarr/Playarr.csproj

# Frontend: dev mode with hot reload
yarn start

# Run tests
dotnet test src/Playarr.Core.Test/Playarr.Core.Test.csproj
```

## License

- [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
- Copyright 2024-2026 — Forked from [Sonarr](https://github.com/Sonarr/Sonarr)

## Credits and Attribution

Playarr is built on the foundation of the excellent Servarr project family:

- **[Sonarr](https://github.com/Sonarr/Sonarr)** — The core architecture, backend framework, and API design
- **[Radarr](https://github.com/Radarr/Radarr)** — Additional design patterns and quality management features
- **[Servarr](https://github.com/Servarr)** — The overall ecosystem and shared libraries

We are grateful to the Servarr community and all contributors to these projects for creating the robust, maintainable codebase that Playarr builds upon.

Additional thanks to:
- **IGDB (Internet Game Database)** — Game metadata and information
- **All open-source contributors** — See git history for detailed contributions
