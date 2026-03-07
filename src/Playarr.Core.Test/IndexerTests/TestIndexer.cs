using NLog;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Indexers;
using Playarr.Core.Localization;
using Playarr.Core.Parser;

namespace Playarr.Core.Test.IndexerTests
{
    public class TestIndexer : HttpIndexerBase<TestIndexerSettings>
    {
        public override string Name => "Test Indexer";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

        public int _supportedPageSize;
        public override int PageSize => _supportedPageSize;

        public TestIndexer(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
            : base(httpClient, indexerStatusService, configService, parsingService, logger, localizationService)
        {
        }

        public IIndexerRequestGenerator _requestGenerator;
        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return _requestGenerator;
        }

        public IParseIndexerResponse _parser;
        public override IParseIndexerResponse GetParser()
        {
            return _parser;
        }
    }
}
