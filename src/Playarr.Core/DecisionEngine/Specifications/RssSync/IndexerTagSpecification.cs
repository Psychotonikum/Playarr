using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.Indexers;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications.RssSync
{
    public class IndexerTagSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IIndexerFactory _indexerFactory;

        public IndexerTagSpecification(Logger logger, IIndexerFactory indexerFactory)
        {
            _logger = logger;
            _indexerFactory = indexerFactory;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (subject.Release == null || subject.Game?.Tags == null || subject.Release.IndexerId == 0)
            {
                return DownloadSpecDecision.Accept();
            }

            IndexerDefinition indexer;
            try
            {
                indexer = _indexerFactory.Get(subject.Release.IndexerId);
            }
            catch (ModelNotFoundException)
            {
                _logger.Debug("Indexer with id {0} does not exist, skipping indexer tags check", subject.Release.IndexerId);
                return DownloadSpecDecision.Accept();
            }

            // If indexer has tags, check that at least one of them is present on the game
            var indexerTags = indexer.Tags;

            if (indexerTags.Any() && indexerTags.Intersect(subject.Game.Tags).Empty())
            {
                _logger.Debug("Indexer {0} has tags. None of these are present on game {1}. Rejecting", subject.Release.Indexer, subject.Game);

                return DownloadSpecDecision.Reject(DownloadRejectionReason.NoMatchingTag, "Game tags do not match any of the indexer tags");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
