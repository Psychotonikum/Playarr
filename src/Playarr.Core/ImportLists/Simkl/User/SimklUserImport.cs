using NLog;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.Parser;

namespace Playarr.Core.ImportLists.Simkl.User
{
    public class SimklUserImport : SimklImportBase<SimklUserSettings>
    {
        public SimklUserImport(IImportListRepository netImportRepository,
                               IHttpClient httpClient,
                               IImportListStatusService netImportStatusService,
                               IConfigService configService,
                               IParsingService parsingService,
                               ILocalizationService localizationService,
                               Logger logger)
        : base(netImportRepository, httpClient, netImportStatusService, configService, parsingService, localizationService, logger)
        {
        }

        public override string Name => _localizationService.GetLocalizedString("ImportListsSimklSettingsName");

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new SimklUserRequestGenerator()
            {
                Settings = Settings,
                ClientId = ClientId
            };
        }
    }
}
