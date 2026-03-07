# Indexers

Indexers are the sources Playarr searches for ROM files. They connect Playarr to Usenet indexers and torrent trackers.

## Supported Protocols

### Newznab (Usenet)

Newznab is the standard API for Usenet indexers. Most Usenet indexers support it.

### Torznab (Torrent)

Torznab is the torrent equivalent of Newznab, commonly provided by Jackett or Prowlarr.

### RSS Feeds

Playarr can monitor RSS feeds for new releases.

## Adding an Indexer

1. Go to **Settings > Indexers**
2. Click the **+** button
3. Select the indexer type
4. Configure:

| Setting | Description |
|---------|-------------|
| **Name** | Friendly name |
| **Enable RSS** | Monitor this indexer's RSS feed |
| **Enable Automatic Search** | Include in automatic searches |
| **Enable Interactive Search** | Include in manual/interactive searches |
| **URL** | Indexer API URL |
| **API Path** | API endpoint path (usually `/api`) |
| **API Key** | Your indexer API key |
| **Categories** | Categories to search (indexer-specific) |

5. Click **Test** to verify
6. Click **Save**

## Using Prowlarr

[Prowlarr](https://prowlarr.com/) is the recommended indexer manager for the Servarr ecosystem. It manages all your indexers in one place and syncs them to Playarr automatically.

### Setup

1. Install Prowlarr
2. Add your indexers to Prowlarr
3. In Prowlarr, add Playarr as an application:
   - Playarr URL: `http://localhost:9797`
   - API Key: Your Playarr API key
4. Prowlarr will automatically sync indexers to Playarr

## Using Jackett

[Jackett](https://github.com/Jackett/Jackett) translates torrent tracker queries to the Torznab API.

### Setup

1. Install Jackett
2. Add your trackers to Jackett
3. In Playarr, add a Torznab indexer:
   - URL: `http://localhost:9117/api/v2.0/indexers/{tracker}/results/torznab/`
   - API Key: Your Jackett API key

## Search Behavior

### RSS Sync

Playarr periodically checks RSS feeds for new releases. Configure the interval in **Settings > Indexers**:
- **RSS Sync Interval**: Minutes between RSS checks (default: 15, minimum: 10)

### Automatic Search

When a new game is added with monitoring enabled, Playarr searches all enabled indexers automatically.

### Interactive Search

Manual search from a game's detail page:
1. Go to the game
2. Click the search icon next to a ROM or platform
3. Review available releases
4. Click the download icon to grab a specific release

## Restrictions

Restrict downloads based on release attributes:

1. Go to **Settings > Indexers > Restrictions**
2. Add restrictions:
   - **Must Contain**: Release must include these terms
   - **Must Not Contain**: Release must NOT include these terms
   - **Tags**: Apply restriction only to games with these tags

### Example Restrictions

**Prefer verified dumps:**
- Must Contain: `[!]` or `(Verified)`

**Avoid bad dumps:**
- Must Not Contain: `[b]`, `(Bad Dump)`, `(Overdump)`

**Avoid hacks:**
- Must Not Contain: `(Hack)`, `[h]`
