using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.CustomFormats;
using Playarr.Core.Download;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles.EpisodeImport.Aggregation;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.EpisodeImport.Manual
{
    public interface IManualImportService
    {
        List<ManualImportItem> GetMediaFiles(int gameId, int? platformNumber);
        List<ManualImportItem> GetMediaFiles(string path, string downloadId, int? gameId, bool filterExistingFiles);
        ManualImportItem ReprocessItem(string path, string downloadId, int gameId, int? platformNumber, List<int> romIds, string releaseGroup, QualityModel quality, List<Language> languages, int indexerFlags, ReleaseType releaseType);
    }

    public class ManualImportService : IExecute<ManualImportCommand>, IManualImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IParsingService _parsingService;
        private readonly IDiskScanService _diskScanService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IGameService _gameService;
        private readonly IRomService _romService;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly IAggregationService _aggregationService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDownloadedEpisodesImportService _downloadedEpisodesImportService;
        private readonly IMediaFileService _mediaFileService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly ILocalEpisodeCustomFormatCalculationService _localRomFormatCalculator;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ManualImportService(IDiskProvider diskProvider,
                                   IParsingService parsingService,
                                   IDiskScanService diskScanService,
                                   IMakeImportDecision importDecisionMaker,
                                   IGameService seriesService,
                                   IRomService episodeService,
                                   IAggregationService aggregationService,
                                   IImportApprovedEpisodes importApprovedEpisodes,
                                   ITrackedDownloadService trackedDownloadService,
                                   IDownloadedEpisodesImportService downloadedEpisodesImportService,
                                   IMediaFileService mediaFileService,
                                   ILocalEpisodeCustomFormatCalculationService localRomFormatCalculator,
                                   ICustomFormatCalculationService formatCalculator,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _diskProvider = diskProvider;
            _parsingService = parsingService;
            _diskScanService = diskScanService;
            _importDecisionMaker = importDecisionMaker;
            _gameService = seriesService;
            _romService = episodeService;
            _aggregationService = aggregationService;
            _importApprovedEpisodes = importApprovedEpisodes;
            _trackedDownloadService = trackedDownloadService;
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _mediaFileService = mediaFileService;
            _localRomFormatCalculator = localRomFormatCalculator;
            _formatCalculator = formatCalculator;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ManualImportItem> GetMediaFiles(int gameId, int? platformNumber)
        {
            var game = _gameService.GetGame(gameId);
            var directoryInfo = new DirectoryInfo(game.Path);
            var seriesFiles = platformNumber.HasValue ? _mediaFileService.GetFilesBySeason(gameId, platformNumber.Value) : _mediaFileService.GetFilesBySeries(gameId);
            var roms = _romService.GetEpisodeBySeries(game.Id);

            var items = seriesFiles.Select(romFile => MapItem(romFile, game, directoryInfo.Name, roms)).ToList();

            if (!platformNumber.HasValue)
            {
                var mediaFiles = _diskScanService.FilterPaths(game.Path, _diskScanService.GetVideoFiles(game.Path)).ToList();
                var unmappedFiles = MediaFileService.FilterExistingFiles(mediaFiles, seriesFiles, game);

                items.AddRange(unmappedFiles.Select(file =>
                    new ManualImportItem
                    {
                        Path = Path.Combine(game.Path, file),
                        FolderName = directoryInfo.Name,
                        RelativePath = game.Path.GetRelativePath(file),
                        Name = Path.GetFileNameWithoutExtension(file),
                        Game = game,
                        PlatformNumber = null,
                        Roms = new List<Rom>(),
                        ReleaseGroup = string.Empty,
                        Quality = new QualityModel(Quality.Unknown),
                        Languages = new List<Language> { Language.Unknown },
                        Size = _diskProvider.GetFileSize(file),
                        Rejections = Enumerable.Empty<ImportRejection>()
                    }));
            }

            return items;
        }

        public List<ManualImportItem> GetMediaFiles(string path, string downloadId, int? gameId, bool filterExistingFiles)
        {
            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);

                if (trackedDownload == null)
                {
                    return new List<ManualImportItem>();
                }

                path = trackedDownload.ImportItem.OutputPath.FullPath;
            }

            if (!_diskProvider.FolderExists(path))
            {
                if (!_diskProvider.FileExists(path))
                {
                    return new List<ManualImportItem>();
                }

                var rootFolder = Path.GetDirectoryName(path);
                return new List<ManualImportItem> { ProcessFile(rootFolder, rootFolder, path, downloadId) };
            }

            return ProcessFolder(path, path, downloadId, gameId, filterExistingFiles);
        }

        public ManualImportItem ReprocessItem(string path, string downloadId, int gameId, int? platformNumber, List<int> romIds, string releaseGroup, QualityModel quality, List<Language> languages, int indexerFlags, ReleaseType releaseType)
        {
            var rootFolder = Path.GetDirectoryName(path);
            var game = _gameService.GetGame(gameId);

            var languageParse = LanguageParser.ParseLanguages(path);

            if (languageParse.Count <= 1 && languageParse.First() == Language.Unknown && game != null)
            {
                languageParse = new List<Language> { game.OriginalLanguage };
                _logger.Debug("Language couldn't be parsed from release, falling back to game original language: {0}", game.OriginalLanguage.Name);
            }

            if (romIds.Any())
            {
                var downloadClientItem = GetTrackedDownload(downloadId)?.DownloadItem;
                var roms = _romService.GetRoms(romIds);
                var finalReleaseGroup = releaseGroup.IsNullOrWhiteSpace()
                    ? Parser.ReleaseGroupParser.ParseReleaseGroup(path)
                    : releaseGroup;
                var finalQuality = quality.Quality == Quality.Unknown ? QualityParser.ParseQuality(path) : quality;
                var finalLanguages =
                    languages?.Count <= 1 && (languages?.SingleOrDefault() ?? Language.Unknown) == Language.Unknown
                        ? languageParse
                        : languages;

                var localRom = new LocalEpisode();
                localRom.Game = game;
                localRom.Roms = roms;
                localRom.FileRomInfo = Parser.Parser.ParsePath(path);
                localRom.DownloadClientRomInfo = downloadClientItem == null ? null : Parser.Parser.ParseTitle(downloadClientItem.Title);
                localRom.DownloadItem = downloadClientItem;
                localRom.Path = path;
                localRom.SceneSource = SceneSource(game, rootFolder);
                localRom.ExistingFile = game.Path.IsParentPath(path);
                localRom.Size = _diskProvider.GetFileSize(path);
                localRom.ReleaseGroup = finalReleaseGroup;
                localRom.Languages = finalLanguages;
                localRom.Quality = finalQuality;
                localRom.IndexerFlags = (IndexerFlags)indexerFlags;
                localRom.ReleaseType = releaseType;

                _localRomFormatCalculator.UpdateEpisodeCustomFormats(localRom);

                // Augment rom file so imported files have all additional information an automatic import would
                localRom = _aggregationService.Augment(localRom, downloadClientItem);

                // Reapply the user-chosen values.
                localRom.Game = game;
                localRom.Roms = roms;
                localRom.ReleaseGroup = finalReleaseGroup;
                localRom.Quality = finalQuality;
                localRom.Languages = finalLanguages;
                localRom.IndexerFlags = (IndexerFlags)indexerFlags;
                localRom.ReleaseType = releaseType;

                return MapItem(_importDecisionMaker.GetDecision(localRom, downloadClientItem), rootFolder, downloadId, null);
            }

            // This case will happen if the user selected a platform, but didn't select the roms in the platform then changed the language or quality.
            // Instead of overriding their platform selection let it persist and reject it with an appropriate error.

            if (platformNumber.HasValue)
            {
                var downloadClientItem = GetTrackedDownload(downloadId)?.DownloadItem;

                var localRom = new LocalEpisode
                {
                    Game = game,
                    Roms = new List<Rom>(),
                    FileRomInfo = Parser.Parser.ParsePath(path),
                    DownloadClientRomInfo = downloadClientItem == null
                        ? null
                        : Parser.Parser.ParseTitle(downloadClientItem.Title),
                    DownloadItem = downloadClientItem,
                    Path = path,
                    SceneSource = SceneSource(game, rootFolder),
                    ExistingFile = game.Path.IsParentPath(path),
                    Size = _diskProvider.GetFileSize(path),
                    ReleaseGroup = releaseGroup.IsNullOrWhiteSpace() ? Parser.ReleaseGroupParser.ParseReleaseGroup(path) : releaseGroup,
                    Languages = languages?.Count <= 1 && (languages?.SingleOrDefault() ?? Language.Unknown) == Language.Unknown ? LanguageParser.ParseLanguages(path) : languages,
                    Quality = quality.Quality == Quality.Unknown ? QualityParser.ParseQuality(path) : quality,
                    IndexerFlags = (IndexerFlags)indexerFlags,
                    ReleaseType = releaseType
                };

                return MapItem(new ImportDecision(localRom, new ImportRejection(ImportRejectionReason.NoEpisodes, "Roms not selected")), rootFolder, downloadId, null);
            }

            return ProcessFile(rootFolder, rootFolder, path, downloadId, game);
        }

        private List<ManualImportItem> ProcessFolder(string rootFolder, string baseFolder, string downloadId, int? gameId, bool filterExistingFiles)
        {
            DownloadClientItem downloadClientItem = null;
            Game game = null;

            var directoryInfo = new DirectoryInfo(baseFolder);

            if (gameId.HasValue)
            {
                game = _gameService.GetGame(gameId.Value);
            }
            else
            {
                try
                {
                    game = _parsingService.GetGame(directoryInfo.Name);
                }
                catch (MultipleSeriesFoundException e)
                {
                    _logger.Warn(e, "Unable to find game from title");
                }
            }

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                downloadClientItem = trackedDownload.DownloadItem;

                if (game == null)
                {
                    game = trackedDownload.RemoteRom?.Game;
                }
            }

            if (game == null)
            {
                // Filter paths based on the rootFolder, so files in subfolders that should be ignored are ignored.
                // It will lead to some extra directories being checked for files, but it saves the processing of them and is cleaner than
                // teaching FilterPaths to know whether it's processing a file or a folder and changing it's filtering based on that.

                // If the game is unknown for the directory and there are more than 100 files in the folder don't process the items before returning.
                var files = _diskScanService.FilterPaths(rootFolder, _diskScanService.GetVideoFiles(baseFolder, false));

                if (files.Count > 100)
                {
                    _logger.Warn("Unable to determine game from folder name and found more than 100 files. Skipping parsing");
                    return ProcessDownloadDirectory(rootFolder, files);
                }

                var subfolders = _diskScanService.FilterPaths(rootFolder, _diskProvider.GetDirectories(baseFolder));

                var processedFiles = files.Select(file => ProcessFile(rootFolder, baseFolder, file, downloadId));
                var processedFolders = subfolders.SelectMany(subfolder => ProcessFolder(rootFolder, subfolder, downloadId, null, filterExistingFiles));

                return processedFiles.Concat(processedFolders).Where(i => i != null).ToList();
            }

            var downloadClientItemInfo = downloadClientItem == null ? null : Parser.Parser.ParseTitle(downloadClientItem.Title);
            var folderInfo = Parser.Parser.ParseTitle(directoryInfo.Name);
            var seriesFiles = _diskScanService.FilterPaths(rootFolder, _diskScanService.GetVideoFiles(baseFolder).ToList());
            var decisions = _importDecisionMaker.GetImportDecisions(seriesFiles, game, downloadClientItem, downloadClientItemInfo, folderInfo, SceneSource(game, baseFolder), filterExistingFiles);

            return decisions.Select(decision => MapItem(decision, rootFolder, downloadId, directoryInfo.Name)).ToList();
        }

        private ManualImportItem ProcessFile(string rootFolder, string baseFolder, string file, string downloadId, Game game = null)
        {
            try
            {
                var trackedDownload = GetTrackedDownload(downloadId);
                var relativeFile = baseFolder.GetRelativePath(file);

                if (game == null)
                {
                    _parsingService.GetGame(relativeFile.Split('\\', '/')[0]);
                }

                if (game == null)
                {
                    game = _parsingService.GetGame(relativeFile);
                }

                if (trackedDownload != null && game == null)
                {
                    game = trackedDownload?.RemoteRom?.Game;
                }

                if (game == null)
                {
                    var relativeParseInfo = Parser.Parser.ParsePath(relativeFile);

                    if (relativeParseInfo != null)
                    {
                        game = _gameService.FindByTitle(relativeParseInfo.GameTitle);
                    }
                }

                if (game == null)
                {
                    var localRom = new LocalEpisode();
                    localRom.Path = file;
                    localRom.ReleaseGroup = Parser.ReleaseGroupParser.ParseReleaseGroup(file);
                    localRom.Quality = QualityParser.ParseQuality(file);
                    localRom.Languages = LanguageParser.ParseLanguages(file);
                    localRom.Size = _diskProvider.GetFileSize(file);

                    return MapItem(new ImportDecision(localRom,
                        new ImportRejection(ImportRejectionReason.UnknownSeries, "Unknown Game")),
                        rootFolder,
                        downloadId,
                        null);
                }

                var downloadClientItemInfo = trackedDownload?.DownloadItem == null ? null : Parser.Parser.ParseTitle(trackedDownload.DownloadItem.Title);

                var importDecisions = _importDecisionMaker.GetImportDecisions(new List<string> { file },
                    game,
                    trackedDownload?.DownloadItem,
                    downloadClientItemInfo,
                    null,
                    SceneSource(game, baseFolder));

                if (importDecisions.Any())
                {
                    return MapItem(importDecisions.First(), rootFolder, downloadId, null);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to process file: {0}", file);
            }

            return new ManualImportItem
            {
                DownloadId = downloadId,
                Path = file,
                RelativePath = rootFolder.GetRelativePath(file),
                Name = Path.GetFileNameWithoutExtension(file),
                Size = _diskProvider.GetFileSize(file),
                Rejections = new List<ImportRejection>()
            };
        }

        private List<ManualImportItem> ProcessDownloadDirectory(string rootFolder, List<string> videoFiles)
        {
            var items = new List<ManualImportItem>();

            foreach (var file in videoFiles)
            {
                var localRom = new LocalEpisode();
                localRom.Path = file;
                localRom.Quality = new QualityModel(Quality.Unknown);
                localRom.Languages = new List<Language> { Language.Unknown };
                localRom.Size = _diskProvider.GetFileSize(file);

                items.Add(MapItem(new ImportDecision(localRom), rootFolder, null, null));
            }

            return items;
        }

        private bool SceneSource(Game game, string folder)
        {
            return !(game.Path.PathEquals(folder) || game.Path.IsParentPath(folder));
        }

        private TrackedDownload GetTrackedDownload(string downloadId)
        {
            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);

                return trackedDownload;
            }

            return null;
        }

        private ManualImportItem MapItem(ImportDecision decision, string rootFolder, string downloadId, string folderName)
        {
            var item = new ManualImportItem();

            item.Path = decision.LocalEpisode.Path;
            item.FolderName = folderName;
            item.RelativePath = rootFolder.GetRelativePath(decision.LocalEpisode.Path);
            item.Name = Path.GetFileNameWithoutExtension(decision.LocalEpisode.Path);
            item.DownloadId = downloadId;

            if (decision.LocalEpisode.Roms.Any() && decision.LocalEpisode.Roms.Select(c => c.PlatformNumber).Distinct().Count() == 1)
            {
                var platforms = decision.LocalEpisode.Roms.Select(c => c.PlatformNumber).Distinct().ToList();

                if (platforms.Empty())
                {
                    _logger.Warn("Expected one platform, but found none for: {0}", decision.LocalEpisode.Path);
                }
                else if (platforms.Count > 1)
                {
                    _logger.Warn("Expected one platform, but found {0} ({1}) for: {2}", platforms.Count, string.Join(", ", platforms), decision.LocalEpisode.Path);
                }
                else
                {
                    item.PlatformNumber = decision.LocalEpisode.PlatformNumber;
                    item.Roms = decision.LocalEpisode.Roms;
                }
            }

            item.ReleaseGroup = decision.LocalEpisode.ReleaseGroup;
            item.Quality = decision.LocalEpisode.Quality;
            item.Languages = decision.LocalEpisode.Languages;
            item.Size = _diskProvider.GetFileSize(decision.LocalEpisode.Path);
            item.Rejections = decision.Rejections;
            item.IndexerFlags = (int)decision.LocalEpisode.IndexerFlags;
            item.ReleaseType = decision.LocalEpisode.ReleaseType;

            if (decision.LocalEpisode.Game != null)
            {
                item.Game = decision.LocalEpisode.Game;
                item.CustomFormats = _localRomFormatCalculator.ParseEpisodeCustomFormats(decision.LocalEpisode);
                item.CustomFormatScore = item.Game.QualityProfile?.Value.CalculateCustomFormatScore(item.CustomFormats) ?? 0;
            }

            return item;
        }

        private ManualImportItem MapItem(RomFile romFile, Game game, string folderName, List<Rom> roms)
        {
            var item = new ManualImportItem();

            item.Path = Path.Combine(game.Path, romFile.RelativePath);
            item.FolderName = folderName;
            item.RelativePath = romFile.RelativePath;
            item.Name = Path.GetFileNameWithoutExtension(romFile.Path);
            item.Game = game;
            item.PlatformNumber = romFile.PlatformNumber;
            item.Roms = roms.Where(e => e.EpisodeFileId == romFile.Id).ToList();
            item.ReleaseGroup = romFile.ReleaseGroup;
            item.Quality = romFile.Quality;
            item.Languages = romFile.Languages;
            item.IndexerFlags = (int)romFile.IndexerFlags;
            item.ReleaseType = romFile.ReleaseType;
            item.Size = _diskProvider.GetFileSize(item.Path);
            item.Rejections = Enumerable.Empty<ImportRejection>();
            item.EpisodeFileId = romFile.Id;
            item.CustomFormats = _formatCalculator.ParseCustomFormat(romFile, game);

            return item;
        }

        public void Execute(ManualImportCommand message)
        {
            _logger.ProgressTrace("Manually importing {0} files using mode {1}", message.Files.Count, message.ImportMode);

            var imported = new List<ImportResult>();
            var importedTrackedDownload = new List<ManuallyImportedFile>();
            var importedUntrackedDownload = new List<ImportResult>();

            for (var i = 0; i < message.Files.Count; i++)
            {
                _logger.ProgressTrace("Processing file {0} of {1}", i + 1, message.Files.Count);

                var file = message.Files[i];
                var game = _gameService.GetGame(file.GameId);
                var roms = _romService.GetRoms(file.RomIds);
                var fileRomInfo = Parser.Parser.ParsePath(file.Path) ?? new ParsedRomInfo();
                var existingFile = game.Path.IsParentPath(file.Path);

                TrackedDownload trackedDownload = null;

                var localRom = new LocalEpisode
                {
                    ExistingFile = existingFile,
                    Roms = roms,
                    FileRomInfo = fileRomInfo,
                    Path = file.Path,
                    ReleaseGroup = file.ReleaseGroup,
                    Quality = file.Quality,
                    Languages = file.Languages,
                    IndexerFlags = (IndexerFlags)file.IndexerFlags,
                    ReleaseType = file.ReleaseType,
                    Game = game,
                    Size = 0
                };

                if (file.DownloadId.IsNotNullOrWhiteSpace())
                {
                    trackedDownload = _trackedDownloadService.Find(file.DownloadId);
                    localRom.DownloadClientRomInfo = trackedDownload?.RemoteRom?.ParsedRomInfo;
                    localRom.DownloadItem = trackedDownload?.DownloadItem;
                }

                if (file.FolderName.IsNotNullOrWhiteSpace())
                {
                    localRom.FolderRomInfo = Parser.Parser.ParseTitle(file.FolderName);
                    localRom.SceneSource = !existingFile;
                }

                // Augment rom file so imported files have all additional information an automatic import would
                localRom = _aggregationService.Augment(localRom, trackedDownload?.DownloadItem);

                // Apply the user-chosen values.
                localRom.Game = game;
                localRom.Roms = roms;
                localRom.ReleaseGroup = file.ReleaseGroup;
                localRom.Quality = file.Quality;
                localRom.Languages = file.Languages;
                localRom.IndexerFlags = (IndexerFlags)file.IndexerFlags;
                localRom.ReleaseType = file.ReleaseType;

                _localRomFormatCalculator.UpdateEpisodeCustomFormats(localRom);

                // TODO: Cleanup non-tracked downloads

                var importDecision = new ImportDecision(localRom);

                if (trackedDownload == null)
                {
                    var importResult = _importApprovedEpisodes.Import(new List<ImportDecision> { importDecision }, !existingFile, null, message.ImportMode);

                    imported.AddRange(importResult);
                    importedUntrackedDownload.AddRange(importResult);
                }
                else
                {
                    var importResult = _importApprovedEpisodes.Import(new List<ImportDecision> { importDecision }, true, trackedDownload.DownloadItem, message.ImportMode).First();

                    imported.Add(importResult);

                    importedTrackedDownload.Add(new ManuallyImportedFile
                    {
                        TrackedDownload = trackedDownload,
                        ImportResult = importResult
                    });
                }
            }

            if (imported.Any())
            {
                _logger.ProgressTrace("Manually imported {0} files", imported.Count);
            }

            var untrackedImports = importedUntrackedDownload.Where(i => i.Result == ImportResultType.Imported).ToList();

            if (untrackedImports.Any())
            {
                foreach (var groupedUntrackedImport in untrackedImports.GroupBy(i => new { i.RomFile.GameId, i.RomFile.PlatformNumber }))
                {
                    var localRoms = groupedUntrackedImport.Select(u => u.ImportDecision.LocalEpisode).ToList();
                    var romFiles = groupedUntrackedImport.Select(u => u.RomFile).ToList();
                    var localRom = localRoms.First();
                    var game = localRom.Game;
                    var sourcePath = localRoms.Select(l => l.Path).ToList().GetLongestCommonPath();
                    var roms = localRoms.SelectMany(l => l.Roms).ToList();
                    var parsedRomInfo = localRom.FolderRomInfo ?? localRom.FileRomInfo;

                    _eventAggregator.PublishEvent(new UntrackedDownloadCompletedEvent(game, roms, romFiles, parsedRomInfo, sourcePath));
                }
            }

            foreach (var groupedTrackedDownload in importedTrackedDownload.GroupBy(i => i.TrackedDownload.DownloadItem.DownloadId).ToList())
            {
                var trackedDownload = groupedTrackedDownload.First().TrackedDownload;
                var importedSeries = imported.First().ImportDecision.LocalEpisode.Game;
                var outputPath = trackedDownload.ImportItem.OutputPath.FullPath;

                if (_diskProvider.FolderExists(outputPath))
                {
                    if (_downloadedEpisodesImportService.ShouldDeleteFolder(
                            new DirectoryInfo(outputPath), importedSeries) &&
                        trackedDownload.DownloadItem.CanMoveFiles)
                    {
                        _diskProvider.DeleteFolder(outputPath, true);
                    }
                }

                var importedResults = groupedTrackedDownload.Select(c => c.ImportResult)
                    .Where(c => c.Result == ImportResultType.Imported)
                    .ToList();

                var allEpisodesImported = importedResults
                                                                .SelectMany(c => c.ImportDecision.LocalEpisode.Roms).Count() >=
                                                                    Math.Max(1, trackedDownload.RemoteRom?.Roms?.Count ?? 1);

                if (allEpisodesImported)
                {
                    var romFiles = importedResults.Select(i => i.RomFile).ToList();

                    trackedDownload.State = TrackedDownloadState.Imported;
                    _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload, importedSeries.Id, romFiles, importedResults.First().ImportDecision.LocalEpisode.Release));
                }
            }
        }
    }
}
