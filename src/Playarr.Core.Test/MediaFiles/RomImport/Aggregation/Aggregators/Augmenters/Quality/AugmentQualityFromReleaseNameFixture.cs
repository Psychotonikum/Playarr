using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Download;
using Playarr.Core.Download.History;
using Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    [TestFixture]
    public class AugmentQualityFromReleaseNameFixture : CoreTest<AugmentQualityFromReleaseName>
    {
        private LocalEpisode _localRom;
        private DownloadClientItem _downloadClientItem;
        private ParsedRomInfo _hdtvParsedRomInfo;
        private ParsedRomInfo _webdlParsedRomInfo;

        [SetUp]
        public void Setup()
        {
            _hdtvParsedRomInfo = Builder<ParsedRomInfo>.CreateNew()
                                                               .With(p => p.Quality =
                                                                   new QualityModel(Core.Qualities.Quality.HDTV720p))
                                                               .Build();

            _webdlParsedRomInfo = Builder<ParsedRomInfo>.CreateNew()
                                                                .With(p => p.Quality =
                                                                    new QualityModel(Core.Qualities.Quality.WEBDL720p))
                                                                .Build();

            _localRom = Builder<LocalEpisode>.CreateNew()
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                                                             .Build();
        }

        [Test]
        public void should_return_null_if_download_client_item_is_null()
        {
            Subject.AugmentQuality(_localRom, null).Should().BeNull();
        }

        [Test]
        public void should_return_null_if_no_grabbed_history()
        {
            Mocker.GetMock<IDownloadHistoryService>()
                  .Setup(s => s.GetLatestGrab(It.IsAny<string>()))
                  .Returns((DownloadHistory)null);

            Subject.AugmentQuality(_localRom, _downloadClientItem).Should().BeNull();
        }

        [TestCase("Game.Title.S01E01.1080p.WEB.x264", QualitySource.Web, Confidence.Tag, 1080, Confidence.Tag)]
        [TestCase("Game.Title.S01E01.WEB.x264", QualitySource.Web, Confidence.Tag, 480, Confidence.Fallback)]
        [TestCase("Game.Title.S01E01.720p.x264", QualitySource.Television, Confidence.Fallback, 720, Confidence.Tag)]
        public void should_return_augmented_quality(string title, QualitySource source, Confidence sourceConfidence, int resolution, Confidence resolutionConfidence)
        {
            Mocker.GetMock<IDownloadHistoryService>()
                  .Setup(s => s.GetLatestGrab(It.IsAny<string>()))
                  .Returns(Builder<DownloadHistory>.CreateNew()
                                                   .With(h => h.SourceTitle = title)
                                                   .Build());

            var result = Subject.AugmentQuality(_localRom, _downloadClientItem);

            result.Should().NotBe(null);
            result.Source.Should().Be(source);
            result.SourceConfidence.Should().Be(sourceConfidence);
            result.Resolution.Should().Be(resolution);
            result.ResolutionConfidence.Should().Be(resolutionConfidence);
        }
    }
}
