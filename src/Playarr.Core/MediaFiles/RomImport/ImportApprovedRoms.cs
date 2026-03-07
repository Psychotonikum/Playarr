using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Extras;
using Playarr.Core.History;
using Playarr.Core.MediaFiles.Commands;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;

namespace Playarr.Core.MediaFiles.EpisodeImport
{
    public interface IImportApprovedEpisodes
    {
        List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto);
    }

    public class ImportApprovedEpisodes : IImportApprovedEpisodes
    {
        private readonly IUpgradeMediaFiles _romFileUpgrader;
        private readonly IMediaFileService _mediaFileService;
        private readonly IExtraService _extraService;
        private readonly IExistingExtraFiles _existingExtraFiles;
        private readonly IDiskProvider _diskProvider;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public ImportApprovedEpisodes(IUpgradeMediaFiles romFileUpgrader,
                                      IMediaFileService mediaFileService,
                                      IExtraService extraService,
                                      IExistingExtraFiles existingExtraFiles,
                                      IDiskProvider diskProvider,
                                      IHistoryService historyService,
                                      IEventAggregator eventAggregator,
                                      IManageCommandQueue commandQueueManager,
                                      Logger logger)
        {
            _romFileUpgrader = romFileUpgrader;
            _mediaFileService = mediaFileService;
            _extraService = extraService;
            _existingExtraFiles = existingExtraFiles;
            _diskProvider = diskProvider;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        public List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto)
        {
            var qualifiedImports = decisions
                .Where(decision => decision.Approved)
                .GroupBy(decision => decision.LocalEpisode.Game.Id)
                .SelectMany(group => group
                    .OrderByDescending(decision => decision.LocalEpisode.Quality, new QualityModelComparer(group.First().LocalEpisode.Game.QualityProfile))
                    .ThenByDescending(decision => decision.LocalEpisode.Size))
                .ToList();

            var importResults = new List<ImportResult>();

            foreach (var importDecision in qualifiedImports.OrderBy(e => e.LocalEpisode.Roms.Select(rom => rom.EpisodeNumber).MinOrDefault())
                                                           .ThenByDescending(e => e.LocalEpisode.Size))
            {
                var localRom = importDecision.LocalEpisode;
                var oldFiles = new List<DeletedRomFile>();

                try
                {
                    // check if already imported
                    if (importResults.SelectMany(r => r.ImportDecision.LocalEpisode.Roms)
                                         .Select(e => e.Id)
                                         .Intersect(localRom.Roms.Select(e => e.Id))
                                         .Any())
                    {
                        importResults.Add(new ImportResult(importDecision, "Rom has already been imported"));
                        continue;
                    }

                    var romFile = localRom.ToRomFile();
                    romFile.Size = _diskProvider.GetFileSize(localRom.Path);

                    if (downloadClientItem?.DownloadId.IsNotNullOrWhiteSpace() == true)
                    {
                        var grabHistory = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                            .OrderByDescending(h => h.Date)
                            .FirstOrDefault(h => h.EventType == EpisodeHistoryEventType.Grabbed);

                        if (Enum.TryParse(grabHistory?.Data.GetValueOrDefault("indexerFlags"), true, out IndexerFlags flags))
                        {
                            romFile.IndexerFlags = flags;
                        }

                        // Prefer the release type from the grabbed history
                        if (Enum.TryParse(grabHistory?.Data.GetValueOrDefault("releaseType"), true, out ReleaseType releaseType))
                        {
                            if (releaseType != ReleaseType.Unknown)
                            {
                                romFile.ReleaseType = releaseType;
                            }
                        }
                    }

                    bool copyOnly;
                    switch (importMode)
                    {
                        default:
                        case ImportMode.Auto:
                            copyOnly = downloadClientItem is { CanMoveFiles: false };
                            break;
                        case ImportMode.Move:
                            copyOnly = false;
                            break;
                        case ImportMode.Copy:
                            copyOnly = true;
                            break;
                    }

                    if (newDownload)
                    {
                        if (downloadClientItem is { OutputPath.IsEmpty: false })
                        {
                            var outputDirectory = downloadClientItem.OutputPath.Directory.ToString();

                            if (outputDirectory.IsParentPath(localRom.Path))
                            {
                                romFile.OriginalFilePath = outputDirectory.GetRelativePath(localRom.Path);
                            }
                        }

                        oldFiles = _romFileUpgrader.UpgradeRomFile(romFile, localRom, copyOnly).OldFiles;
                    }
                    else
                    {
                        // Delete existing files from the DB mapped to this path
                        var previousFiles = _mediaFileService.GetFilesWithRelativePath(localRom.Game.Id, romFile.RelativePath);

                        foreach (var previousFile in previousFiles)
                        {
                            _mediaFileService.Delete(previousFile, DeleteMediaFileReason.ManualOverride);
                        }
                    }

                    romFile = _mediaFileService.Add(romFile);
                    importResults.Add(new ImportResult(importDecision, romFile));

                    if (newDownload)
                    {
                        if (localRom.ScriptImported)
                        {
                            _existingExtraFiles.ImportExtraFiles(localRom.Game, localRom.PossibleExtraFiles, localRom.FileNameBeforeRename);

                            if (localRom.FileNameBeforeRename != romFile.RelativePath)
                            {
                                _extraService.MoveFilesAfterRename(localRom.Game, romFile);
                            }
                        }

                        if (!localRom.ScriptImported || localRom.ShouldImportExtras)
                        {
                            _extraService.ImportEpisode(localRom, romFile, copyOnly);
                        }
                    }

                    _eventAggregator.PublishEvent(new EpisodeImportedEvent(localRom, romFile, oldFiles, newDownload, downloadClientItem));
                }
                catch (RootFolderNotFoundException e)
                {
                    _logger.Warn(e, "Couldn't import rom " + localRom);
                    _eventAggregator.PublishEvent(new EpisodeImportFailedEvent(e, localRom, newDownload, downloadClientItem));

                    importResults.Add(new ImportResult(importDecision, "Failed to import rom, Root folder missing."));
                }
                catch (DestinationAlreadyExistsException e)
                {
                    _logger.Warn(e, "Couldn't import rom " + localRom);
                    importResults.Add(new ImportResult(importDecision, "Failed to import rom, Destination already exists."));

                    _commandQueueManager.Push(new RescanSeriesCommand(localRom.Game.Id));
                }
                catch (RecycleBinException e)
                {
                    _logger.Warn(e, "Couldn't import rom " + localRom);
                    _eventAggregator.PublishEvent(new EpisodeImportFailedEvent(e, localRom, newDownload, downloadClientItem));

                    importResults.Add(new ImportResult(importDecision, "Failed to import rom, unable to move existing file to the Recycle Bin."));
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Couldn't import rom " + localRom);
                    importResults.Add(new ImportResult(importDecision, "Failed to import rom"));
                }
            }

            // Adding all the rejected decisions
            importResults.AddRange(decisions.Where(c => !c.Approved)
                                            .Select(d => new ImportResult(d, d.Rejections.Select(r => r.Message).ToArray())));

            return importResults;
        }
    }
}
