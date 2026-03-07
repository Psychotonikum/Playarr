# Quick Start Guide

After installing Playarr and opening `http://localhost:9797`, follow these steps to get started.

## Step 1: Set Up a Root Folder

A root folder is where Playarr will store your ROM files.

1. Go to **Settings > Media Management**
2. Click **Add Root Folder**
3. Select or type the path to your ROM directory (e.g., `/roms` or `C:\ROMs`)
4. Click **Save**

## Step 2: Configure a Download Client

Playarr needs a download client to fetch ROM files.

1. Go to **Settings > Download Clients**
2. Click the **+** button
3. Select your client (SABnzbd, NZBGet, qBittorrent, Transmission, etc.)
4. Enter the connection details:
   - **Host**: Usually `localhost` or your client's IP
   - **Port**: The client's port
   - **API Key / Password**: Authentication credentials
5. Click **Test** to verify the connection
6. Click **Save**

## Step 3: Add an Indexer

Indexers are where Playarr searches for ROM files.

1. Go to **Settings > Indexers**
2. Click the **+** button
3. Select the indexer type (Newznab, Torznab, etc.)
4. Enter the indexer URL and API key
5. Click **Test**, then **Save**

## Step 4: Create a Quality Profile

Quality profiles determine which ROM versions Playarr prefers.

1. Go to **Settings > Profiles**
2. Edit the default profile or create a new one
3. Arrange qualities in order of preference (drag to reorder)
4. Set a cutoff — Playarr will stop upgrading once this quality is reached
5. Click **Save**

## Step 5: Add Your First Game

1. Click **Games > Add New** (or the **+** button in the navbar)
2. Search for a game by name
3. Select the correct result
4. Configure:
   - **Root Folder**: Where to store this game's ROMs
   - **Quality Profile**: Which quality to prefer
   - **Monitored**: Whether to automatically search for ROMs
5. Click **Add Game**

Playarr will automatically search for and download available ROMs based on your configuration.

## Step 6: Import Existing ROMs

If you already have ROMs on disk:

1. Go to **Games > Library Import** (or manual import)
2. Select the folder containing your existing ROMs
3. Playarr will scan and match files to games
4. Review and confirm the matches
5. Click **Import**

## What's Next?

- **Calendar**: View upcoming game releases at **Calendar**
- **Activity**: Monitor downloads at **Activity > Queue**
- **History**: View past activity at **Activity > History**
- **Settings**: Fine-tune behavior in **Settings**
- **System**: Check health and logs at **System > Status**
