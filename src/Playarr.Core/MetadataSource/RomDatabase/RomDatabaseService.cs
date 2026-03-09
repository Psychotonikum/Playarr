using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.EnvironmentInfo;

namespace Playarr.Core.MetadataSource.RomDatabase
{
    public class RomDatabaseService : IRomDatabaseService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;
        private readonly string _databasePath;

        private static readonly List<RomDatabaseSystem> AvailableSystems = new List<RomDatabaseSystem>
        {
            // No-Intro DAT sources (cartridge-based / simple ROM systems)
            new RomDatabaseSystem { Id = "nointro-nes", Name = "Nintendo Entertainment System", Source = "No-Intro", EstimatedSizeBytes = 2_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "nointro-snes", Name = "Super Nintendo", Source = "No-Intro", EstimatedSizeBytes = 3_500_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "nointro-n64", Name = "Nintendo 64", Source = "No-Intro", EstimatedSizeBytes = 1_500_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "nointro-gb", Name = "Game Boy", Source = "No-Intro", EstimatedSizeBytes = 800_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "nointro-gbc", Name = "Game Boy Color", Source = "No-Intro", EstimatedSizeBytes = 1_200_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "nointro-gba", Name = "Game Boy Advance", Source = "No-Intro", EstimatedSizeBytes = 2_500_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "nointro-nds", Name = "Nintendo DS", Source = "No-Intro", EstimatedSizeBytes = 4_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "nointro-3ds", Name = "Nintendo 3DS", Source = "No-Intro", EstimatedSizeBytes = 3_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "nointro-switch", Name = "Nintendo Switch", Source = "No-Intro", EstimatedSizeBytes = 5_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "nointro-megadrive", Name = "Sega Genesis / Mega Drive", Source = "No-Intro", EstimatedSizeBytes = 2_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "nointro-mastersystem", Name = "Sega Master System", Source = "No-Intro", EstimatedSizeBytes = 800_000, EntryCount = 0 },

            // Redump DAT sources (disc-based systems)
            new RomDatabaseSystem { Id = "redump-psx", Name = "Sony PlayStation", Source = "Redump", EstimatedSizeBytes = 5_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "redump-ps2", Name = "Sony PlayStation 2", Source = "Redump", EstimatedSizeBytes = 8_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "redump-ps3", Name = "Sony PlayStation 3", Source = "Redump", EstimatedSizeBytes = 4_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "redump-psp", Name = "Sony PlayStation Portable", Source = "Redump", EstimatedSizeBytes = 3_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "redump-saturn", Name = "Sega Saturn", Source = "Redump", EstimatedSizeBytes = 2_500_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "redump-dreamcast", Name = "Sega Dreamcast", Source = "Redump", EstimatedSizeBytes = 3_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "redump-gc", Name = "Nintendo GameCube", Source = "Redump", EstimatedSizeBytes = 3_500_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "redump-wii", Name = "Nintendo Wii", Source = "Redump", EstimatedSizeBytes = 5_000_000, EntryCount = 0 },
            new RomDatabaseSystem { Id = "redump-wiiu", Name = "Nintendo Wii U", Source = "Redump", EstimatedSizeBytes = 2_000_000, EntryCount = 0 },
        };

        public RomDatabaseService(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
            _databasePath = Path.Combine(appFolderInfo.AppDataFolder, "rom_databases");

            if (!_diskProvider.FolderExists(_databasePath))
            {
                _diskProvider.CreateFolder(_databasePath);
            }
        }

        public List<RomDatabaseSystem> GetAvailableSystems()
        {
            var systems = AvailableSystems.Select(s => new RomDatabaseSystem
            {
                Id = s.Id,
                Name = s.Name,
                Source = s.Source,
                EstimatedSizeBytes = s.EstimatedSizeBytes,
                IsDownloaded = IsDatabaseDownloaded(s.Id),
                EntryCount = GetEntryCount(s.Id)
            }).ToList();

            return systems;
        }

        public RomDatabaseStatus GetSystemStatus(string systemId)
        {
            var datFile = GetDatFilePath(systemId);
            var isDownloaded = File.Exists(datFile);

            return new RomDatabaseStatus
            {
                SystemId = systemId,
                IsDownloaded = isDownloaded,
                EntryCount = isDownloaded ? GetEntryCount(systemId) : 0,
                LastUpdated = isDownloaded ? File.GetLastWriteTimeUtc(datFile).ToString("yyyy-MM-dd") : null
            };
        }

        public void DownloadDatabase(string systemId)
        {
            // DAT files must be manually placed by the user from:
            // No-Intro: https://datomatic.no-intro.org/
            // Redump: http://redump.org/
            // Playarr will parse XML DAT files in the rom_databases folder
            _logger.Info("ROM database '{0}' should be placed in: {1}", systemId, _databasePath);
            _logger.Info("No-Intro DATs: https://datomatic.no-intro.org/");
            _logger.Info("Redump DATs: http://redump.org/");
        }

        public RomDatabaseEntry LookupByHash(string systemId, string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                return null;
            }

            var entries = LoadDatabase(systemId);
            return entries.FirstOrDefault(e =>
                string.Equals(e.Hash, hash, StringComparison.OrdinalIgnoreCase));
        }

        public RomDatabaseEntry LookupByName(string systemId, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return null;
            }

            var entries = LoadDatabase(systemId);
            var cleanName = Path.GetFileNameWithoutExtension(filename);

            return entries.FirstOrDefault(e =>
                e.Name != null && e.Name.IndexOf(cleanName, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool IsDatabaseDownloaded(string systemId)
        {
            return File.Exists(GetDatFilePath(systemId));
        }

        private string GetDatFilePath(string systemId)
        {
            return Path.Combine(_databasePath, $"{systemId}.dat");
        }

        private int GetEntryCount(string systemId)
        {
            try
            {
                var entries = LoadDatabase(systemId);
                return entries.Count;
            }
            catch
            {
                return 0;
            }
        }

        private List<RomDatabaseEntry> LoadDatabase(string systemId)
        {
            var datFile = GetDatFilePath(systemId);

            if (!File.Exists(datFile))
            {
                return new List<RomDatabaseEntry>();
            }

            try
            {
                var doc = XDocument.Load(datFile);
                var entries = new List<RomDatabaseEntry>();
                var source = systemId.StartsWith("nointro") ? "No-Intro" : "Redump";

                foreach (var gameElement in doc.Descendants("game"))
                {
                    var name = gameElement.Attribute("name")?.Value;
                    var rom = gameElement.Element("rom");
                    var hash = rom?.Attribute("sha1")?.Value ?? rom?.Attribute("md5")?.Value ?? rom?.Attribute("crc")?.Value;
                    var region = ExtractRegion(name);

                    entries.Add(new RomDatabaseEntry
                    {
                        Name = name,
                        System = systemId,
                        Region = region,
                        Hash = hash,
                        Source = source,
                        TrackCount = gameElement.Elements("rom").Count()
                    });
                }

                return entries;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to parse DAT file for system {0}", systemId);
                return new List<RomDatabaseEntry>();
            }
        }

        private static string ExtractRegion(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Unknown";
            }

            var regionStart = name.IndexOf('(');
            var regionEnd = name.IndexOf(')');

            if (regionStart >= 0 && regionEnd > regionStart)
            {
                return name.Substring(regionStart + 1, regionEnd - regionStart - 1);
            }

            return "Unknown";
        }
    }
}
