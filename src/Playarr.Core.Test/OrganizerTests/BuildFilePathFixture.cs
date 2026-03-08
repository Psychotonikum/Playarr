using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.MediaFiles;
using Playarr.Core.Organizer;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.OrganizerTests
{
    [TestFixture]

    public class BuildFilePathFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [Test]
        [TestCase("30 Rock - S01E05 - Rom Title", 1, true, "Platform {platform:00}", @"C:\Test\30 Rock\Platform 01\30 Rock - S01E05 - Rom Title.mkv")]
        [TestCase("30 Rock - S01E05 - Rom Title", 1, true, "Platform {platform}", @"C:\Test\30 Rock\Platform 1\30 Rock - S01E05 - Rom Title.mkv")]
        [TestCase("30 Rock - S01E05 - Rom Title", 1, false, "Platform {platform:00}", @"C:\Test\30 Rock\30 Rock - S01E05 - Rom Title.mkv")]
        [TestCase("30 Rock - S01E05 - Rom Title", 1, false, "Platform {platform}", @"C:\Test\30 Rock\30 Rock - S01E05 - Rom Title.mkv")]
        [TestCase("30 Rock - S01E05 - Rom Title", 1, true, "ReallyUglyPlatformFolder {platform}", @"C:\Test\30 Rock\ReallyUglyPlatformFolder 1\30 Rock - S01E05 - Rom Title.mkv")]
        [TestCase("30 Rock - S00E05 - Rom Title", 0, true, "Platform {platform}", @"C:\Test\30 Rock\MySpecials\30 Rock - S00E05 - Rom Title.mkv")]
        public void CalculateFilePath_PlatformFolder_SingleNumber(string filename, int platformNumber, bool usePlatformFolder, string platformFolderFormat, string expectedPath)
        {
            var fakeEpisodes = Builder<Rom>.CreateListOfSize(1)
                .All()
                .With(s => s.Title = "Rom Title")
                .With(s => s.PlatformNumber = platformNumber)
                .With(s => s.EpisodeNumber = 5)
                .Build().ToList();
            var fakeSeries = Builder<Game>.CreateNew()
                .With(s => s.Title = "30 Rock")
                .With(s => s.Path = @"C:\Test\30 Rock".AsOsAgnostic())
                .With(s => s.PlatformFolder = usePlatformFolder)
                .With(s => s.SeriesType = GameTypes.Standard)
                .Build();
            var fakeRomFile = Builder<RomFile>.CreateNew()
                .With(s => s.SceneName = filename)
                .Build();

            _namingConfig.PlatformFolderFormat = platformFolderFormat;
            _namingConfig.SpecialsFolderFormat = "MySpecials";

            Subject.BuildFilePath(fakeEpisodes, fakeSeries, fakeRomFile, ".mkv").Should().Be(expectedPath.AsOsAgnostic());
        }

        [Test]
        public void should_clean_season_folder_when_it_contains_illegal_characters_in_series_title()
        {
            var filename = @"S01E05 - Rom Title";
            var platformNumber = 1;
            var expectedPath = @"C:\Test\NCIS - Los Angeles\NCIS - Los Angeles Platform 1\S01E05 - Rom Title.mkv";

            var fakeEpisodes = Builder<Rom>.CreateListOfSize(1)
                .All()
                .With(s => s.Title = "Rom Title")
                .With(s => s.PlatformNumber = platformNumber)
                .With(s => s.EpisodeNumber = 5)
                .Build().ToList();
            var fakeSeries = Builder<Game>.CreateNew()
                .With(s => s.Title = "NCIS: Los Angeles")
                .With(s => s.Path = @"C:\Test\NCIS - Los Angeles".AsOsAgnostic())
                .With(s => s.PlatformFolder = true)
                .With(s => s.SeriesType = GameTypes.Standard)
                .Build();
            var fakeRomFile = Builder<RomFile>.CreateNew()
                .With(s => s.SceneName = filename)
                .Build();

            _namingConfig.PlatformFolderFormat = "{Game Title} Platform {platform:0}";

            Subject.BuildFilePath(fakeEpisodes, fakeSeries, fakeRomFile, ".mkv").Should().Be(expectedPath.AsOsAgnostic());
        }
    }
}
