using System.Collections.Generic;
using System.Net;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation;
using Playarr.Common.Serializer;
using Playarr.Core.ImportLists.Exceptions;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.ImportLists.Simkl
{
    public class SimklParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;
        private static readonly Logger Logger = PlayarrLogger.GetLogger(typeof(SimklParser));

        public SimklParser()
        {
        }

        public virtual IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var game = new List<ImportListItemInfo>();

            if (!PreProcess(_importResponse))
            {
                return game;
            }

            var jsonResponse = Json.Deserialize<SimklResponse>(_importResponse.Content);

            // no shows were return
            if (jsonResponse == null)
            {
                return game;
            }

            if (jsonResponse.Anime != null)
            {
                foreach (var show in jsonResponse.Anime)
                {
                    var tentativeIgdbId = int.TryParse(show.Show.Ids.Tvdb, out var igdbId) ? igdbId : 0;

                    if (tentativeIgdbId > 0 && (show.AnimeType is SimklAnimeType.Tv or SimklAnimeType.Ona or SimklAnimeType.Ova or SimklAnimeType.Special))
                    {
                        game.AddIfNotNull(new ImportListItemInfo()
                        {
                            Title = show.Show.Title,
                            ImdbId = show.Show.Ids.Imdb,
                            TvdbId = igdbId,
                            MalId = int.TryParse(show.Show.Ids.Mal, out var malId) ? malId : 0
                        });
                    }
                    else
                    {
                        Logger.Warn("Skipping info grabbing for '{0}' because it is an unsupported content type.", show.Show.Title);
                    }
                }
            }

            if (jsonResponse.Shows != null)
            {
                foreach (var show in jsonResponse.Shows)
                {
                    game.AddIfNotNull(new ImportListItemInfo()
                    {
                        Title = show.Show.Title,
                        TvdbId = int.TryParse(show.Show.Ids.Tvdb, out var igdbId) ? igdbId : 0,
                        ImdbId = show.Show.Ids.Imdb
                    });
                }
            }

            return game;
        }

        protected virtual bool PreProcess(ImportListResponse netImportResponse)
        {
            if (netImportResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(netImportResponse, "Simkl API call resulted in an unexpected StatusCode [{0}]", netImportResponse.HttpResponse.StatusCode);
            }

            if (netImportResponse.HttpResponse.Headers.ContentType != null && netImportResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                netImportResponse.HttpRequest.Headers.Accept != null && !netImportResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(netImportResponse, "Simkl API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
