# Managing Games

## Adding a Game

### Search and Add

1. Click **+ Add Game** (or the plus icon in the navigation bar)
2. Type the game name in the search box
3. Select the correct match from the results
4. Configure options:
   - **Root Folder** — Directory where this game's ROMs will be stored
   - **Monitor** — Choose what to monitor:
     - All ROMs
     - Future ROMs only
     - Missing ROMs only
     - None
   - **Quality Profile** — Which quality standard to use
   - **Tags** — Optional organizational tags
5. Toggle **Start search for missing ROMs** to immediately search
6. Click **Add**

### Bulk Import

To import multiple games at once from an external list:

1. Go to **Settings > Import Lists**
2. Add a list source (e.g., another Playarr instance)
3. Playarr will periodically sync and add new games automatically

## Viewing Your Library

The main **Games** page shows your library in several views:

- **Posters** — Grid of game cover art
- **Overview** — Detailed list with synopsis
- **Table** — Compact sortable table

Use the toolbar to:
- **Filter** by monitored status, quality, tags, or missing ROMs
- **Sort** by title, date added, platform count, ROM count, etc.
- **Search** with the search box

## Game Details

Click any game to see its detail page:

- **Overview** — Title, description, statistics
- **Platforms** — List of platforms with ROM status
- **ROM List** — Individual ROMs with download status
- **Activity** — Download history for this game
- **Files** — Physical ROM files on disk

### Actions

- **Refresh** — Re-scan metadata from sources
- **Search** — Trigger a search for all missing/upgradable ROMs
- **Edit** — Change quality profile, path, monitoring, tags
- **Delete** — Remove the game (optionally delete files)

## Monitoring

Monitored ROMs are actively tracked — Playarr will search for and download them automatically.

To change monitoring:
- **Game level**: Edit the game and toggle monitoring
- **Platform level**: Click the monitor icon next to a platform
- **ROM level**: Click the monitor icon next to individual ROMs

## Mass Editor

For bulk changes across your library:

1. Go to **Games > Mass Editor**
2. Select multiple games using checkboxes
3. Apply changes:
   - Change root folder
   - Change quality profile
   - Toggle monitoring
   - Apply/remove tags
   - Delete selected games

## Tags

Tags help organize your library. Common uses:

- Platform grouping (e.g., "NES", "SNES", "Genesis")
- Quality tiers (e.g., "verified", "needs-upgrade")
- Collections (e.g., "favorites", "speedrun")

Create tags in **Settings > Tags** or inline when editing a game.
