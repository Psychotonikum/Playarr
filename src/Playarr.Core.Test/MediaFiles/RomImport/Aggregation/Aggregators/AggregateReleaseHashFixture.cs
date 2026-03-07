using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateReleaseHashFixture : CoreTest<AggregateReleaseHash>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew().Build();
        }

        [Test]
        public void should_prefer_file()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC) [ABCDEFGH]");
            var folderRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 [12345678]");
            var downloadClientRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC) [ABCD1234]");
            var localRom = new LocalEpisode
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                DownloadClientRomInfo = downloadClientRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseHash.Should().Be("ABCDEFGH");
        }

        [Test]
        public void should_fallback_to_downloadclient()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC)");
            var downloadClientRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC) [ABCD1234]");
            var folderRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 [12345678]");
            var localRom = new LocalEpisode
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                DownloadClientRomInfo = downloadClientRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseHash.Should().Be("ABCD1234");
        }

        [Test]
        public void should_fallback_to_folder()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC)");
            var downloadClientRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC)");
            var folderRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 [12345678]");
            var localRom = new LocalEpisode
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                DownloadClientRomInfo = downloadClientRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseHash.Should().Be("12345678");
        }
    }
}
