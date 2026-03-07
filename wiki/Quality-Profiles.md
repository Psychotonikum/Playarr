# Quality Profiles

Quality profiles control which ROM versions Playarr downloads and when it upgrades.

## Concepts

- **Quality** — A level of ROM dump quality (e.g., Raw Dump, Verified Good Dump)
- **Profile** — An ordered list of acceptable qualities
- **Cutoff** — The quality at which Playarr stops looking for upgrades

## Default Qualities

From lowest to highest:

| Quality | Description |
|---------|-------------|
| Unknown | Quality cannot be determined |
| Raw Dump | Unverified ROM dump |
| Verified Good Dump | Dump verified against known good checksums |
| Patched | Modified ROM (translations, bug fixes) |
| Best Available | Highest available quality |

## Creating a Profile

1. Go to **Settings > Profiles**
2. Click **+** to add a new profile
3. Name your profile (e.g., "High Quality Only")
4. Check the qualities you want to accept
5. Drag qualities to set preference order (top = most preferred)
6. Set the **Cutoff** — Playarr won't upgrade past this quality
7. Click **Save**

## Example Profiles

### Collector (Everything)
- Accept all qualities
- Cutoff: Best Available
- Always gets the best version

### Casual
- Accept: Verified Good Dump and above
- Cutoff: Verified Good Dump
- Gets a good version and stops

### Archival
- Accept: Verified Good Dump only
- Cutoff: Verified Good Dump
- Only verified dumps, nothing else

## Assigning Profiles

Profiles are assigned per-game:
- When adding a new game, select the quality profile
- In the mass editor, change profiles for multiple games at once
- Edit an individual game to change its profile

## Custom Formats

Custom formats let you fine-tune quality matching with specific criteria. They work alongside quality profiles to prefer or avoid certain release characteristics.

### Creating a Custom Format

1. Go to **Settings > Custom Formats**
2. Click **+** to add a new format
3. Name it (e.g., "Region: USA")
4. Add conditions:
   - **Release Title** — Regex match against the release name
   - **Size** — File size range
   - **Language** — Language tag
5. Set a **Score** (positive = prefer, negative = avoid)
6. Click **Save**

### Using Custom Formats with Profiles

1. Edit a quality profile
2. Scroll to **Custom Formats**
3. Set minimum and upgrade-until scores
4. Assign scores to each custom format
5. Playarr will factor scores into download decisions

### Example: Prefer USA Region

Custom Format: "USA Region"
- Condition: Release title matches `\(USA\)`
- Score: +100

Custom Format: "Japan Region"
- Condition: Release title matches `\(Japan\)`
- Score: +50

In the profile, set minimum score to 0 — this means both regions are acceptable, but USA releases are preferred.
