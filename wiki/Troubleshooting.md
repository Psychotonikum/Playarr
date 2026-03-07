# Troubleshooting

## Common Issues

### Playarr Won't Start

**Port in use:**
```bash
# Check what's using port 9797
sudo lsof -i :9797
# Or
sudo ss -tlnp | grep 9797

# Kill the process or change Playarr's port in config.xml
```

**Missing .NET runtime:**
```bash
dotnet --info
# If not found, install .NET 10 Runtime
```

**Permission denied:**
```bash
# Check file permissions on the config directory
ls -la ~/.config/Playarr/

# Fix ownership
sudo chown -R $USER:$USER ~/.config/Playarr/
```

### Blank/White Web UI

1. Hard refresh: `Ctrl+Shift+R` (or `Cmd+Shift+R` on macOS)
2. Clear browser cache
3. Try a different browser
4. Check browser console (F12) for JavaScript errors
5. Rebuild frontend if running from source: `yarn build`

### Database Locked

Only one Playarr instance can access the database:

```bash
# Check for duplicate processes
ps aux | grep -i playarr

# Kill duplicates
pkill -f Playarr
```

If the issue persists after killing all processes, the lock file may be stale:
```bash
# Remove write-ahead log (safe)
rm ~/.config/Playarr/playarr.db-wal
rm ~/.config/Playarr/playarr.db-shm
```

### Downloads Not Importing

**Check permissions:**
```bash
# Playarr needs read access to download folder and write access to ROM folder
ls -la /path/to/downloads/
ls -la /path/to/roms/
```

**Docker path mismatch:**
Ensure the download client and Playarr share the same volume mount. If they see files at different paths, configure [Remote Path Mappings](Download-Clients.md#remote-path-mappings).

**Check the queue:**
Go to **Activity > Queue** to see if imports are stuck. Look for warning icons.

### Can't Connect to Download Client

1. Verify client is running and accessible
2. Check host/port/API key in **Settings > Download Clients**
3. Click **Test** to see the specific error
4. In Docker, use container name as host (not `localhost`)
5. Check firewall rules

### Search Returns No Results

1. Verify indexers are configured in **Settings > Indexers**
2. Test each indexer individually (click the wrench icon > Test)
3. Check **System > Status** for indexer warnings
4. Try an interactive search from the game's detail page to see raw results
5. Check indexer categories match ROM content

### Wrong Files Being Downloaded

1. Review your quality profile (**Settings > Profiles**)
2. Check custom format scores
3. Add release restrictions (**Settings > Indexers > Restrictions**)
4. Use the interactive search to see what Playarr finds and why certain releases are preferred

### High Memory Usage

- Check **System > Status** for the current memory footprint
- Large libraries with many monitored games use more memory
- Consider using PostgreSQL instead of SQLite for very large libraries
- Restart Playarr periodically if memory grows unbounded

### API Returns 401 Unauthorized

- Verify `X-Api-Key` header is present and correct
- Find your API key at **Settings > General > Security**
- If key was changed, update all clients/scripts

## Logs

### Log Location

| Setup | Path |
|-------|------|
| Linux | `~/.config/Playarr/logs/` |
| Docker | `/config/logs/` |
| Windows | `%AppData%\Playarr\logs\` |

### Increasing Log Verbosity

For debugging, increase the log level:

1. **Settings > General > Logging > Log Level** → `Debug` or `Trace`
2. Reproduce the issue
3. Check the log files
4. Set log level back to `Info` when done

### Log Files

| File | Contents |
|------|----------|
| `playarr.txt` | Current application log |
| `playarr.*.txt` | Rotated log files |
| `update.txt` | Self-update log |

## Getting Help

If you can't resolve an issue:

1. Check the [FAQ](FAQ.md)
2. Search existing [GitHub Issues](https://github.com/Psychotonikum/playarr/issues)
3. If it's a new bug, open an issue with:
   - Playarr version
   - OS and runtime version
   - Steps to reproduce
   - Relevant log excerpts (set to Debug first)
   - Screenshots if UI-related
