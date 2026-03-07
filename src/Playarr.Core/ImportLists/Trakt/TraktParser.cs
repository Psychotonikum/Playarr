using System.Collections.Generic;
using System.Net;
using Playarr.Common.Extensions;
using Playarr.Common.Serializer;
using Playarr.Core.ImportLists.Exceptions;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.ImportLists.Trakt
{
    public class TraktParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;

        public virtual IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var game = new List<ImportListItemInfo>();

            if (!PreProcess(_importResponse))
            {
                return game;
            }

            var traktResponses = STJson.Deserialize<List<TraktResponse>>(_importResponse.Content);

            // no game were returned
            if (traktResponses == null)
            {
                return game;
            }

            foreach (var traktResponse in traktResponses)
            {
                game.AddIfNotNull(new ImportListItemInfo()
                {
                    Title = traktResponse.Show.Title,
                    TvdbId = traktResponse.Show.Ids.Tvdb.GetValueOrDefault(),
                    ImdbId = traktResponse.Show.Ids.Imdb
                });
            }

            return game;
        }

        protected virtual bool PreProcess(ImportListResponse netImportResponse)
        {
            if (netImportResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(netImportResponse, "Trakt API call resulted in an unexpected StatusCode [{0}]", netImportResponse.HttpResponse.StatusCode);
            }

            if (netImportResponse.HttpResponse.Headers.ContentType != null && netImportResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                netImportResponse.HttpRequest.Headers.Accept != null && !netImportResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(netImportResponse, "Trakt API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
