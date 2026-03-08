using System;
using System.Collections.Generic;
using Playarr.Core.Games;

namespace Playarr.Core.MetadataSource
{
    public interface IFetchUpcomingReleases
    {
        List<Game> GetUpcomingReleases(DateTime start, DateTime end);
    }
}
