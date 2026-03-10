using Playarr.Core.Parser.Model;

namespace Playarr.Core.Download.Aggregation.Aggregators
{
    public interface IAggregateRemoteEpisode
    {
        RemoteRom Aggregate(RemoteRom remoteRom);
    }
}
