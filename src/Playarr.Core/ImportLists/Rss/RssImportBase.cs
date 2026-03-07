using System;
using NLog;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.Parser;

namespace Playarr.Core.ImportLists.Rss
{
    public class RssImportBase<TSettings> : HttpImportListBase<TSettings>
        where TSettings : RssImportBaseSettings<TSettings>, new()
    {
        public override string Name => "RSS List Base";
        public override ImportListType ListType => ImportListType.Advanced;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public RssImportBase(IHttpClient httpClient,
            IImportListStatusService importListStatusService,
            IConfigService configService,
            IParsingService parsingService,
            ILocalizationService localizationService,
            Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
        {
        }

        public override ImportListFetchResult Fetch()
        {
            return FetchItems(g => g.GetListItems());
        }

        public override IParseImportListResponse GetParser()
        {
            return new RssImportBaseParser(_logger);
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new RssImportRequestGenerator<TSettings>
            {
                Settings = Settings
            };
        }
    }
}
