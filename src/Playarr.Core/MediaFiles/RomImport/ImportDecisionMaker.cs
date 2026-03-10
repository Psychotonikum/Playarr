using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.MediaFiles.EpisodeImport.Aggregation;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.EpisodeImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Game game);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Game game, bool filterExistingFiles);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Game game, DownloadClientItem downloadClientItem, ParsedRomInfo downloadClientItemInfo, ParsedRomInfo folderInfo, bool sceneSource);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Game game, DownloadClientItem downloadClientItem, ParsedRomInfo downloadClientItemInfo, ParsedRomInfo folderInfo, bool sceneSource, bool filterExistingFiles);
        ImportDecision GetDecision(LocalEpisode localRom, DownloadClientItem downloadClientItem);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification> _specifications;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAggregationService _aggregationService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDetectSample _detectSample;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly ILocalEpisodeCustomFormatCalculationService _formatCalculator;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification> specifications,
                                   IMediaFileService mediaFileService,
                                   IAggregationService aggregationService,
                                   IDiskProvider diskProvider,
                                   IDetectSample detectSample,
                                   ITrackedDownloadService trackedDownloadService,
                                   ILocalEpisodeCustomFormatCalculationService formatCalculator,
                                   Logger logger)
        {
            _specifications = specifications;
            _mediaFileService = mediaFileService;
            _aggregationService = aggregationService;
            _diskProvider = diskProvider;
            _detectSample = detectSample;
            _trackedDownloadService = trackedDownloadService;
            _formatCalculator = formatCalculator;
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Game game)
        {
            return GetImportDecisions(videoFiles, game, false);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Game game, bool filterExistingFiles)
        {
            return GetImportDecisions(videoFiles, game, null, null, null, false, filterExistingFiles);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Game game, DownloadClientItem downloadClientItem, ParsedRomInfo downloadClientItemInfo, ParsedRomInfo folderInfo, bool sceneSource)
        {
            return GetImportDecisions(videoFiles, game, downloadClientItem, downloadClientItemInfo, folderInfo, sceneSource, true);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Game game, DownloadClientItem downloadClientItem, ParsedRomInfo downloadClientItemInfo, ParsedRomInfo folderInfo, bool sceneSource, bool filterExistingFiles)
        {
            var newFiles = filterExistingFiles ? _mediaFileService.FilterExistingFiles(videoFiles.ToList(), game) : videoFiles.ToList();

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count);

            // If not importing from a scene source (game folder for example), then assume all files are not samples
            // to avoid using media info on every file needlessly (especially if Analyse Media Files is disabled).
            var nonSampleVideoFileCount = sceneSource ? GetNonSampleVideoFileCount(newFiles, game, downloadClientItemInfo, folderInfo) : videoFiles.Count;

            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                var localRom = new LocalEpisode
                {
                    Game = game,
                    DownloadClientRomInfo = downloadClientItemInfo,
                    DownloadItem = downloadClientItem,
                    FolderRomInfo = folderInfo,
                    Path = file,
                    SceneSource = sceneSource,
                    ExistingFile = game.Path.IsParentPath(file),
                    OtherVideoFiles = nonSampleVideoFileCount > 1
                };

                decisions.AddIfNotNull(GetDecision(localRom, downloadClientItem, nonSampleVideoFileCount > 1));
            }

            return decisions;
        }

        public ImportDecision GetDecision(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, localRom, downloadClientItem))
                                         .Where(c => c != null);

            return new ImportDecision(localRom, reasons.ToArray());
        }

        private ImportDecision GetDecision(LocalEpisode localRom, DownloadClientItem downloadClientItem, bool otherFiles)
        {
            ImportDecision decision = null;

            try
            {
                var fileRomInfo = Parser.Parser.ParsePath(localRom.Path);

                localRom.FileRomInfo = fileRomInfo;
                localRom.Size = _diskProvider.GetFileSize(localRom.Path);
                localRom.ReleaseType = localRom.DownloadClientRomInfo?.ReleaseType ??
                                           localRom.FolderRomInfo?.ReleaseType ??
                                           localRom.FileRomInfo?.ReleaseType ??
                                           ReleaseType.Unknown;

                _aggregationService.Augment(localRom, downloadClientItem);

                if (localRom.Roms.Empty())
                {
                    if (IsPartialSeason(localRom))
                    {
                        decision = new ImportDecision(localRom, new ImportRejection(ImportRejectionReason.PartialSeason, "Partial platform packs are not supported"));
                    }
                    else if (IsSeasonExtra(localRom))
                    {
                        decision = new ImportDecision(localRom, new ImportRejection(ImportRejectionReason.SeasonExtra, "Extras are not supported"));
                    }
                    else
                    {
                        decision = new ImportDecision(localRom, new ImportRejection(ImportRejectionReason.InvalidSeasonOrEpisode, "Invalid platform or rom"));
                    }
                }
                else
                {
                    if (downloadClientItem?.DownloadId.IsNotNullOrWhiteSpace() == true)
                    {
                        var trackedDownload = _trackedDownloadService.Find(downloadClientItem.DownloadId);

                        if (trackedDownload?.RemoteRom?.Release?.IndexerFlags != null)
                        {
                            localRom.IndexerFlags = trackedDownload.RemoteRom.Release.IndexerFlags;
                        }
                    }

                    _formatCalculator.UpdateEpisodeCustomFormats(localRom);

                    decision = GetDecision(localRom, downloadClientItem);
                }
            }
            catch (AugmentingFailedException)
            {
                decision = new ImportDecision(localRom, new ImportRejection(ImportRejectionReason.UnableToParse, "Unable to parse file"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Couldn't import file. {0}", localRom.Path);

                decision = new ImportDecision(localRom, new ImportRejection(ImportRejectionReason.Error, "Unexpected error processing file"));
            }

            if (decision == null)
            {
                _logger.Error("Unable to make a decision on {0}", localRom.Path);
            }
            else if (decision.Rejections.Any())
            {
                _logger.Debug("File rejected for the following reasons: {0}", string.Join(", ", decision.Rejections));
            }
            else
            {
                _logger.Debug("File accepted");
            }

            return decision;
        }

        private ImportRejection EvaluateSpec(IImportDecisionEngineSpecification spec, LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            try
            {
                var result = spec.IsSatisfiedBy(localRom, downloadClientItem);

                if (!result.Accepted)
                {
                    return new ImportRejection(result.Reason, result.Message);
                }
            }
            catch (Exception e)
            {
                // e.Data.Add("report", remoteRom.Report.ToJson());
                // e.Data.Add("parsed", remoteRom.ParsedRomInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on {0}", localRom.Path);
                return new ImportRejection(ImportRejectionReason.DecisionError, $"{spec.GetType().Name}: {e.Message}");
            }

            return null;
        }

        private int GetNonSampleVideoFileCount(List<string> videoFiles, Game game, ParsedRomInfo downloadClientItemInfo, ParsedRomInfo folderInfo)
        {
            var isPossibleSpecialEpisode = downloadClientItemInfo?.IsPossibleSpecialEpisode ?? false;

            // If we might already have a special, don't try to get it from the folder info.
            isPossibleSpecialEpisode = isPossibleSpecialEpisode || (folderInfo?.IsPossibleSpecialEpisode ?? false);

            return videoFiles.Count(file =>
            {
                var sample = _detectSample.IsSample(game, file, isPossibleSpecialEpisode);

                if (sample == DetectSampleResult.Sample)
                {
                    return false;
                }

                return true;
            });
        }

        private bool IsPartialSeason(LocalEpisode localRom)
        {
            var downloadClientRomInfo = localRom.DownloadClientRomInfo;
            var folderRomInfo = localRom.FolderRomInfo;
            var fileRomInfo = localRom.FileRomInfo;

            if (downloadClientRomInfo != null && downloadClientRomInfo.IsPartialSeason)
            {
                return true;
            }

            if (folderRomInfo != null && folderRomInfo.IsPartialSeason)
            {
                return true;
            }

            if (fileRomInfo != null && fileRomInfo.IsPartialSeason)
            {
                return true;
            }

            return false;
        }

        private bool IsSeasonExtra(LocalEpisode localRom)
        {
            var downloadClientRomInfo = localRom.DownloadClientRomInfo;
            var folderRomInfo = localRom.FolderRomInfo;
            var fileRomInfo = localRom.FileRomInfo;

            if (downloadClientRomInfo != null && downloadClientRomInfo.IsSeasonExtra)
            {
                return true;
            }

            if (folderRomInfo != null && folderRomInfo.IsSeasonExtra)
            {
                return true;
            }

            if (fileRomInfo != null && fileRomInfo.IsSeasonExtra)
            {
                return true;
            }

            return false;
        }
    }
}
