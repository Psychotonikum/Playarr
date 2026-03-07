using System.Linq;
using System.Threading.Tasks;
using NLog;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Download;
using Playarr.Core.Download.Pending;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Indexers
{
    public class RssSyncService : IExecute<RssSyncCommand>
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public RssSyncService(IFetchAndParseRss rssFetcherAndParser,
                              IMakeDownloadDecision downloadDecisionMaker,
                              IProcessDownloadDecisions processDownloadDecisions,
                              IPendingReleaseService pendingReleaseService,
                              IEventAggregator eventAggregator,
                              Logger logger)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _downloadDecisionMaker = downloadDecisionMaker;
            _processDownloadDecisions = processDownloadDecisions;
            _pendingReleaseService = pendingReleaseService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private async Task<ProcessedDecisions> Sync()
        {
            _logger.ProgressInfo("Starting RSS Sync");

            var rssReleases = await _rssFetcherAndParser.Fetch();
            var pendingReleases = _pendingReleaseService.GetPending();

            var reports = rssReleases.Concat(pendingReleases).ToList();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);
            var processed = await _processDownloadDecisions.ProcessDecisions(decisions);

            if (processed.Pending.Any())
            {
                _logger.ProgressInfo("RSS Sync Completed. Reports found: {0}, Reports grabbed: {1}, Reports pending: {2}", reports.Count, processed.Grabbed.Count, processed.Pending.Count);
            }
            else
            {
                _logger.ProgressInfo("RSS Sync Completed. Reports found: {0}, Reports grabbed: {1}", reports.Count, processed.Grabbed.Count);
            }

            return processed;
        }

        public void Execute(RssSyncCommand message)
        {
            var processed = Sync().GetAwaiter().GetResult();

            _eventAggregator.PublishEvent(new RssSyncCompleteEvent(processed));
        }
    }
}
