using NLog;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.Parser;

namespace Playarr.Core.ImportLists.Trakt.Popular
{
    public class TraktPopularImport : TraktImportBase<TraktPopularSettings>
    {
        public TraktPopularImport(IImportListRepository netImportRepository,
                   IHttpClient httpClient,
                   IImportListStatusService netImportStatusService,
                   IConfigService configService,
                   IParsingService parsingService,
                   ILocalizationService localizationService,
                   Logger logger)
        : base(netImportRepository, httpClient, netImportStatusService, configService, parsingService, localizationService, logger)
        {
        }

        public override string Name => _localizationService.GetLocalizedString("ImportListsTraktSettingsPopularName");

        public override IParseImportListResponse GetParser()
        {
            return new TraktPopularParser(Settings);
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TraktPopularRequestGenerator()
            {
                Settings = Settings,
                ClientId = ClientId
            };
        }
    }
}
