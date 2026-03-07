using NLog;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.Parser;

namespace Playarr.Core.Indexers.FileList
{
    public class FileList : HttpIndexerBase<FileListSettings>
    {
        public override string Name => "FileList";
        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;

        public FileList(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
            : base(httpClient, indexerStatusService, configService, parsingService, logger, localizationService)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new FileListRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new FileListParser(Settings);
        }
    }
}
