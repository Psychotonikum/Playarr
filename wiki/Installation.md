# Installation

## Methods

### Docker (Recommended)

The easiest way to run Playarr:

```bash
docker run -d \
  --name playarr \
  -p 9797:9797 \
  -v /path/to/config:/config \
  -v /path/to/roms:/roms \
  -e PUID=1000 \
  -e PGID=1000 \
  -e TZ=America/New_York \
  playarr/playarr:latest
```

#### Docker Compose

```yaml
version: "3"
services:
  playarr:
    image: playarr/playarr:latest
    container_name: playarr
    ports:
      - "9797:9797"
    volumes:
      - ./config:/config
      - /path/to/roms:/roms
    environment:
      - PUID=1000
      - PGID=1000
      - TZ=America/New_York
    restart: unless-stopped
```

### Debian/Ubuntu

```bash
git clone https://github.com/Psychotonikum/playarr.git
cd playarr/distribution/debian
sudo bash install.sh
```

This installs Playarr as a systemd service running on port 9797.

### From Source

Prerequisites: .NET 10 SDK, Node.js 20+, Yarn

```bash
git clone https://github.com/Psychotonikum/playarr.git
cd playarr
yarn install && yarn build
dotnet build src/Playarr.sln
dotnet run --project src/Playarr/Playarr.csproj
```

### Windows

1. Install [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Download the latest release
3. Extract to a folder (e.g., `C:\Playarr`)
4. Run `Playarr.exe`

### macOS

1. Install .NET 10 Runtime
2. Download the latest release
3. Extract and run

```bash
dotnet Playarr.dll
```

## Verifying Installation

Open `http://localhost:9797` in your browser. You should see the Playarr web UI.

## Next Steps

See the [Quick Start Guide](Quick-Start-Guide.md) for initial configuration.
