using System.Collections.Generic;
using Playarr.Common.Extensions;
using Playarr.Common.Serializer;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.ImportLists.Trakt.Popular
{
    public class TraktPopularParser : TraktParser
    {
        private readonly TraktPopularSettings _settings;
        private ImportListResponse _importResponse;

        public TraktPopularParser(TraktPopularSettings settings)
        {
            _settings = settings;
        }

        public override IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var listItems = new List<ImportListItemInfo>();

            if (!PreProcess(_importResponse))
            {
                return listItems;
            }

            var traktSeries = new List<TraktGameResource>();

            if (_settings.TraktListType == (int)TraktPopularListType.Popular)
            {
                traktSeries = STJson.Deserialize<List<TraktGameResource>>(_importResponse.Content);
            }
            else
            {
                traktSeries = STJson.Deserialize<List<TraktResponse>>(_importResponse.Content).SelectList(c => c.Show);
            }

            // no game were returned
            if (traktSeries == null)
            {
                return listItems;
            }

            foreach (var game in traktSeries)
            {
                listItems.AddIfNotNull(new ImportListItemInfo()
                {
                    Title = game.Title,
                    IgdbId = game.Ids.Igdb.GetValueOrDefault(),
                });
            }

            return listItems;
        }
    }
}
