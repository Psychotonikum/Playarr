using NLog;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class SeasonSearchService : IExecute<SeasonSearchCommand>
    {
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public SeasonSearchService(ISearchForReleases releaseSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   Logger logger)
        {
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(SeasonSearchCommand message)
        {
            var decisions = _releaseSearchService.SeasonSearch(message.SeriesId, message.SeasonNumber, false, true, message.Trigger == CommandTrigger.Manual, false).GetAwaiter().GetResult();
            var processed = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();

            _logger.ProgressInfo("Platform search completed. {0} reports downloaded.", processed.Grabbed.Count);
        }
    }
}
