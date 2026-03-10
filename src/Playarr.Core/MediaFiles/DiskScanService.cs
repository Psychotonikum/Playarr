using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.MediaFiles.Commands;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.RootFolders;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles
{
    public interface IDiskScanService
    {
        void Scan(Game game);
        string[] GetVideoFiles(string path, bool allDirectories = true);
        string[] GetNonVideoFiles(string path, bool allDirectories = true);
        List<string> FilterPaths(string basePath, IEnumerable<string> files, bool filterExtras = true);
    }

    public class DiskScanService :
        IDiskScanService,
        IExecute<RescanSeriesCommand>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly IConfigService _configService;
        private readonly IGameService _gameService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMediaFileTableCleanupService _mediaFileTableCleanupService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IUpdateMediaInfo _updateMediaInfoService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public DiskScanService(IDiskProvider diskProvider,
                               IMakeImportDecision importDecisionMaker,
                               IImportApprovedEpisodes importApprovedEpisodes,
                               IConfigService configService,
                               IGameService seriesService,
                               IMediaFileService mediaFileService,
                               IMediaFileTableCleanupService mediaFileTableCleanupService,
                               IRootFolderService rootFolderService,
                               IUpdateMediaInfo updateMediaInfoService,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _diskProvider = diskProvider;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedEpisodes = importApprovedEpisodes;
            _configService = configService;
            _gameService = seriesService;
            _mediaFileService = mediaFileService;
            _mediaFileTableCleanupService = mediaFileTableCleanupService;
            _rootFolderService = rootFolderService;
            _updateMediaInfoService = updateMediaInfoService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private static readonly Regex ExcludedExtrasSubFolderRegex = new Regex(@"(?:\\|\/|^)(?:extras|extrafanart|behind the scenes|deleted scenes|featurettes|interviews|other|scenes|samples|shorts|trailers|theme[-_. ]music|backdrops)(?:\\|\/)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ExcludedSubFoldersRegex = new Regex(@"(?:\\|\/|^)(?:@eadir|\.@__thumb|plex versions|\.[^\\/]+)(?:\\|\/)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ExcludedExtraFilesRegex = new Regex(@"(-(trailer|other|behindthescenes|deleted|featurette|interview|scene|short)\.[^.]+$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ExcludedFilesRegex = new Regex(@"^\.(_|unmanic|DS_Store$)|^Thumbs\.db$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void Scan(Game game)
        {
            var rootFolder = _rootFolderService.GetBestRootFolderPath(game.Path);

            var seriesFolderExists = _diskProvider.FolderExists(game.Path);

            if (!seriesFolderExists)
            {
                if (!_diskProvider.FolderExists(rootFolder))
                {
                    _logger.Warn("Game' root folder ({0}) doesn't exist.", rootFolder);
                    _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(game, SeriesScanSkippedReason.RootFolderDoesNotExist));
                    return;
                }

                if (_diskProvider.FolderEmpty(rootFolder))
                {
                    _logger.Warn("Game' root folder ({0}) is empty.", rootFolder);
                    _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(game, SeriesScanSkippedReason.RootFolderIsEmpty));
                    return;
                }
            }

            _logger.ProgressInfo("Scanning {0}", game.Title);

            if (!seriesFolderExists)
            {
                if (_configService.CreateEmptyGameFolders)
                {
                    if (_configService.DeleteEmptyFolders)
                    {
                        _logger.Debug("Not creating missing game folder: {0} because delete empty game folders is enabled", game.Path);
                    }
                    else
                    {
                        _logger.Debug("Creating missing game folder: {0}", game.Path);

                        _diskProvider.CreateFolder(game.Path);
                        SetPermissions(game.Path);
                    }
                }
                else
                {
                    _logger.Debug("Game folder doesn't exist: {0}", game.Path);
                }

                CleanMediaFiles(game, new List<string>());
                CompletedScanning(game, new List<string>());

                return;
            }

            var videoFilesStopwatch = Stopwatch.StartNew();
            var mediaFileList = FilterPaths(game.Path, GetVideoFiles(game.Path)).ToList();
            videoFilesStopwatch.Stop();
            _logger.Trace("Finished getting rom files for: {0} [{1}]", game, videoFilesStopwatch.Elapsed);

            CleanMediaFiles(game, mediaFileList);

            var seriesFiles = _mediaFileService.GetFilesBySeries(game.Id);
            var unmappedFiles = MediaFileService.FilterExistingFiles(mediaFileList, seriesFiles, game);

            var decisionsStopwatch = Stopwatch.StartNew();
            var decisions = _importDecisionMaker.GetImportDecisions(unmappedFiles, game, false);
            decisionsStopwatch.Stop();
            _logger.Trace("Import decisions complete for: {0} [{1}]", game, decisionsStopwatch.Elapsed);
            _importApprovedEpisodes.Import(decisions, false);

            // Update existing files that have a different file size

            var fileInfoStopwatch = Stopwatch.StartNew();
            var filesToUpdate = new List<RomFile>();

            foreach (var file in seriesFiles)
            {
                var path = Path.Combine(game.Path, file.RelativePath);
                var fileSize = _diskProvider.GetFileSize(path);

                if (file.Size == fileSize)
                {
                    continue;
                }

                file.Size = fileSize;

                if (!_updateMediaInfoService.Update(file, game))
                {
                    filesToUpdate.Add(file);
                }
            }

            // Update any files that had a file size change, but didn't get media info updated.
            if (filesToUpdate.Any())
            {
                _mediaFileService.Update(filesToUpdate);
            }

            fileInfoStopwatch.Stop();
            _logger.Trace("Reprocessing existing files complete for: {0} [{1}]", game, decisionsStopwatch.Elapsed);

            RemoveEmptyGameFolder(game.Path);

            CompletedScanning(game, new List<string>());
        }

        private void CleanMediaFiles(Game game, List<string> mediaFileList)
        {
            _logger.Debug("{0} Cleaning up media files in DB", game);
            _mediaFileTableCleanupService.Clean(game, mediaFileList);
        }

        private void CompletedScanning(Game game, List<string> possibleExtraFiles)
        {
            _logger.Info("Completed scanning disk for {0}", game.Title);
            _eventAggregator.PublishEvent(new SeriesScannedEvent(game, possibleExtraFiles));
        }

        public string[] GetVideoFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for video files", path);

            var filesOnDisk = _diskProvider.GetFiles(path, allDirectories).ToList();

            var mediaFileList = filesOnDisk.Where(file => MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                                           .ToList();

            _logger.Trace("{0} files were found in {1}", filesOnDisk.Count, path);
            _logger.Debug("{0} video files were found in {1}", mediaFileList.Count, path);

            return mediaFileList.ToArray();
        }

        public string[] GetNonVideoFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for non-video files", path);

            var filesOnDisk = _diskProvider.GetFiles(path, allDirectories).ToList();

            var mediaFileList = filesOnDisk.Where(file => !MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                                           .ToList();

            _logger.Trace("{0} files were found in {1}", filesOnDisk.Count, path);
            _logger.Debug("{0} non-video files were found in {1}", mediaFileList.Count, path);

            return mediaFileList.ToArray();
        }

        public List<string> FilterPaths(string basePath, IEnumerable<string> paths, bool filterExtras = true)
        {
            var filteredPaths = paths.Where(path => !ExcludedSubFoldersRegex.IsMatch(basePath.GetRelativePath(path)))
                        .Where(path => !ExcludedFilesRegex.IsMatch(Path.GetFileName(path)))
                        .ToList();

            if (filterExtras)
            {
                filteredPaths = filteredPaths.Where(path => !ExcludedExtrasSubFolderRegex.IsMatch(basePath.GetRelativePath(path)))
                                             .Where(path => !ExcludedExtraFilesRegex.IsMatch(Path.GetFileName(path)))
                                             .ToList();
            }

            return filteredPaths;
        }

        public static List<string> FilteredSubFolderMatches(string subfolder)
        {
            var matches = new List<string>();

            foreach (var match in ExcludedSubFoldersRegex.Matches(subfolder))
            {
                matches.Add(match.ToString());
            }

            foreach (var match in ExcludedExtrasSubFolderRegex.Matches(subfolder))
            {
                matches.Add(match.ToString());
            }

            foreach (var match in ExcludedExtraFilesRegex.Matches(subfolder))
            {
                matches.Add(match.ToString());
            }

            return matches;
        }

        private void SetPermissions(string path)
        {
            if (!_configService.SetPermissionsLinux)
            {
                return;
            }

            try
            {
                _diskProvider.SetPermissions(path, _configService.ChmodFolder, _configService.ChownGroup);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to apply permissions to: " + path);
                _logger.Debug(ex, ex.Message);
            }
        }

        private void RemoveEmptyGameFolder(string path)
        {
            if (_configService.DeleteEmptyFolders)
            {
                _diskProvider.RemoveEmptySubfolders(path);

                if (_diskProvider.FolderEmpty(path))
                {
                    _diskProvider.DeleteFolder(path, true);
                }
            }
        }

        public void Execute(RescanSeriesCommand message)
        {
            if (message.GameId.HasValue)
            {
                var game = _gameService.GetGame(message.GameId.Value);
                Scan(game);
            }
            else
            {
                var allGames = _gameService.GetAllGames();

                foreach (var game in allGames)
                {
                    Scan(game);
                }
            }
        }
    }
}
