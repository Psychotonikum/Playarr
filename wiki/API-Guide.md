# API Guide

Playarr exposes a REST API for automation and third-party integration. The API follows the Servarr v3 standard.

## Authentication

All API requests require the `X-Api-Key` header:

```bash
curl -H "X-Api-Key: YOUR_API_KEY" http://localhost:9797/api/v3/system/status
```

Find your API key at **Settings > General > Security** in the web UI.

## Base URL

```
http://localhost:9797/api/v3
```

If you configured a URL base (e.g., `/playarr`), the API base becomes:
```
http://localhost:9797/playarr/api/v3
```

## Quick Examples

### List all games

```bash
curl -H "X-Api-Key: YOUR_KEY" http://localhost:9797/api/v3/game
```

### Add a game

```bash
curl -X POST \
  -H "X-Api-Key: YOUR_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "tvdbId": 12345,
    "title": "The Legend of Zelda",
    "qualityProfileId": 1,
    "rootFolderPath": "/roms",
    "monitored": true,
    "addOptions": {
      "searchForMissingRoms": true
    }
  }' \
  http://localhost:9797/api/v3/game
```

### Search for a game's ROMs

```bash
curl -X POST \
  -H "X-Api-Key: YOUR_KEY" \
  -H "Content-Type: application/json" \
  -d '{"name": "GameSearch", "gameId": 1}' \
  http://localhost:9797/api/v3/command
```

### Get system health

```bash
curl -H "X-Api-Key: YOUR_KEY" http://localhost:9797/api/v3/health
```

### Delete a game (with files)

```bash
curl -X DELETE \
  -H "X-Api-Key: YOUR_KEY" \
  "http://localhost:9797/api/v3/game/1?deleteFiles=true"
```

## Full API Reference

See [docs/api.md](../docs/api.md) for the complete endpoint documentation including request/response schemas for every resource.

## Real-Time Events (SignalR)

Connect to the SignalR hub for real-time updates:

```
ws://localhost:9797/signalr/messages
```

Events pushed include:
- Game added/updated/deleted
- ROM file imported
- Download started/completed/failed
- Health status changes
- Command progress

### JavaScript Example

```javascript
const signalR = require("@microsoft/signalr");

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:9797/signalr/messages", {
    headers: { "X-Api-Key": "YOUR_KEY" }
  })
  .build();

connection.on("receiveMessage", (message) => {
  console.log("Event:", message.name, message.body);
});

connection.start();
```

## Error Handling

Errors return appropriate HTTP status codes:

| Code | Meaning |
|------|---------|
| 400 | Bad request (invalid parameters) |
| 401 | Unauthorized (missing or invalid API key) |
| 404 | Resource not found |
| 409 | Conflict (duplicate resource) |
| 500 | Internal server error |

Error response body:

```json
[
  {
    "propertyName": "title",
    "errorMessage": "Title is required",
    "severity": "error"
  }
]
```

## Rate Limiting

No built-in rate limits. Be considerate — avoid polling more than once per second. Use SignalR for real-time updates instead of polling.
