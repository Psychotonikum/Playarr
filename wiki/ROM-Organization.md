# ROM Organization

## How Playarr Organizes Files

Playarr stores ROM files in a structured folder hierarchy:

```
{Root Folder}/
└── {Game Title}/
    └── Platform {Number}/
        └── {Game Title} - S{Platform}E{ROM} - {ROM Title}.ext
```

Example:
```
/roms/
├── Super Mario Bros/
│   └── Platform 01/
│       ├── Super Mario Bros - S01E01 - USA.nes
│       └── Super Mario Bros - S01E02 - Japan.nes
├── The Legend of Zelda/
│   ├── Platform 01/
│   │   └── The Legend of Zelda - S01E01 - USA.nes
│   └── Platform 02/
│       └── The Legend of Zelda - S02E01 - USA.snes
```

## Naming Conventions

### Configuring Naming

Go to **Settings > Media Management** to customize naming.

### Available Tokens

#### Game Tokens
| Token | Example |
|-------|---------|
| `{Game Title}` | Super Mario Bros |
| `{Game CleanTitle}` | Super Mario Bros |
| `{Game TitleYear}` | Super Mario Bros (1985) |
| `{Game TitleThe}` | Super Mario Bros, The |

#### Platform Tokens
| Token | Example |
|-------|---------|
| `{Platform}` | 1 |
| `{Platform:00}` | 01 |

#### ROM Tokens
| Token | Example |
|-------|---------|
| `{Rom Title}` | USA Rev A |
| `{Rom CleanTitle}` | USA Rev A |
| `{Rom:00}` | 01 |

#### Quality Tokens
| Token | Example |
|-------|---------|
| `{Quality Title}` | Verified Good Dump |
| `{Quality Full}` | [Verified Good Dump] |

### Example Schemes

**Standard:**
```
{Game Title} - S{Platform:00}E{Rom:00} - {Rom Title}
→ Super Mario Bros - S01E01 - USA Rev A.nes
```

**Simple:**
```
{Game Title} ({Rom Title})
→ Super Mario Bros (USA Rev A).nes
```

**Detailed:**
```
{Game Title} - S{Platform:00}E{Rom:00} - {Rom Title} {Quality Full}
→ Super Mario Bros - S01E01 - USA Rev A [Verified Good Dump].nes
```

## File Renaming

### Automatic Renaming

When Playarr downloads a new ROM, it automatically renames it according to your naming scheme.

### Manual Rename

To rename existing files:

1. Go to the game's detail page
2. Click **Files**
3. Select files to rename
4. Click **Rename** and confirm

### Bulk Rename

1. Go to **Games > Mass Editor**
2. Select games
3. Use **Rename Files** from the actions menu

## Root Folders

Root folders define base directories for your ROM storage.

### Adding Root Folders

**Settings > Media Management > Root Folders**

You can have multiple root folders for different storage locations:
- `/roms/active` — Current favorites
- `/roms/archive` — Archived collections
- `/external/roms` — External drive

### Folder Permissions

Playarr needs read and write access to root folders. On Linux:

```bash
# Ensure the Playarr user owns the ROM directory
sudo chown -R playarr:playarr /roms

# Or add the Playarr user to the appropriate group
sudo usermod -aG media playarr
```

## Importing Existing Files

### Library Import

1. Go to **Games > Library Import**
2. Select the root folder containing existing ROMs
3. Playarr scans the folder structure and matches files to games
4. Review the suggestions and adjust any incorrect matches
5. Click **Import Selected**

### Manual Import

For individual files or folders:

1. Go to **Activity > Import** (or use the manual import feature)
2. Browse to the file or folder
3. Select the target game, platform, and ROM
4. Click **Import**

## Recycle Bin

When Playarr replaces a ROM with an upgrade, the old file is moved to the recycle bin (if configured).

Configure in **Settings > Media Management > File Management**:
- **Recycling Bin**: Path to the recycle bin folder
- **Recycling Bin Cleanup**: Days before files are permanently deleted (0 = never auto-delete)
