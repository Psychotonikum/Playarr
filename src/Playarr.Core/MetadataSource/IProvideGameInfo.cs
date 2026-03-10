using System;
using System.Collections.Generic;
using Playarr.Core.Games;

namespace Playarr.Core.MetadataSource
{
    public interface IProvideSeriesInfo
    {
        Tuple<Game, List<Rom>> GetGameInfo(int igdbGameId);
    }
}
