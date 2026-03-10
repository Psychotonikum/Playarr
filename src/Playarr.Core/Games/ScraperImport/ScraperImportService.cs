using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser;
using Playarr.Core.RootFolders;

namespace Playarr.Core.Games.ScraperImport
{
    public interface IScraperImportService
    {
        List<ScraperImportItem> Scan(string path);
        List<ScraperImportResult> Import(List<ScraperImportRequest> requests);
    }

    public class ScraperImportService : IScraperImportService
    {
        private static readonly object _idLock = new object();
        private static int _nextLocalId = -1;
        private readonly IDiskProvider _diskProvider;
        private readonly IGameService _gameService;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public ScraperImportService(
            IDiskProvider diskProvider,
            IGameService gameService,
            IRootFolderService rootFolderService,
            Logger logger)
        {
            _diskProvider = diskProvider;
            _gameService = gameService;
            _rootFolderService = rootFolderService;
            _logger = logger;
        }

        public List<ScraperImportItem> Scan(string path)
        {
            var items = new List<ScraperImportItem>();

            if (!_diskProvider.FolderExists(path))
            {
                _logger.Warn("Scraper import path does not exist: {0}", path);
                return items;
            }

            var systems = DefaultGameSystems.GetDefaults();
            var systemFolders = _diskProvider.GetDirectories(path);

            foreach (var systemFolder in systemFolders)
            {
                var folderName = new DirectoryInfo(systemFolder).Name;
                var system = systems.FirstOrDefault(s =>
                    s.FolderName.Equals(folderName, StringComparison.OrdinalIgnoreCase));

                if (system == null)
                {
                    _logger.Debug("Skipping unknown system folder: {0}", folderName);
                    continue;
                }

                if (system.SystemType == GameSystemType.Patchable)
                {
                    ScanPatchableSystem(systemFolder, system, items);
                }
                else
                {
                    ScanClassicSystem(systemFolder, system, items);
                }
            }

            return items;
        }

        private void ScanClassicSystem(string systemFolder, GameSystem system, List<ScraperImportItem> items)
        {
            var files = _diskProvider.GetFiles(systemFolder, false);

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);

                if (!MediaFileExtensions.Extensions.Contains(ext))
                {
                    continue;
                }

                var fileName = Path.GetFileNameWithoutExtension(file);
                var gameName = CleanRomName(fileName);

                var existing = items.FirstOrDefault(i =>
                    i.GameName.Equals(gameName, StringComparison.OrdinalIgnoreCase) &&
                    i.SystemName.Equals(system.Name, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.Files.Add(new ScraperImportFile
                    {
                        SourcePath = file,
                        FileName = Path.GetFileName(file),
                        Size = _diskProvider.GetFileSize(file),
                        FileType = "base"
                    });
                }
                else
                {
                    items.Add(new ScraperImportItem
                    {
                        GameName = gameName,
                        SystemName = system.Name,
                        SystemFolder = system.FolderName,
                        SystemType = system.SystemType,
                        Files = new List<ScraperImportFile>
                        {
                            new ScraperImportFile
                            {
                                SourcePath = file,
                                FileName = Path.GetFileName(file),
                                Size = _diskProvider.GetFileSize(file),
                                FileType = "base"
                            }
                        }
                    });
                }
            }
        }

        private void ScanPatchableSystem(string systemFolder, GameSystem system, List<ScraperImportItem> items)
        {
            var gameFolders = _diskProvider.GetDirectories(systemFolder);

            foreach (var gameFolder in gameFolders)
            {
                var gameName = CleanRomName(new DirectoryInfo(gameFolder).Name);
                var item = new ScraperImportItem
                {
                    GameName = gameName,
                    SystemName = system.Name,
                    SystemFolder = system.FolderName,
                    SystemType = system.SystemType,
                    Files = new List<ScraperImportFile>()
                };

                // Check for base/update/dlc subfolders
                var hasSubfolders = false;
                foreach (var subfolder in new[] { "base", "update", "dlc", "updates", "Base", "Update", "DLC", "Updates" })
                {
                    var subPath = Path.Combine(gameFolder, subfolder);
                    if (_diskProvider.FolderExists(subPath))
                    {
                        hasSubfolders = true;
                        var fileType = subfolder.ToLowerInvariant().TrimEnd('s');
                        if (fileType == "update")
                        {
                            fileType = "update";
                        }

                        AddFilesFromFolder(subPath, system, item, fileType);
                    }
                }

                // If no subfolders, treat files directly in game folder
                if (!hasSubfolders)
                {
                    AddFilesFromFolder(gameFolder, system, item, "base");
                }

                if (item.Files.Any())
                {
                    items.Add(item);
                }
            }

            // Also check for loose files directly in the system folder
            var looseFiles = _diskProvider.GetFiles(systemFolder, false);
            foreach (var file in looseFiles)
            {
                var ext = Path.GetExtension(file);
                if (!MediaFileExtensions.Extensions.Contains(ext))
                {
                    continue;
                }

                var fileName = Path.GetFileNameWithoutExtension(file);
                var gameName = CleanRomName(fileName);

                items.Add(new ScraperImportItem
                {
                    GameName = gameName,
                    SystemName = system.Name,
                    SystemFolder = system.FolderName,
                    SystemType = system.SystemType,
                    Files = new List<ScraperImportFile>
                    {
                        new ScraperImportFile
                        {
                            SourcePath = file,
                            FileName = Path.GetFileName(file),
                            Size = _diskProvider.GetFileSize(file),
                            FileType = "base"
                        }
                    }
                });
            }
        }

