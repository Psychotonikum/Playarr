using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Download;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles
{
    public interface IDownloadedEpisodesImportService
    {
        List<ImportResult> ProcessRootFolder(DirectoryInfo directoryInfo);
        List<ImportResult> ProcessPath(string path, ImportMode importMode = ImportMode.Auto, Game game = null, DownloadClientItem downloadClientItem = null);
        bool ShouldDeleteFolder(DirectoryInfo directoryInfo, Game game);
    }

    public class DownloadedEpisodesImportService : IDownloadedEpisodesImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly IGameService _gameService;
        private readonly IParsingService _parsingService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly IDetectSample _detectSample;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DownloadedEpisodesImportService(IDiskProvider diskProvider,
                                               IDiskScanService diskScanService,
                                               IGameService seriesService,
                                               IParsingService parsingService,
                                               IMakeImportDecision importDecisionMaker,
                                               IImportApprovedEpisodes importApprovedEpisodes,
                                               IDetectSample detectSample,
                                               IRuntimeInfo runtimeInfo,
                                               IConfigService configService,
                                               Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _gameService = seriesService;
            _parsingService = parsingService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedEpisodes = importApprovedEpisodes;
            _detectSample = detectSample;
            _runtimeInfo = runtimeInfo;
            _configService = configService;
            _logger = logger;
        }

        public List<ImportResult> ProcessRootFolder(DirectoryInfo directoryInfo)
        {
            var results = new List<ImportResult>();

            foreach (var subFolder in _diskProvider.GetDirectories(directoryInfo.FullName))
            {
                var folderResults = ProcessFolder(new DirectoryInfo(subFolder), ImportMode.Auto, null);
                results.AddRange(folderResults);
            }

            foreach (var videoFile in _diskScanService.GetVideoFiles(directoryInfo.FullName, false))
            {
                var fileResults = ProcessFile(new FileInfo(videoFile), ImportMode.Auto, null);
                results.AddRange(fileResults);
            }

            return results;
        }

        public List<ImportResult> ProcessPath(string path, ImportMode importMode = ImportMode.Auto, Game game = null, DownloadClientItem downloadClientItem = null)
        {
            _logger.Debug("Processing path: {0}", path);

            if (_diskProvider.FolderExists(path))
            {
                var directoryInfo = new DirectoryInfo(path);

                if (game == null)
                {
                    return ProcessFolder(directoryInfo, importMode, downloadClientItem);
                }

                return ProcessFolder(directoryInfo, importMode, game, downloadClientItem);
            }

            if (_diskProvider.FileExists(path))
            {
                var fileInfo = new FileInfo(path);

                if (game == null)
                {
                    return ProcessFile(fileInfo, importMode, downloadClientItem);
                }

                return ProcessFile(fileInfo, importMode, game, downloadClientItem);
            }

            LogInaccessiblePathError(path);
            return new List<ImportResult>();
        }

        public bool ShouldDeleteFolder(DirectoryInfo directoryInfo, Game game)
        {
            try
            {
                var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);
                var rarFiles = _diskProvider.GetFiles(directoryInfo.FullName, true).Where(f =>
                    Path.GetExtension(f).Equals(".rar",
                        StringComparison.OrdinalIgnoreCase));

                foreach (var videoFile in videoFiles)
                {
                    var episodeParseResult = Parser.Parser.ParseTitle(Path.GetFileName(videoFile));

                    if (episodeParseResult == null)
                    {
                        _logger.Warn("Unable to parse file on import: [{0}]", videoFile);
                        return false;
                    }

                    if (_detectSample.IsSample(game, videoFile, episodeParseResult.IsPossibleSpecialEpisode) !=
                        DetectSampleResult.Sample)
                    {
                        _logger.Warn("Non-sample file detected: [{0}]", videoFile);
                        return false;
                    }
                }

                if (rarFiles.Any(f => _diskProvider.GetFileSize(f) > 10.Megabytes()))
                {
                    _logger.Warn("RAR file detected, will require manual cleanup");
                    return false;
                }

                return true;
            }
            catch (DirectoryNotFoundException e)
            {
                _logger.Debug(e, "Folder {0} has already been removed", directoryInfo.FullName);
                return false;
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Unable to determine whether folder {0} should be removed", directoryInfo.FullName);
                return false;
            }
        }

        private List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, ImportMode importMode, DownloadClientItem downloadClientItem)
        {
            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var game = _parsingService.GetGame(cleanedUpName);

            if (game == null)
            {
                _logger.Debug("Unknown Game {0}", cleanedUpName);

                return new List<ImportResult>
                       {
                           UnknownSeriesResult("Unknown Game")
                       };
            }

            return ProcessFolder(directoryInfo, importMode, game, downloadClientItem);
        }

        private List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, ImportMode importMode, Game game, DownloadClientItem downloadClientItem)
        {
            if (_gameService.SeriesPathExists(directoryInfo.FullName))
            {
                _logger.Warn("Unable to process folder that is mapped to an existing game");
                return new List<ImportResult>
                {
                    RejectionResult(ImportRejectionReason.GameFolder, "Import path is mapped to a game folder")
                };
            }

            var folderInfo = Parser.Parser.ParseTitle(directoryInfo.Name);
            var videoFiles = _diskScanService.FilterPaths(directoryInfo.FullName, _diskScanService.GetVideoFiles(directoryInfo.FullName));
            var downloadClientItemInfo = downloadClientItem == null ? null : Parser.Parser.ParseTitle(downloadClientItem.Title);

            if (downloadClientItem == null)
            {
                foreach (var videoFile in videoFiles)
                {
                    if (_diskProvider.IsFileLocked(videoFile))
                    {
                        return new List<ImportResult>
                               {
                                   FileIsLockedResult(videoFile)
                               };
                    }
                }
            }

            if (downloadClientItemInfo is { IsMultiSeason: true })
            {
                _logger.Debug("Download client item is marked as multi-platform, not processing automatically to avoid importing incorrect files");

                return new List<ImportResult>
                {
                    RejectionResult(ImportRejectionReason.MultiSeason, "Multi-platform download, unable to import automatically")
                };
            }

            var decisions = _importDecisionMaker.GetImportDecisions(videoFiles.ToList(), game, downloadClientItem, downloadClientItemInfo, folderInfo, true);
            var importResults = _importApprovedEpisodes.Import(decisions, true, downloadClientItem, importMode);

            if (importMode == ImportMode.Auto)
            {
                importMode = (downloadClientItem == null || downloadClientItem.CanMoveFiles) ? ImportMode.Move : ImportMode.Copy;
            }

            if (importMode == ImportMode.Move &&
                importResults.Any(i => i.Result == ImportResultType.Imported) &&
                ShouldDeleteFolder(directoryInfo, game))
            {
                _logger.Debug("Deleting folder after importing valid files");

                try
                {
                    _diskProvider.DeleteFolder(directoryInfo.FullName, true);
                }
                catch (IOException e)
                {
                    _logger.Debug(e, "Unable to delete folder after importing: {0}", e.Message);
                }
            }
            else if (importResults.Empty())
            {
                importResults.AddIfNotNull(CheckEmptyResultForIssue(directoryInfo.FullName));
            }

            return importResults;
        }

        private List<ImportResult> ProcessFile(FileInfo fileInfo, ImportMode importMode, DownloadClientItem downloadClientItem)
        {
            var game = _parsingService.GetGame(Path.GetFileNameWithoutExtension(fileInfo.Name));

            if (game == null)
            {
                _logger.Debug("Unknown Game for file: {0}", fileInfo.Name);

                return new List<ImportResult>
                       {
                           UnknownSeriesResult(string.Format("Unknown Game for file: {0}", fileInfo.Name), fileInfo.FullName)
                       };
            }

            return ProcessFile(fileInfo, importMode, game, downloadClientItem);
        }

        private List<ImportResult> ProcessFile(FileInfo fileInfo, ImportMode importMode, Game game, DownloadClientItem downloadClientItem)
        {
            if (Path.GetFileNameWithoutExtension(fileInfo.Name).StartsWith("._"))
            {
                _logger.Debug("[{0}] starts with '._', skipping", fileInfo.FullName);

                return new List<ImportResult>
                       {
                           new ImportResult(new ImportDecision(new LocalEpisode { Path = fileInfo.FullName }, new ImportRejection(ImportRejectionReason.InvalidFilePath, "Invalid video file, filename starts with '._'")), "Invalid video file, filename starts with '._'")
                       };
            }

            var extension = Path.GetExtension(fileInfo.Name);

            if (FileExtensions.DangerousExtensions.Contains(extension))
            {
                return new List<ImportResult>
                {
                    new ImportResult(new ImportDecision(new LocalEpisode { Path = fileInfo.FullName },
                            new ImportRejection(ImportRejectionReason.DangerousFile, $"Caution: Found potentially dangerous file with extension: {extension}")),
                        $"Caution: Found potentially dangerous file with extension: {extension}")
                };
            }

            if (FileExtensions.ExecutableExtensions.Contains(extension))
            {
                return new List<ImportResult>
                {
                    new ImportResult(new ImportDecision(new LocalEpisode { Path = fileInfo.FullName },
                            new ImportRejection(ImportRejectionReason.ExecutableFile, $"Caution: Found executable file with extension: '{extension}'")),
                        $"Caution: Found executable file with extension: '{extension}'")
                };
            }

            if (_configService.UserRejectedExtensions is not null)
            {
                var userRejectedExtensions = _configService.UserRejectedExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim(' ', '.')
                        .Insert(0, "."))
                    .ToList();

                if (userRejectedExtensions.Contains(extension))
                {
                    return new List<ImportResult>
                    {
                        new ImportResult(new ImportDecision(new LocalEpisode
                                {
                                    Path = fileInfo.FullName
                                },
                                new ImportRejection(ImportRejectionReason.UserRejectedExtension, $"Caution: Found file with user defined rejected extension: '{extension}'")),
                            $"Caution: Found executable file with user defined rejected extension: '{extension}'")
                    };
                }
            }

            if (extension.IsNullOrWhiteSpace() || !MediaFileExtensions.Extensions.Contains(extension))
            {
                _logger.Debug("[{0}] has an unsupported extension: '{1}'", fileInfo.FullName, extension);

                return new List<ImportResult>
                       {
                           new ImportResult(new ImportDecision(new LocalEpisode { Path = fileInfo.FullName },
                               new ImportRejection(ImportRejectionReason.UnsupportedExtension, $"Invalid video file, unsupported extension: '{extension}'")),
                               $"Invalid video file, unsupported extension: '{extension}'")
                       };
            }

            if (downloadClientItem == null)
            {
                if (_diskProvider.IsFileLocked(fileInfo.FullName))
                {
                    return new List<ImportResult>
                           {
                               FileIsLockedResult(fileInfo.FullName)
                           };
                }
            }

            var downloadClientItemInfo = downloadClientItem == null ? null : Parser.Parser.ParseTitle(downloadClientItem.Title);
            var decisions = _importDecisionMaker.GetImportDecisions(new List<string>() { fileInfo.FullName }, game, downloadClientItem, downloadClientItemInfo, null, true);

            return _importApprovedEpisodes.Import(decisions, true, downloadClientItem, importMode);
        }

        private string GetCleanedUpFolderName(string folder)
        {
            folder = folder.Replace("_UNPACK_", "")
                           .Replace("_FAILED_", "");

            return folder;
        }

        private ImportResult FileIsLockedResult(string videoFile)
        {
            _logger.Debug("[{0}] is currently locked by another process, skipping", videoFile);
            return new ImportResult(new ImportDecision(new LocalEpisode { Path = videoFile }, new ImportRejection(ImportRejectionReason.FileLocked, "Locked file, try again later")), "Locked file, try again later");
        }

        private ImportResult UnknownSeriesResult(string message, string videoFile = null)
        {
            var localRom = videoFile == null ? null : new LocalEpisode { Path = videoFile };

            return new ImportResult(new ImportDecision(localRom, new ImportRejection(ImportRejectionReason.UnknownSeries, "Unknown Game")), message);
        }

        private ImportResult RejectionResult(ImportRejectionReason reason, string message)
        {
            return new ImportResult(new ImportDecision(null, new ImportRejection(reason, message)), message);
        }

        private ImportResult CheckEmptyResultForIssue(string folder)
        {
            var files = _diskProvider.GetFiles(folder, true);

            if (files.Any(file => FileExtensions.DangerousExtensions.Contains(Path.GetExtension(file))))
            {
                return RejectionResult(ImportRejectionReason.DangerousFile, "Caution: Found potentially dangerous file");
            }

            if (files.Any(file => FileExtensions.ExecutableExtensions.Contains(Path.GetExtension(file))))
            {
                return RejectionResult(ImportRejectionReason.ExecutableFile, "Caution: Found executable file");
            }

            if (files.Any(file => FileExtensions.ArchiveExtensions.Contains(Path.GetExtension(file))))
            {
                return RejectionResult(ImportRejectionReason.ArchiveFile, "Found archive file, might need to be extracted");
            }

            return null;
        }

        private void LogInaccessiblePathError(string path)
        {
            if (_runtimeInfo.IsWindowsService)
            {
                var mounts = _diskProvider.GetMounts();
                var mount = mounts.FirstOrDefault(m => m.RootDirectory == Path.GetPathRoot(path));

                if (mount == null)
                {
                    _logger.Error("Import failed, path does not exist or is not accessible by Playarr: {0}. Unable to find a volume mounted for the path. If you're using a mapped network drive see the FAQ for more info", path);
                    return;
                }

                if (mount.DriveType == DriveType.Network)
                {
                    _logger.Error("Import failed, path does not exist or is not accessible by Playarr: {0}. It's recommended to avoid mapped network drives when running as a Windows service. See the FAQ for more info", path);
                    return;
                }
            }

            if (OsInfo.IsWindows)
            {
                if (path.StartsWith(@"\\"))
                {
                    _logger.Error("Import failed, path does not exist or is not accessible by Playarr: {0}. Ensure the user running Playarr has access to the network share", path);
                    return;
                }
            }

            _logger.Error("Import failed, path does not exist or is not accessible by Playarr: {0}. Ensure the path exists and the user running Playarr has the correct permissions to access this file/folder", path);
        }
    }
}
