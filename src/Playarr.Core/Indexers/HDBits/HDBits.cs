using NLog;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.Parser;

namespace Playarr.Core.Indexers.HDBits
{
    public class HDBits : HttpIndexerBase<HDBitsSettings>
    {
        public override string Name => "HDBits";
        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;
        public override int PageSize => 100;

        public HDBits(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
            : base(httpClient, indexerStatusService, configService, parsingService, logger, localizationService)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new HDBitsRequestGenerator { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new HDBitsParser(Settings);
        }
    }
}
