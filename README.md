# Playarr

<p align="center">
  <img src="Logo/1024.png" alt="Playarr Logo" width="200">
</p>

**Playarr** is a game ROM management system built on the [Sonarr](https://github.com/Sonarr/Sonarr)/Servarr architecture. It organizes, renames, and manages game ROM collections through a web UI, using [IGDB](https://www.igdb.com/) for game metadata.

> **Status**: Early development. Playarr is functional but under active development. Expect rough edges.

## Features

- **Game Library Management** — Organize ROMs by game and platform (NES, SNES, Genesis, PS1, Switch, etc.)
- **IGDB Metadata** — Automatic game artwork, descriptions, ratings, and platform info from IGDB
- **File Renaming** — Configurable naming schemes for clean library organization
- **Download Client Integration** — SABnzbd, NZBGet, qBittorrent, Transmission, Deluge, and more
- **Calendar** — Track upcoming game releases from IGDB and Metacritic
- **Quality Profiles** — Define preferences for ROM file quality and format
- **ROM Verification** — Validate ROM integrity using [No-Intro](https://no-intro.org/) and [Redump](http://redump.org/) DAT databases
- **Game System Presets** — Built-in definitions for 25+ retro and modern platforms
- **Metacritic Scores** — Optional alternative rating source
- **REST API** — Full API at `/api/v3` for automation and integrations
- **Web UI** — Responsive React/TypeScript interface

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- [Yarn](https://yarnpkg.com/) (v1.x)

### From Source

```bash
git clone https://github.com/Psychotonikum/playarr.git
cd playarr

# Set up the development environment (installs all dependencies)
sudo bash scripts/setup-dev.sh

# Or manually:
yarn install && yarn build
dotnet build src/Playarr.sln

# Run
./_output/net10.0/Playarr
```

Open **http://localhost:9797** in your browser.

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

**From GitHub release** (once a release is published):

```bash
# Clone the repo and run the installer (downloads latest release automatically)
git clone https://github.com/Psychotonikum/playarr.git
cd playarr
sudo bash distribution/debian/install.sh
```

**From a locally built tarball** (for private repo / no releases yet):

```bash
git clone https://github.com/Psychotonikum/playarr.git
cd playarr

# Build a release tarball
bash scripts/build-release.sh --skip-frontend

# Install from the local tarball
sudo bash distribution/debian/install.sh --tarball _artifacts/Playarr.*.linux-x64.tar.gz
```

The service runs on port **9797** and is managed via systemd:

```bash
sudo systemctl {start|stop|restart|status} playarr
```

## Configuration

After first launch, configure Playarr through the web UI at `http://localhost:9797`:

1. **Settings > Metadata Source** — Enter your [Twitch/IGDB API credentials](https://api-docs.igdb.com/#account-creation) (required for game search)
2. **Settings > Media Management** — Add a root folder for your ROM library
3. **Settings > Game Systems** — Add the platforms you want to manage
4. **Games > Add New** — Search for a game by name and add it

Configuration file location:

| OS | Path |
|----|------|
| Linux | `~/.config/Playarr/config.xml` |
| Docker | `/config/config.xml` |
| Windows | `%AppData%\Playarr\config.xml` |

## Development

```bash
# Build backend
dotnet msbuild -restore src/Playarr.sln -p:Configuration=Debug -p:Platform=Posix

# Build frontend
yarn build

# Frontend dev mode (hot reload)
yarn start

# Run unit tests (excludes integration tests that need external services)
dotnet test src/Playarr.sln --filter 'Category!=IntegrationTest&Category!=AutomationTest&Category!=ManualTest'

# Run a specific test project
dotnet test src/Playarr.Core.Test/Playarr.Core.Test.csproj --no-build --filter 'Category!=IntegrationTest'
```

See [docs/development.md](docs/development.md) for the full development guide.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10, ASP.NET Core |
| Frontend | React 18, TypeScript, webpack 5 |
| Database | SQLite (default), PostgreSQL (optional) |
| ORM | Dapper |
| Migrations | FluentMigrator |
| Real-time | SignalR |
| Testing | NUnit, Moq, FluentAssertions |
| Metadata | IGDB (via [igdb-dotnet](https://github.com/kamranayub/igdb-dotnet)), Metacritic |
| ROM Verification | [No-Intro](https://no-intro.org/), [Redump](http://redump.org/) |

## Project Structure

```
src/
 Playarr/               # Application entry point
 Playarr.Core/          # Business logic, domain models, data access
   ├── Games/             # Game, Rom, Platform, GameSystem models
   ├── MetadataSource/    # IGDB and Metacritic integrations
   └── Datastore/         # DB repositories, 223+ migrations
 Playarr.Api.V3/        # REST API controllers
 Playarr.Host/          # ASP.NET Core hosting
 Playarr.Http/          # HTTP middleware, authentication
 Playarr.Common/        # Shared utilities
 Playarr.SignalR/       # Real-time notifications

frontend/src/              # React/TypeScript SPA
distribution/              # Packaging (Debian, Docker, macOS, Windows)
scripts/                   # Build, test, and dev setup scripts
```

## Domain Model

Playarr reuses the Sonarr database schema, mapping TV concepts to gaming:

| Sonarr | Playarr | DB Table |
|--------|---------|----------|
| Series | Game | `Series` |
| Season | Platform | (computed) |
| Episode | ROM | `Episodes` |
| EpisodeFile | RomFile | `EpisodeFiles` |

## License

[GNU General Public License v3.0](LICENSE.md)

## Credits & Attribution

Playarr is a derivative work of [Sonarr](https://github.com/Sonarr/Sonarr) (GPL v3).

Third-party components:
- **[Sonarr](https://github.com/Sonarr/Sonarr)** — Original codebase (GPL v3). Copyright 2010-2017 Mark McDowall, Keivan Beigi, Taloth Saldono and contributors.
- **[igdb-dotnet](https://github.com/kamranayub/igdb-dotnet)** — IGDB API client (Apache 2.0) by Kamran Ayub
- **[AeroFoil](https://github.com/luketanti/AeroFoil)** — Inspiration for ROM file name parsing conventions
- **[No-Intro](https://no-intro.org/)** — Cartridge-based ROM verification databases
- **[Redump](http://redump.org/)** — Disc-based ROM preservation databases

See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) and [COPYRIGHT.md](COPYRIGHT.md) for full details.
