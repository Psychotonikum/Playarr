# Configuration Reference

Playarr stores its configuration in `config.xml` inside the application data directory.

## Config File Locations

| OS | Path |
|----|------|
| Linux | `~/.config/Playarr/config.xml` |
| macOS | `~/.config/Playarr/config.xml` |
| Windows | `%AppData%\Playarr\config.xml` |
| Docker | `/config/config.xml` |

## Settings Reference

### Server

| Setting | Default | Description |
|---------|---------|-------------|
| `Port` | `9797` | HTTP port for the web UI and API |
| `SslPort` | `9898` | HTTPS port (when SSL is enabled) |
| `BindAddress` | `*` | IP address to bind to. Use `*` for all interfaces, `127.0.0.1` for localhost only |
| `EnableSsl` | `False` | Enable HTTPS |
| `SslCertPath` | | Path to SSL certificate (.pfx) |
| `SslCertPassword` | | Password for the SSL certificate |
| `UrlBase` | | URL base for reverse proxy setups (e.g., `/playarr`) |
| `LaunchBrowser` | `True` | Open browser on startup (desktop only) |

### Security

| Setting | Default | Description |
|---------|---------|-------------|
| `ApiKey` | (auto-generated) | API authentication key. Required for all API calls |
| `AuthenticationMethod` | `None` | Authentication type: `None`, `Basic`, `Forms`, `External` |
| `AuthenticationRequired` | `Enabled` | Whether authentication is required |
| `Branch` | `main` | Update branch |

### Logging

| Setting | Default | Description |
|---------|---------|-------------|
| `LogLevel` | `info` | Log verbosity: `trace`, `debug`, `info`, `warn`, `error`, `fatal` |
| `LogSql` | `False` | Log SQL queries (debug only) |
| `ConsoleLogLevel` | | Override log level for console output |
| `LogSizeLimit` | `1` | Maximum log file size in MB before rotation |

### Database

| Setting | Default | Description |
|---------|---------|-------------|
| Database engine | SQLite | Default embedded database |
| PostgreSQL | | Supported via connection string for larger deployments |

#### PostgreSQL Configuration

Set these environment variables:

```bash
Playarr__Postgres__Host=localhost
Playarr__Postgres__Port=5432
Playarr__Postgres__User=playarr
Playarr__Postgres__Password=secret
Playarr__Postgres__MainDb=playarr-main
Playarr__Postgres__LogDb=playarr-log
```

## Command-Line Arguments

```bash
dotnet Playarr.dll [options]
```

| Argument | Description |
|----------|-------------|
| `--port=PORT` | Override the configured port |
| `--data=/path` | Override the app data directory |
| `--nobrowser` | Don't open browser on startup |
| `--debug` | Enable debug logging |

## Environment Variables

| Variable | Description |
|----------|-------------|
| `Playarr__Server__Port` | Override port |
| `Playarr__Server__UrlBase` | Override URL base |
| `Playarr__Log__Level` | Override log level |
| `Playarr__Update__Branch` | Override update branch |

## Naming Configuration

Playarr supports configurable file naming via Settings > Media Management.

### Standard ROM Format

Tokens available:

| Token | Example | Description |
|-------|---------|-------------|
| `{Game Title}` | Super Mario Bros | Game name |
| `{Game CleanTitle}` | Super Mario Bros | Cleaned game name |
| `{Game TitleYear}` | Super Mario Bros (1985) | Game name with year |
| `{Platform}` | NES | Platform name |
| `{Platform:00}` | 01 | Zero-padded platform number |
| `{Rom Title}` | USA Rev A | ROM title |
| `{Rom:00}` | 01 | Zero-padded ROM number |
| `{Quality Title}` | Verified Good Dump | Quality name |
| `{Quality Full}` | [Verified Good Dump] | Quality with brackets |

### Example Naming Scheme

```
{Game Title}/Platform {Platform:00}/{Game Title} - S{Platform:00}E{Rom:00} - {Rom Title}
```

Produces:
```
Super Mario Bros/Platform 01/Super Mario Bros - S01E01 - USA Rev A.nes
```

## Quality Definitions

Quality profiles determine which ROM versions are preferred. Default qualities (ranked lowest to highest):

1. Unknown
2. Raw Dump
3. Verified Good Dump
4. Patched
5. Best Available

Configure custom quality profiles in **Settings > Profiles**.
