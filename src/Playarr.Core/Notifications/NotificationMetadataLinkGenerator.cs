using System.Collections.Generic;
using Playarr.Common.Extensions;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications;

public static class NotificationMetadataLinkGenerator
{
    public static List<NotificationMetadataLink> GenerateLinks(Game game, IEnumerable<int> metadataLinks)
    {
        var links = new List<NotificationMetadataLink>();

        if (game == null)
        {
            return links;
        }

        foreach (var link in metadataLinks)
        {
            var linkType = (MetadataLinkType)link;

            if (linkType == MetadataLinkType.Imdb && game.ImdbId.IsNotNullOrWhiteSpace())
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Imdb, "IMDb", $"https://www.imdb.com/title/{game.ImdbId}"));
            }

            if (linkType == MetadataLinkType.Tvdb && game.TvdbId > 0)
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Tvdb, "TVDb", $"http://www.thetvdb.com/?tab=game&id={game.TvdbId}"));
            }

            if (linkType == MetadataLinkType.Trakt && game.TvdbId > 0)
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Trakt, "Trakt", $"http://trakt.tv/search/tvdb/{game.TvdbId}?id_type=show"));
            }

            if (linkType == MetadataLinkType.Tvmaze && game.RawgId > 0)
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Tvmaze, "TVMaze", $"http://www.tvmaze.com/shows/{game.RawgId}/_"));
            }
        }

        return links;
    }
}
