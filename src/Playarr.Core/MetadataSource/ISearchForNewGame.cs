using System.Collections.Generic;
using Playarr.Core.Games;

namespace Playarr.Core.MetadataSource
{
    public interface ISearchForNewSeries
    {
        List<Game> SearchForNewSeries(string title);
        List<Game> SearchForNewSeriesByImdbId(string imdbId);
        List<Game> SearchForNewSeriesByAniListId(int aniListId);
        List<Game> SearchForNewSeriesByTmdbId(int tmdbId);
        List<Game> SearchForNewSeriesByMyAnimeListId(int malId);
    }
}
