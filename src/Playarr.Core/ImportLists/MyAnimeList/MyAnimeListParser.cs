using System.Collections.Generic;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation;
using Playarr.Common.Serializer;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.ImportLists.MyAnimeList
{
    public class MyAnimeListParser(MyAnimeListSettings settings) : IParseImportListResponse
    {
        private static readonly Logger Logger = PlayarrLogger.GetLogger(typeof(MyAnimeListParser));
        private readonly MyAnimeListSettings _settings = settings;

        public IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse)
        {
            var jsonResponse = Json.Deserialize<MyAnimeListResponse>(importListResponse.Content);
            var game = new List<ImportListItemInfo>();

            foreach (var show in jsonResponse.Animes)
            {
                if (show.ListStatus.Score < _settings.MinimumScore)
                {
                    Logger.Debug("Skipping {0} because score is below threshold", show.AnimeListInfo.Title);
                    continue;
                }

                game.AddIfNotNull(new ImportListItemInfo
                {
                    Title = show.AnimeListInfo.Title,
                    MalId = show.AnimeListInfo.Id
                });
            }

            return game;
        }
    }
}