        private void AddFilesFromFolder(string folder, GameSystem system, ScraperImportItem item, string fileType)
        {
            // Recurse into subfolders (e.g. update/v851968/)
            var allFiles = _diskProvider.GetFiles(folder, true);

            foreach (var file in allFiles)
            {
                var ext = Path.GetExtension(file);
                if (!MediaFileExtensions.Extensions.Contains(ext))
                {
                    continue;
                }

                item.Files.Add(new ScraperImportFile
                {
                    SourcePath = file,
                    FileName = Path.GetFileName(file),
                    Size = _diskProvider.GetFileSize(file),
                    FileType = fileType
                });
            }
        }

        public List<ScraperImportResult> Import(List<ScraperImportRequest> requests)
        {
            var results = new List<ScraperImportResult>();
            var rootFolders = _rootFolderService.All();
            var rootFolder = rootFolders.FirstOrDefault();

            if (rootFolder == null)
            {
                _logger.Error("No root folder configured, cannot import");
                return results;
            }

            var systems = DefaultGameSystems.GetDefaults();

            foreach (var request in requests)
            {
                try
                {
                    var result = ImportSingle(request, rootFolder.Path, systems);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to import {0}", request.GameName);
                    results.Add(new ScraperImportResult
                    {
                        GameName = request.GameName,
                        Success = false,
                        Error = ex.Message
                    });
                }
            }

            return results;
        }

        private ScraperImportResult ImportSingle(ScraperImportRequest request, string rootPath, List<GameSystem> systems)
        {
            var system = systems.FirstOrDefault(s =>
                s.FolderName.Equals(request.SystemFolder, StringComparison.OrdinalIgnoreCase));

            if (system == null)
            {
                return new ScraperImportResult
                {
                    GameName = request.GameName,
                    Success = false,
                    Error = $"Unknown system: {request.SystemFolder}"
                };
            }

            // Check if game already exists
            Game existingGame = null;
            if (request.IgdbId > 0)
            {
                existingGame = _gameService.FindByIgdbId(request.IgdbId);
            }

            if (existingGame == null)
            {
                existingGame = _gameService.FindByTitle(request.GameName);
            }

            if (existingGame == null)
            {
                // Generate unique negative IgdbId for games without a real IGDB ID
                var igdbId = request.IgdbId;
                if (igdbId <= 0)
                {
                    lock (_idLock)
                    {
                        igdbId = _nextLocalId--;
                    }
                }

                // Create new game directly without SkyHook lookup
                var newGame = new Game
                {
                    IgdbId = igdbId,
                    Title = request.GameName,
                    CleanTitle = request.GameName.CleanGameTitle(),
                    SortTitle = GameTitleNormalizer.Normalize(request.GameName, igdbId),
                    TitleSlug = request.GameName.ToLower().Replace(" ", "-").Replace(":", "").Replace("'", "") + "-" + Math.Abs(igdbId),
                    QualityProfileId = request.QualityProfileId > 0 ? request.QualityProfileId : 1,
                    Path = Path.Combine(rootPath, request.GameName),
                    Monitored = true,
                    PlatformFolder = true,
                    Added = DateTime.UtcNow,
                    OriginalLanguage = Languages.Language.English,
                };

                try
                {
                    existingGame = _gameService.AddGame(newGame);
                    _logger.Info("Added game {0} (Id: {1})", existingGame.Title, existingGame.Id);

                    if (!_diskProvider.FolderExists(existingGame.Path))
                    {
                        _diskProvider.CreateFolder(existingGame.Path);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to add game {0}", request.GameName);
                }
            }

            if (existingGame == null)
            {
                return new ScraperImportResult
                {
                    GameName = request.GameName,
                    Success = false,
                    Error = "Failed to create or find game"
                };
            }

            // Move files to correct location
            var movedFiles = 0;
            foreach (var file in request.Files)
            {
                try
                {
                    var destPath = BuildDestinationPath(rootPath, system, request.GameName, file);
                    var destDir = Path.GetDirectoryName(destPath);

                    if (!_diskProvider.FolderExists(destDir))
                    {
                        _diskProvider.CreateFolder(destDir);
                    }

                    if (_diskProvider.FileExists(file.SourcePath))
                    {
                        if (_diskProvider.FileExists(destPath))
                        {
                            _logger.Debug("File already exists at destination: {0}", destPath);
                        }
                        else
                        {
                            _diskProvider.MoveFile(file.SourcePath, destPath);
                            _logger.Info("Moved {0} → {1}", file.SourcePath, destPath);
                        }

                        movedFiles++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to move file {0}", file.SourcePath);
                }
            }

            return new ScraperImportResult
            {
                GameName = request.GameName,
                GameId = existingGame.Id,
                Success = true,
                FilesImported = movedFiles,
            };
        }

        private string BuildDestinationPath(string rootPath, GameSystem system, string gameName, ScraperImportFile file)
        {
            if (system.SystemType == GameSystemType.Patchable)
            {
                var subFolder = file.FileType switch
                {
                    "update" => system.UpdateFolderName,
                    "dlc" => system.DlcFolderName,
                    _ => system.BaseFolderName
                };

                return Path.Combine(rootPath, system.FolderName, gameName, subFolder, file.FileName);
            }

            // Classic systems: system/filename
            return Path.Combine(rootPath, system.FolderName, file.FileName);
        }

        private static string CleanRomName(string name)
        {
            // Remove common ROM naming artifacts like [TITLEID], (Region), (v1.0), etc.
            var cleaned = name;

            // Remove square bracket content like [0100CD801CE5E000] [BASE][v0]
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s*\[[^\]]*\]", "");

            // Remove parenthetical region/version info for classic ROMs
            // Keep only the first parenthetical if it looks like a region
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s*\((?:v\d[^)]*|Rev \d[^)]*)\)", "");

            return cleaned.Trim();
        }
    }
}
