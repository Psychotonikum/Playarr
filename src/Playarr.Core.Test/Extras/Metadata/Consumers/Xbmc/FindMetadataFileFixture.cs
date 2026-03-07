using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Extras.Metadata;
using Playarr.Core.Extras.Metadata.Consumers.Xbmc;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.Extras.Metadata.Consumers.Xbmc
{
    [TestFixture]
    public class FindMetadataFileFixture : CoreTest<XbmcMetadata>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\TV\The.Game".AsOsAgnostic())
                                     .Build();
        }

        [Test]
        public void should_return_null_if_filename_is_not_handled()
        {
            var path = Path.Combine(_series.Path, "file.jpg");

            Subject.FindMetadataFile(_series, path).Should().BeNull();
        }

        [Test]
        public void should_return_metadata_for_xbmc_nfo()
        {
            var path = Path.Combine(_series.Path, "the.game.s01e01.rom.nfo");

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Setup(v => v.IsXbmcNfoFile(path))
                  .Returns(true);

            Subject.FindMetadataFile(_series, path).Type.Should().Be(MetadataType.EpisodeMetadata);

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Verify(v => v.IsXbmcNfoFile(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_return_null_for_scene_nfo()
        {
            var path = Path.Combine(_series.Path, "the.game.s01e01.rom.nfo");

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Setup(v => v.IsXbmcNfoFile(path))
                  .Returns(false);

            Subject.FindMetadataFile(_series, path).Should().BeNull();

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Verify(v => v.IsXbmcNfoFile(It.IsAny<string>()), Times.Once());
        }
    }
}
