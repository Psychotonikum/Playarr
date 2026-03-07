using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Extras.Metadata;
using Playarr.Core.Extras.Metadata.Consumers.Wdtv;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.Extras.Metadata.Consumers.Wdtv
{
    [TestFixture]
    public class FindMetadataFileFixture : CoreTest<WdtvMetadata>
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

        [TestCase("Specials")]
        [TestCase("specials")]
        [TestCase("Platform 1")]
        public void should_return_season_image(string folder)
        {
            var path = Path.Combine(_series.Path, folder, "folder.jpg");

            Subject.FindMetadataFile(_series, path).Type.Should().Be(MetadataType.SeasonImage);
        }

        [TestCase(".xml", MetadataType.EpisodeMetadata)]
        [TestCase(".metathumb", MetadataType.EpisodeImage)]
        public void should_return_metadata_for_episode_if_valid_file_for_episode(string extension, MetadataType type)
        {
            var path = Path.Combine(_series.Path, "the.game.s01e01.rom" + extension);

            Subject.FindMetadataFile(_series, path).Type.Should().Be(type);
        }

        [TestCase(".xml")]
        [TestCase(".metathumb")]
        public void should_return_null_if_not_valid_file_for_episode(string extension)
        {
            var path = Path.Combine(_series.Path, "the.game.rom" + extension);

            Subject.FindMetadataFile(_series, path).Should().BeNull();
        }

        [Test]
        public void should_return_series_image_for_folder_jpg_in_series_folder()
        {
            var path = Path.Combine(_series.Path, "folder.jpg");

            Subject.FindMetadataFile(_series, path).Type.Should().Be(MetadataType.GameImage);
        }
    }
}
