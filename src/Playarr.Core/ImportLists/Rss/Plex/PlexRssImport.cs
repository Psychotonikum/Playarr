using System;
using NLog;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.Parser;

namespace Playarr.Core.ImportLists.Rss.Plex
{
    public class PlexRssImport : RssImportBase<PlexRssImportSettings>
    {
        public override string Name => _localizationService.GetLocalizedString("ImportListsPlexSettingsWatchlistRSSName");
        public override ImportListType ListType => ImportListType.Plex;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public PlexRssImport(IHttpClient httpClient,
            IImportListStatusService importListStatusService,
            IConfigService configService,
            IParsingService parsingService,
            ILocalizationService localizationService,
            Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
        {
        }

        public override IParseImportListResponse GetParser()
        {
            return new PlexRssImportParser(_logger);
        }
    }
}
