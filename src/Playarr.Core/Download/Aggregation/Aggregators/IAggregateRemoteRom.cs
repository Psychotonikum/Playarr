using Playarr.Core.Parser.Model;

namespace Playarr.Core.Download.Aggregation.Aggregators
{
    public interface IAggregateRemoteEpisode
    {
        RemoteEpisode Aggregate(RemoteEpisode remoteRom);
    }
}
