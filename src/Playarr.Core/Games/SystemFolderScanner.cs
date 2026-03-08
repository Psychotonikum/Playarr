using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;

namespace Playarr.Core.Games
{
    public interface ISystemFolderScanner
    {
        List<ScannedGameEntry> ScanSystemFolder(string rootPath, GameSystem system);
        List<ScannedGameEntry> ScanAllSystems(string rootPath, List<GameSystem> systems);
    }

    public class SystemFolderScanner : ISystemFolderScanner
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public SystemFolderScanner(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<ScannedGameEntry> ScanAllSystems(string rootPath, List<GameSystem> systems)
        {
            var results = new List<ScannedGameEntry>();

            if (!_diskProvider.FolderExists(rootPath))
            {
                _logger.Warn("Root ROM path does not exist: {0}", rootPath);
                return results;
            }

            foreach (var system in systems)
            {
                var systemPath = Path.Combine(rootPath, system.FolderName);
                if (_diskProvider.FolderExists(systemPath))
                {
                    results.AddRange(ScanSystemFolder(rootPath, system));
                }
            }

            return results;
        }

        public List<ScannedGameEntry> ScanSystemFolder(string rootPath, GameSystem system)
        {
            var systemPath = Path.Combine(rootPath, system.FolderName);

            if (!_diskProvider.FolderExists(systemPath))
            {
                _logger.Debug("System folder does not exist: {0}", systemPath);
                return new List<ScannedGameEntry>();
            }

            _logger.Info("Scanning system folder: {0} ({1})", system.Name, systemPath);

            if (system.SystemType == GameSystemType.Classic)
            {
                return ScanClassicSystem(systemPath, system);
            }

            return ScanPatchableSystem(systemPath, system);
        }

        private List<ScannedGameEntry> ScanClassicSystem(string systemPath, GameSystem system)
        {
            var entries = new Dictionary<string, ScannedGameEntry>(StringComparer.OrdinalIgnoreCase);
            var extensionSet = new HashSet<string>(system.FileExtensions, StringComparer.OrdinalIgnoreCase);

            var files = _diskProvider.GetFiles(systemPath, false);

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var ext = Path.GetExtension(filePath);

                if (extensionSet.Count > 0 && !extensionSet.Contains(ext))
                {
                    continue;
                }

                var parseResult = AerofoilFileNameParser.Parse(fileName);
                if (parseResult == null)
                {
                    _logger.Debug("Could not parse file name: {0}", fileName);
                    continue;
                }

                var title = parseResult.GameTitle;
                if (!entries.TryGetValue(title, out var entry))
                {
                    entry = new ScannedGameEntry
                    {
                        GameTitle = title,
                        GameSystemId = system.Id,
                        SystemFolderName = system.FolderName,
                        SystemType = GameSystemType.Classic
                    };
                    entries[title] = entry;
                }

                entry.BaseFile = new ScannedRomFile
                {
                    FullPath = filePath,
                    FileName = fileName,
                    FileType = RomFileType.Base,
                    Region = parseResult.Region,
                    Size = _diskProvider.GetFileSize(filePath)
                };
            }

            return entries.Values.ToList();
        }

        private List<ScannedGameEntry> ScanPatchableSystem(string systemPath, GameSystem system)
        {
            var entries = new Dictionary<string, ScannedGameEntry>(StringComparer.OrdinalIgnoreCase);
            var extensionSet = new HashSet<string>(system.FileExtensions, StringComparer.OrdinalIgnoreCase);

            var baseFolderName = system.BaseFolderName ?? "base";
            var updateFolderName = system.UpdateFolderName ?? "update";
            var dlcFolderName = system.DlcFolderName ?? "dlc";

            var basePath = Path.Combine(systemPath, baseFolderName);
            var updatePath = Path.Combine(systemPath, updateFolderName);
            var dlcPath = Path.Combine(systemPath, dlcFolderName);

            // Scan base files
            if (_diskProvider.FolderExists(basePath))
            {
                ScanSubFolder(basePath, system, extensionSet, entries, RomFileType.Base);
            }

            // Scan update files
            if (_diskProvider.FolderExists(updatePath))
            {
                ScanSubFolder(updatePath, system, extensionSet, entries, RomFileType.Update);
            }

            // Scan DLC files
            if (_diskProvider.FolderExists(dlcPath))
            {
                ScanSubFolder(dlcPath, system, extensionSet, entries, RomFileType.Dlc);
            }

            return entries.Values.ToList();
        }

        private void ScanSubFolder(
            string folderPath,
            GameSystem system,
            HashSet<string> extensionSet,
            Dictionary<string, ScannedGameEntry> entries,
            RomFileType expectedType)
        {
            var files = _diskProvider.GetFiles(folderPath, false);

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var ext = Path.GetExtension(filePath);

                if (extensionSet.Count > 0 && !extensionSet.Contains(ext))
                {
                    continue;
                }

                var parseResult = AerofoilFileNameParser.Parse(fileName);
                if (parseResult == null)
                {
                    _logger.Debug("Could not parse file name: {0}", fileName);
                    continue;
                }

                var title = parseResult.GameTitle;
                if (!entries.TryGetValue(title, out var entry))
                {
                    entry = new ScannedGameEntry
                    {
                        GameTitle = title,
                        GameSystemId = system.Id,
                        SystemFolderName = system.FolderName,
                        SystemType = system.SystemType
                    };
                    entries[title] = entry;
                }

                var scannedFile = new ScannedRomFile
                {
                    FullPath = filePath,
                    FileName = fileName,
                    FileType = parseResult.FileType,
                    Version = parseResult.Version,
                    DlcIndex = parseResult.DlcIndex,
                    Region = parseResult.Region,
                    Size = _diskProvider.GetFileSize(filePath)
                };

                switch (parseResult.FileType)
                {
                    case RomFileType.Base:
                        entry.BaseFile = scannedFile;
                        break;
                    case RomFileType.Update:
                        entry.Updates.Add(scannedFile);
                        break;
                    case RomFileType.Dlc:
                        entry.Dlcs.Add(scannedFile);
                        break;
                }
            }
        }
    }
}
