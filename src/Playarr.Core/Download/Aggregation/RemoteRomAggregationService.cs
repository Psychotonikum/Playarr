using System;
using System.Collections.Generic;
using NLog;
using Playarr.Core.Download.Aggregation.Aggregators;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Download.Aggregation
{
    public interface IRemoteEpisodeAggregationService
    {
        RemoteRom Augment(RemoteRom remoteRom);
    }

    public class RemoteEpisodeAggregationService : IRemoteEpisodeAggregationService
    {
        private readonly IEnumerable<IAggregateRemoteEpisode> _augmenters;
        private readonly Logger _logger;

        public RemoteEpisodeAggregationService(IEnumerable<IAggregateRemoteEpisode> augmenters,
                                  Logger logger)
        {
            _augmenters = augmenters;
            _logger = logger;
        }

        public RemoteRom Augment(RemoteRom remoteRom)
        {
            if (remoteRom == null)
            {
                return null;
            }

            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(remoteRom);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, ex.Message);
                }
            }

            return remoteRom;
        }
    }
}
