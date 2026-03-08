using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Common.Serializer;
using Playarr.Core.CustomFormats;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Download.Aggregation;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine
{
    public interface IMakeDownloadDecision
    {
        List<DownloadDecision> GetRssDecision(List<ReleaseInfo> reports, bool pushedRelease = false);
        List<DownloadDecision> GetSearchDecision(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteriaBase);
    }

    public class DownloadDecisionMaker : IMakeDownloadDecision
    {
        private readonly IEnumerable<IDownloadDecisionEngineSpecification> _specifications;
        private readonly IParsingService _parsingService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IRemoteEpisodeAggregationService _aggregationService;
        private readonly ISceneMappingService _sceneMappingService;
        private readonly Logger _logger;

        public DownloadDecisionMaker(IEnumerable<IDownloadDecisionEngineSpecification> specifications,
                                     IParsingService parsingService,
                                     ICustomFormatCalculationService formatService,
                                     IRemoteEpisodeAggregationService aggregationService,
                                     ISceneMappingService sceneMappingService,
                                     Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _formatCalculator = formatService;
            _aggregationService = aggregationService;
            _sceneMappingService = sceneMappingService;
            _logger = logger;
        }

        public List<DownloadDecision> GetRssDecision(List<ReleaseInfo> reports, bool pushedRelease = false)
        {
            return GetDecisions(reports, pushedRelease).ToList();
        }

        public List<DownloadDecision> GetSearchDecision(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteriaBase)
        {
            return GetDecisions(reports, false, searchCriteriaBase).ToList();
        }

        private IEnumerable<DownloadDecision> GetDecisions(List<ReleaseInfo> reports, bool pushedRelease, SearchCriteriaBase searchCriteria = null)
        {
            if (reports.Any())
            {
                _logger.ProgressInfo("Processing {0} releases", reports.Count);
            }
            else
            {
                _logger.ProgressInfo("No results found");
            }

            var reportNumber = 1;

            foreach (var report in reports)
            {
                DownloadDecision decision = null;
                _logger.ProgressTrace("Processing release {0}/{1}", reportNumber, reports.Count);
                _logger.Debug("Processing release '{0}' from '{1}'", report.Title, report.Indexer);

                try
                {
                    var parsedRomInfo = Parser.Parser.ParseTitle(report.Title);

                    if (parsedRomInfo == null || parsedRomInfo.IsPossibleSpecialEpisode)
                    {
                        var specialRomInfo = _parsingService.ParseSpecialRomTitle(parsedRomInfo, report.Title, report.IgdbId, report.MobyGamesId, report.ImdbId, searchCriteria);

                        if (specialRomInfo != null)
                        {
                            parsedRomInfo = specialRomInfo;
                        }
                    }

                    if (parsedRomInfo != null && !parsedRomInfo.GameTitle.IsNullOrWhiteSpace())
                    {
                        var remoteRom = _parsingService.Map(parsedRomInfo, report.IgdbId, report.MobyGamesId, report.ImdbId, searchCriteria);
                        remoteRom.Release = report;
                        remoteRom.ReleaseSource = GetReleaseSource(pushedRelease, searchCriteria);

                        if (remoteRom.Game == null)
                        {
                            var matchingIgdbId = _sceneMappingService.FindIgdbId(parsedRomInfo.GameTitle, parsedRomInfo.ReleaseTitle, parsedRomInfo.PlatformNumber);

                            if (matchingIgdbId.HasValue)
                            {
                                decision = new DownloadDecision(remoteRom, new DownloadRejection(DownloadRejectionReason.MatchesAnotherGame, $"{parsedRomInfo.GameTitle} matches an alias for game with IGDB ID: {matchingIgdbId}"));
                            }
                            else
                            {
                                decision = new DownloadDecision(remoteRom, new DownloadRejection(DownloadRejectionReason.UnknownSeries, "Unknown Game"));
                            }
                        }
                        else if (remoteRom.Roms.Empty())
                        {
                            decision = new DownloadDecision(remoteRom, new DownloadRejection(DownloadRejectionReason.UnknownEpisode, "Unable to identify correct rom(s) using release name and scene mappings"));
                        }
                        else
                        {
                            _aggregationService.Augment(remoteRom);

                            remoteRom.CustomFormats = _formatCalculator.ParseCustomFormat(remoteRom, remoteRom.Release.Size);
                            remoteRom.CustomFormatScore = remoteRom?.Game?.QualityProfile?.Value.CalculateCustomFormatScore(remoteRom.CustomFormats) ?? 0;

                            _logger.Trace("Custom Format Score of '{0}' [{1}] calculated for '{2}'", remoteRom.CustomFormatScore, remoteRom.CustomFormats?.ConcatToString(), report.Title);

                            remoteRom.DownloadAllowed = remoteRom.Roms.Any();
                            decision = GetDecisionForReport(remoteRom, new ReleaseDecisionInformation(pushedRelease, searchCriteria));
                        }
                    }

                    if (searchCriteria != null)
                    {
                        if (parsedRomInfo == null)
                        {
                            parsedRomInfo = new ParsedRomInfo
                            {
                                Languages = LanguageParser.ParseLanguages(report.Title),
                                Quality = QualityParser.ParseQuality(report.Title)
                            };
                        }

                        if (parsedRomInfo.GameTitle.IsNullOrWhiteSpace())
                        {
                            var remoteRom = new RemoteEpisode
                            {
                                Release = report,
                                ReleaseSource = GetReleaseSource(pushedRelease, searchCriteria),
                                ParsedRomInfo = parsedRomInfo,
                                Languages = parsedRomInfo.Languages,
                            };

                            decision = new DownloadDecision(remoteRom, new DownloadRejection(DownloadRejectionReason.UnableToParse, "Unable to parse release"));
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't process release.");

                    var remoteRom = new RemoteEpisode { Release = report, ReleaseSource = GetReleaseSource(pushedRelease, searchCriteria) };

                    decision = new DownloadDecision(remoteRom, new DownloadRejection(DownloadRejectionReason.Error, "Unexpected error processing release"));
                }

                reportNumber++;

                if (decision != null)
                {
                    if (decision.Rejections.Any())
                    {
                        _logger.Debug("Release '{0}' from '{1}' rejected for the following reasons: {2}", report.Title, report.Indexer, string.Join(", ", decision.Rejections));
                    }
                    else
                    {
                        _logger.Debug("Release '{0}' from '{1}' accepted", report.Title, report.Indexer);
                    }

                    yield return decision;
                }
            }
        }

        private DownloadDecision GetDecisionForReport(RemoteEpisode remoteRom, ReleaseDecisionInformation information)
        {
            var reasons = Array.Empty<DownloadRejection>();

            foreach (var specifications in _specifications.GroupBy(v => v.Priority).OrderBy(v => v.Key))
            {
                reasons = specifications.Select(c => EvaluateSpec(c, remoteRom, information))
                                        .Where(c => c != null)
                                        .ToArray();

                if (reasons.Any())
                {
                    break;
                }
            }

            return new DownloadDecision(remoteRom, reasons.ToArray());
        }

        private DownloadRejection EvaluateSpec(IDownloadDecisionEngineSpecification spec, RemoteEpisode remoteRom, ReleaseDecisionInformation information)
        {
            try
            {
                var result = spec.IsSatisfiedBy(remoteRom, information);

                if (!result.Accepted)
                {
                    return new DownloadRejection(result.Reason, result.Message, spec.Type);
                }
            }
            catch (Exception e)
            {
                e.Data.Add("report", remoteRom.Release.ToJson());
                e.Data.Add("parsed", remoteRom.ParsedRomInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on {0}", remoteRom.Release.Title);
                return new DownloadRejection(DownloadRejectionReason.DecisionError, $"{spec.GetType().Name}: {e.Message}");
            }

            return null;
        }

        private ReleaseSourceType GetReleaseSource(bool pushedRelease, SearchCriteriaBase searchCriteria = null)
        {
            if (searchCriteria == null)
            {
                return pushedRelease ? ReleaseSourceType.ReleasePush : ReleaseSourceType.Rss;
            }

            if (searchCriteria.InteractiveSearch)
            {
                return ReleaseSourceType.InteractiveSearch;
            }
            else if (searchCriteria.UserInvokedSearch)
            {
                return ReleaseSourceType.UserInvokedSearch;
            }
            else
            {
                return ReleaseSourceType.Search;
            }
        }
    }
}
