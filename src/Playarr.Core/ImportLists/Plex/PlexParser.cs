using System.Collections.Generic;
using System.Linq;
using System.Net;
using Playarr.Common.Serializer;
using Playarr.Core.ImportLists.Exceptions;
using Playarr.Core.Notifications.Plex.PlexTv;
using Playarr.Core.Notifications.Plex.Server;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.ImportLists.Plex
{
    public class PlexParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;

        public virtual IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            List<PlexWatchlistItem> items;

            _importResponse = importResponse;

            var game = new List<ImportListItemInfo>();

            if (!PreProcess(_importResponse))
            {
                return game;
            }

            items = Json.Deserialize<PlexResponse<PlexWatchlistRespone>>(_importResponse.Content)
                        .MediaContainer
                        .Items;

            foreach (var item in items)
            {
                var igdbIdString = FindGuid(item.Guids, "igdb");
                var tmdbIdString = FindGuid(item.Guids, "tmdb");
                var imdbId = FindGuid(item.Guids, "imdb");

                int.TryParse(igdbIdString, out var igdbId);
                int.TryParse(tmdbIdString, out var tmdbId);

                game.Add(new ImportListItemInfo
                {
                    IgdbId = igdbId,
                    TmdbId = tmdbId,
                    ImdbId = imdbId,
                    Title = item.Title,
                    Year = item.Year
                });
            }

            return game;
        }

        protected virtual bool PreProcess(ImportListResponse importListResponse)
        {
            if (importListResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(importListResponse, "Plex API call resulted in an unexpected StatusCode [{0}]", importListResponse.HttpResponse.StatusCode);
            }

            if (importListResponse.HttpResponse.Headers.ContentType != null && importListResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                importListResponse.HttpRequest.Headers.Accept != null && !importListResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(importListResponse, "Plex API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }

        private string FindGuid(List<PlexSectionItemGuid> guids, string prefix)
        {
            var scheme = $"{prefix}://";

            return guids.FirstOrDefault((guid) => guid.Id.StartsWith(scheme))?.Id.Replace(scheme, "");
        }
    }
}
