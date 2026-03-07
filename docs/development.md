# Development Guide

## Architecture Overview

Playarr is a fork of [Sonarr](https://github.com/Sonarr/Sonarr), adapted for ROM management. It follows the Servarr architecture:

```
┌─────────────────────────────────────────────┐
│                 Web Browser                  │
│            React/TypeScript SPA              │
├─────────────────────────────────────────────┤
│              REST API (v3/v5)                │
│           Playarr.Api.V3 / V5               │
├─────────────────────────────────────────────┤
│          ASP.NET Core Host                   │
│     Playarr.Host / Playarr.Http             │
├─────────────────────────────────────────────┤
│            Core Business Logic               │
│             Playarr.Core                     │
│  ┌─────────┬──────────┬──────────────────┐  │
│  │  Games  │ Download │  Notifications   │  │
│  │  ROMs   │ Clients  │  Import Lists    │  │
│  │Platform │ Indexers  │  Housekeeping    │  │
│  └─────────┴──────────┴──────────────────┘  │
├─────────────────────────────────────────────┤
│            Data Access Layer                 │
│    Dapper ORM + FluentMigrator + SQLite      │
└─────────────────────────────────────────────┘
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10, ASP.NET Core |
| Frontend | React 18, TypeScript, webpack 5 |
| Database | SQLite (default), PostgreSQL (optional) |
| ORM | Dapper (micro-ORM) |
| Migrations | FluentMigrator |
| Real-time | SignalR |
| DI Container | DryIoc |
| Testing | NUnit, Moq, FluentAssertions |

## Prerequisites

- .NET 10 SDK
- Node.js 20+
- Yarn 1.x
- Git

## Getting Started

```bash
git clone https://github.com/Psychotonikum/playarr.git
cd playarr

# Restore NuGet packages
dotnet restore src/Playarr.sln

# Build backend
dotnet build src/Playarr.sln

# Install frontend dependencies
yarn install

# Build frontend (production)
yarn build

# Or run frontend dev server (hot reload on port 9797)
yarn start
```

## Project Structure

```
src/
├── Playarr/                    # Entry point (Program.cs)
├── Playarr.Host/               # ASP.NET Core setup, Startup.cs, middleware
├── Playarr.Http/               # HTTP pipeline, auth, error handling
├── Playarr.Api.V3/             # REST API v3 controllers & resources
├── Playarr.Api.V5/             # REST API v5 controllers & resources
├── Playarr.Core/               # All business logic
│   ├── Games/                  # Game, Rom, Platform, RomFile domain models
│   ├── Datastore/              # DB context, repositories, table mapping
│   │   └── Migration/          # 223 FluentMigrator migration files
│   ├── Download/               # Download client providers
│   ├── Indexers/               # Indexer providers (Newznab, Torznab, etc.)
│   ├── ImportLists/            # Import list providers
│   ├── MediaFiles/             # File management, renaming, importing
│   ├── Notifications/          # Notification providers (Plex, Kodi, email, etc.)
│   ├── Housekeeping/           # Scheduled cleanup tasks
│   ├── Configuration/          # Config file management
│   ├── Profiles/               # Quality & language profiles
│   └── Validation/             # Custom validators
├── Playarr.Common/             # Shared utilities, exceptions, logging
├── Playarr.SignalR/            # SignalR hub for real-time updates
├── Playarr.Update/             # Self-update mechanism
├── Playarr.Mono/               # Mono-specific implementations
├── Playarr.Windows/            # Windows-specific implementations
├── Playarr.RuntimePatches/     # Runtime patches for compatibility
├── Playarr.Core.Test/          # Core unit tests (NUnit)
├── Playarr.Host.Test/          # Host tests
├── Playarr.Common.Test/        # Common library tests
├── Playarr.Api.Test/           # API tests
├── Playarr.Integration.Test/   # Integration tests
├── Playarr.Libraries.Test/     # Third-party library tests
└── Playarr.Test.Common/        # Shared test utilities

frontend/
└── src/
    ├── Game/                   # Game list, details, editor views
    ├── AddGame/                # Add new game workflow
    ├── Rom/                    # ROM detail views
    ├── Platform/               # Platform management
    ├── Calendar/               # Calendar view
    ├── Activity/               # Queue, history, blocklist
    ├── Settings/               # All settings pages
    ├── System/                 # System status, tasks, logs, updates
    ├── Components/             # Shared UI components
    ├── Store/                  # Redux-like state management
    └── Helpers/                # Utility functions
```

## Domain Model Mapping

Playarr keeps Sonarr's database schema but maps domain concepts:

| C# Class | DB Table | Sonarr Equivalent | Description |
|----------|----------|-------------------|-------------|
| `Game` | `Series` | Series | A game title |
| `Platform` | (computed) | Season | A gaming platform |
| `Rom` | `Episodes` | Episode | An individual ROM |
| `RomFile` | `EpisodeFiles` | EpisodeFile | Physical file on disk |

**Important**: Model properties that map to database columns use the **original Sonarr column names** (e.g., `TvdbId`, `SeriesId`, `SeasonNumber`, `EpisodeNumber`). This is because Dapper uses convention-based mapping, and the FluentMigrator migrations create tables with the original Sonarr schema.

The `TableMapping.cs` file maps C# classes to DB table names:
```csharp
Mapper.Entity<Game>("Series");
Mapper.Entity<Rom>("Episodes");
Mapper.Entity<RomFile>("EpisodeFiles");
```

## Database Migrations

Migrations are in `src/Playarr.Core/Datastore/Migration/`. There are 223 migration files following Sonarr's historical schema evolution.

**Critical rule**: Never rename table or column names in migration files. They must keep original Sonarr names because they represent historical schema steps.

To add a new migration:

```csharp
// src/Playarr.Core/Datastore/Migration/224_add_your_feature.cs
[Migration(224)]
public class AddYourFeature : PlayarrMigrationBase
{
    protected override void MainDbUpgrade()
    {
        Create.Column("NewColumn").OnTable("Series").AsString().Nullable();
    }
}
```

## Running Tests

```bash
# All Core tests
dotnet test src/Playarr.Core.Test/Playarr.Core.Test.csproj

# Specific test
dotnet test src/Playarr.Core.Test/Playarr.Core.Test.csproj --filter "ClassName.TestName"

# All tests in solution
dotnet test src/Playarr.sln
```

Expected results: ~4800+ pass, ~200 fail (network/integration tests that need external services).

## Frontend Development

```bash
# Development server with hot reload
yarn start
# Proxies API requests to http://localhost:9797

# Production build
yarn build
# Output goes to frontend/build/

# Type checking
yarn tsc

# Linting
yarn lint
```

The frontend is a React SPA using:
- TypeScript for type safety
- CSS Modules for scoped styles
- webpack 5 for bundling
- Custom state management (Redux-like pattern)

## Code Style

- Backend: StyleCop analyzer rules (see `.editorconfig`)
- Frontend: ESLint + TypeScript strict mode
- 4-space indentation everywhere
- `var` preferred over explicit types in C#
- Using directives at top of file (SA1200 suppressed)

## Building for Release

```bash
# Backend publish (self-contained for Linux x64)
dotnet publish src/Playarr/Playarr.csproj -c Release -r linux-x64 --self-contained

# Frontend production build
yarn build
```

## Adding a New Feature

1. **Domain model**: Add/modify classes in `Playarr.Core/Games/`
2. **Repository**: Add data access in `Playarr.Core/Datastore/`
3. **Service**: Add business logic service in `Playarr.Core/`
4. **Migration**: Add database migration if schema changes
5. **API**: Add controller/resource in `Playarr.Api.V3/`
6. **Frontend**: Add React components in `frontend/src/`
7. **Tests**: Add NUnit tests in `Playarr.Core.Test/`
