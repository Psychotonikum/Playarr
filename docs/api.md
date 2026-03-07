# API Reference

Playarr provides a full REST API compatible with the Servarr API standard (v3). All endpoints require authentication via the `X-Api-Key` header.

## Authentication

Every request must include your API key:

```
X-Api-Key: your-api-key-here
```

Find your API key in **Settings > General > Security**.

## Base URL

```
http://localhost:9797/api/v3
```

## Response Format

All responses are JSON. Successful responses return `200 OK` with the resource body. Errors return appropriate HTTP status codes with an error message array.

---

## Game

Manage game entries in your library.

### `GET /api/v3/game`

List all games.

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `tvdbId` | int | Filter by TVDB ID |
| `includeGameFile` | bool | Include associated ROM files |
| `includeImages` | bool | Include cover images |

**Response:** `200 OK`

```json
[
  {
    "id": 1,
    "title": "Super Mario Bros.",
    "sortTitle": "super mario bros",
    "status": "continuing",
    "overview": "Classic platformer by Nintendo",
    "platformCount": 3,
    "romCount": 12,
    "romFileCount": 10,
    "images": [],
    "path": "/roms/Super Mario Bros",
    "qualityProfileId": 1,
    "monitored": true,
    "added": "2026-01-15T10:30:00Z",
    "tags": [1, 3]
  }
]
```

### `GET /api/v3/game/{id}`

Get a single game by ID.

### `POST /api/v3/game`

Add a new game.

**Request Body:**

```json
{
  "tvdbId": 12345,
  "title": "The Legend of Zelda",
  "qualityProfileId": 1,
  "rootFolderPath": "/roms",
  "monitored": true,
  "addOptions": {
    "searchForMissingRoms": true
  },
  "tags": []
}
```

**Response:** `201 Created`

### `PUT /api/v3/game/{id}`

Update an existing game.

### `DELETE /api/v3/game/{id}`

Delete a game. Query parameter `deleteFiles=true` to also remove ROM files from disk.

---

## ROM (Episode)

Manage individual ROMs within a game.

### `GET /api/v3/rom`

List ROMs. At least one filter is required.

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `gameId` | int | Filter by game ID |
| `platformNumber` | int | Filter by platform number |
| `includeRomFile` | bool | Include associated file info |

**Response:** `200 OK`

```json
[
  {
    "id": 42,
    "gameId": 1,
    "platformNumber": 1,
    "romNumber": 1,
    "title": "Super Mario Bros. (USA)",
    "hasFile": true,
    "monitored": true,
    "romFileId": 15
  }
]
```

### `GET /api/v3/rom/{id}`

Get a single ROM by ID.

### `PUT /api/v3/rom/{id}`

Update a ROM (e.g., toggle monitoring).

### `PUT /api/v3/rom/monitor`

Bulk update ROM monitoring status.

**Request Body:**

```json
{
  "romIds": [1, 2, 3],
  "monitored": true
}
```

---

## ROM File (Episode File)

Manage physical ROM files on disk.

### `GET /api/v3/romfile`

List ROM files.

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `gameId` | int | Filter by game ID |

### `GET /api/v3/romfile/{id}`

Get a specific ROM file.

### `DELETE /api/v3/romfile/{id}`

Delete a ROM file from disk.

---

## Calendar

### `GET /api/v3/calendar`

Get ROMs with upcoming release dates.

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `start` | datetime | Start of date range (ISO 8601) |
| `end` | datetime | End of date range (ISO 8601) |
| `includeGame` | bool | Include parent game data |
| `includeRomFile` | bool | Include file data |

---

## Command

Trigger system commands (search, scan, rename, etc.).

### `POST /api/v3/command`

Execute a command.

**Common Commands:**

| Command Name | Parameters | Description |
|-------------|------------|-------------|
| `RomSearch` | `romIds: [int]` | Search for specific ROMs |
| `GameSearch` | `gameId: int` | Search all ROMs for a game |
| `PlatformSearch` | `gameId: int, platformNumber: int` | Search ROMs for a platform |
| `RefreshGame` | `gameId: int` | Refresh game metadata |
| `RenameFiles` | `gameId: int, files: [int]` | Rename ROM files on disk |
| `RescanGame` | `gameId: int` | Rescan game folder |
| `MissingRomSearch` | | Search for all missing monitored ROMs |
| `RssSync` | | Trigger an RSS feed sync |

**Example:**

```json
{
  "name": "GameSearch",
  "gameId": 1
}
```

### `GET /api/v3/command/{id}`

Check the status of a running command.

---

## Quality Profile

### `GET /api/v3/qualityprofile`

List all quality profiles.

### `GET /api/v3/qualityprofile/{id}`

Get a specific quality profile.

### `POST /api/v3/qualityprofile`

Create a quality profile.

### `PUT /api/v3/qualityprofile/{id}`

Update a quality profile.

### `DELETE /api/v3/qualityprofile/{id}`

Delete a quality profile.

---

## Indexer

### `GET /api/v3/indexer`

List all indexers.

### `POST /api/v3/indexer`

Add a new indexer.

### `PUT /api/v3/indexer/{id}`

Update an indexer.

### `DELETE /api/v3/indexer/{id}`

Delete an indexer.

### `POST /api/v3/indexer/test`

Test an indexer configuration.

---

## Download Client

### `GET /api/v3/downloadclient`

List all download clients.

### `POST /api/v3/downloadclient`

Add a new download client.

### `PUT /api/v3/downloadclient/{id}`

Update a download client.

### `DELETE /api/v3/downloadclient/{id}`

Delete a download client.

---

## Tag

### `GET /api/v3/tag`

List all tags.

### `POST /api/v3/tag`

Create a tag: `{ "label": "retro" }`

### `PUT /api/v3/tag/{id}`

Update a tag.

### `DELETE /api/v3/tag/{id}`

Delete a tag.

---

## System

### `GET /api/v3/system/status`

Get system status (version, OS, paths, etc.).

```json
{
  "version": "10.0.0.1",
  "appName": "Playarr",
  "startupPath": "/opt/playarr",
  "appData": "/config",
  "osName": "debian",
  "isLinux": true,
  "isDocker": true,
  "runtimeName": ".NET",
  "runtimeVersion": "10.0.0"
}
```

### `POST /api/v3/system/shutdown`

Shutdown the application.

### `POST /api/v3/system/restart`

Restart the application.

---

## Health

### `GET /api/v3/health`

Get health check results.

```json
[
  {
    "source": "IndexerStatusCheck",
    "type": "warning",
    "message": "Indexers unavailable due to failures: ExampleIndexer"
  }
]
```

---

## History

### `GET /api/v3/history`

Get download history. Supports paging:

| Parameter | Type | Description |
|-----------|------|-------------|
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Results per page (default: 10) |
| `sortKey` | string | Sort field |
| `sortDir` | string | `asc` or `desc` |
| `gameId` | int | Filter by game |

---

## Blocklist

### `GET /api/v3/blocklist`

List blocked releases (supports paging).

### `DELETE /api/v3/blocklist/{id}`

Remove an item from the blocklist.

---

## Pagination

Endpoints that support pagination return:

```json
{
  "page": 1,
  "pageSize": 10,
  "sortKey": "title",
  "sortDirection": "ascending",
  "totalRecords": 150,
  "records": [...]
}
```

## Rate Limiting

There are no built-in rate limits. If you're writing an integration, be considerate — avoid polling more than once per second.

## WebSocket / SignalR

Real-time updates are available via SignalR at:

```
http://localhost:9797/signalr/messages
```

Events include game updates, download progress, health status changes, and command completions.
