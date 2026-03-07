# Architecture

Technical architecture of Playarr, forked from [Sonarr](https://github.com/Sonarr/Sonarr).

## System Architecture

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Browser    │     │  API Client  │     │   Prowlarr   │
│  React SPA   │     │  (scripts)   │     │  (indexers)  │
└──────┬───────┘     └──────┬───────┘     └──────┬───────┘
       │                    │                    │
       ▼                    ▼                    ▼
┌────────────────────────────────────────────────────────┐
│                    Playarr Host                         │
│                  ASP.NET Core                           │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐             │
│  │ Static   │  │ REST API │  │ SignalR  │             │
│  │ Files    │  │ v3 / v5  │  │ Hub     │             │
│  └──────────┘  └────┬─────┘  └────┬─────┘             │
│                     │              │                    │
│  ┌─────────────────────────────────────────────────┐   │
│  │              Core Business Logic                 │   │
│  │  ┌─────────┐ ┌──────────┐ ┌───────────────┐    │   │
│  │  │  Games  │ │ Download │ │ Notifications │    │   │
│  │  │  ROMs   │ │ Clients  │ │    Import     │    │   │
│  │  │Platform │ │ Indexers │ │  Housekeeping │    │   │
│  │  └─────────┘ └──────────┘ └───────────────┘    │   │
│  └──────────────────────┬──────────────────────────┘   │
│                         │                               │
│  ┌──────────────────────┴──────────────────────────┐   │
│  │            Data Access (Dapper + SQLite)          │   │
│  │  ┌──────────────┐  ┌────────────────────────┐   │   │
│  │  │ Repositories │  │ FluentMigrator (223)   │   │   │
│  │  │ TableMapping │  │ Schema Migrations      │   │   │
│  │  └──────────────┘  └────────────────────────┘   │   │
│  └──────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────┘
       │                    │                    │
       ▼                    ▼                    ▼
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   SQLite /   │     │  File System │     │  Download    │
│  PostgreSQL  │     │  (ROM files) │     │  Clients     │
└──────────────┘     └──────────────┘     └──────────────┘
```

## Component Overview

### Playarr.Host
ASP.NET Core application host. Configures middleware pipeline, authentication, static files, API routing, and SignalR.

### Playarr.Http
HTTP middleware layer. Handles authentication, CORS, error formatting, and request/response pipeline.

### Playarr.Api.V3 / V5
REST API controllers. Each controller maps to a resource (Game, Rom, Calendar, Command, etc.) and delegates to Core services.

### Playarr.Core
All business logic. Contains:
- **Domain models** (`Games/`): Game, Rom, Platform, RomFile
- **Services**: GameService, RomService, DownloadService, etc.
- **Repositories**: Data access via Dapper
- **Providers**: Download clients, indexers, notifications, import lists
- **Housekeeping**: Scheduled cleanup tasks (orphan files, old history, etc.)
- **Datastore**: Database context, table mapping, migrations

### Playarr.Common
Shared utilities across all projects: logging (NLog), HTTP client, disk operations, exceptions, extensions.

### Playarr.SignalR
SignalR hub for real-time push notifications to connected clients (browser, API consumers).

### Playarr.Update
Self-update mechanism. Downloads and applies updates from configured update source.

## Database

### Engine
SQLite by default. PostgreSQL supported for larger deployments.

### ORM
Dapper micro-ORM with convention-based column mapping. Model properties must match database column names exactly.

### Table Mapping
`TableMapping.cs` maps C# classes to database table names:

```csharp
Mapper.Entity<Game>("Series");      // Game class → Series table
Mapper.Entity<Rom>("Episodes");     // Rom class → Episodes table
Mapper.Entity<RomFile>("EpisodeFiles"); // RomFile class → EpisodeFiles table
```

Tables keep original Sonarr names because 223 FluentMigrator migrations define the historical schema using those names.

### Migrations
Sequential numbered migrations in `Datastore/Migration/`. Each migration runs exactly once. Never modify existing migrations — always add new ones.

## Frontend

### Stack
- React 18 with TypeScript
- CSS Modules for scoped styling
- webpack 5 for bundling
- Custom Redux-like state management

### Structure
The SPA is organized by feature:
- `Game/` — Game list, detail, editor
- `AddGame/` — Add new game wizard
- `Rom/` — ROM detail views
- `Platform/` — Platform views
- `Calendar/` — Calendar view
- `Activity/` — Queue, history, blocklist
- `Settings/` — Configuration pages
- `System/` — Status, tasks, logs

### API Communication
Frontend communicates with the backend via:
- REST API (`/api/v3/`) for CRUD operations
- SignalR WebSocket for real-time updates

## Dependency Injection

DryIoc container. Services are registered automatically by convention (interfaces → implementations). Override by explicit registration in `CompositionRoot.cs`.

## Scheduling

Background tasks use a custom scheduler:
- RSS feed sync (configurable interval, default 15 min)
- Refresh metadata (daily by default)
- Housekeeping (daily cleanup tasks)
- Health checks (periodic)
- Download monitoring (frequent polling)
