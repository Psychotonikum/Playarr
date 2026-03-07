using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.Extras.Others;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.Extras.Others
{
    [TestFixture]
    public class OtherExtraServiceFixture : CoreTest<OtherExtraService>
    {
        private Game _series;
        private RomFile _romFile;
        private LocalEpisode _localRom;

        private string _seriesFolder;
        private string _episodeFolder;

        [SetUp]
        public void Setup()
        {
            _seriesFolder = @"C:\Test\TV\Game Title".AsOsAgnostic();
            _episodeFolder = @"C:\Test\Unsorted TV\Game.Title.S01".AsOsAgnostic();

            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Path = _seriesFolder)
                                     .Build();

            var roms = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .Build()
                                           .ToList();

            _romFile = Builder<RomFile>.CreateNew()
                                               .With(f => f.Path = Path.Combine(_series.Path, "Platform 1", "Game Title - S01E01.mkv").AsOsAgnostic())
                                               .With(f => f.RelativePath = @"Platform 1\Game Title - S01E01.mkv")
                                               .Build();

            _localRom = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Game = _series)
                                                 .With(l => l.Roms = roms)
                                                 .With(l => l.Path = Path.Combine(_episodeFolder, "Game.Title.S01E01.mkv").AsOsAgnostic())
                                                 .With(l => l.FileRomInfo = new ParsedRomInfo
                                                 {
                                                     SeasonNumber = 1,
                                                     RomNumbers = new[] { 1 }
                                                 })
                                                 .Build();
        }

        [Test]
        [TestCase("Game Title - S01E01.nfo", "Game Title - S01E01.nfo")]
        [TestCase("Game.Title.S01E01.nfo", "Game Title - S01E01.nfo")]
        [TestCase("Game-Title-S01E01.nfo", "Game Title - S01E01.nfo")]
        [TestCase("Game Title S01E01.nfo", "Game Title - S01E01.nfo")]
        [TestCase("Series_Title_S01E01.nfo", "Game Title - S01E01.nfo")]
        [TestCase("S01E01.thumb.jpg", "Game Title - S01E01.jpg")]
        [TestCase(@"Game.Title.S01E01\thumb.jpg", "Game Title - S01E01.jpg")]
        public void should_import_matching_file(string filePath, string expectedOutputPath)
        {
            var files = new List<string> { Path.Combine(_episodeFolder, filePath).AsOsAgnostic() };

            var results = Subject.ImportFiles(_localRom, _romFile, files, true).ToList();

            results.Count.Should().Be(1);

            results[0].RelativePath.AsOsAgnostic().PathEquals(Path.Combine("Platform 1", expectedOutputPath).AsOsAgnostic()).Should().Be(true);
        }

        [Test]
        public void should_not_import_multiple_nfo_files()
        {
            var files = new List<string>
            {
                Path.Combine(_episodeFolder, "Game.Title.S01E01.nfo").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Series_Title_S01E01.nfo").AsOsAgnostic(),
            };

            var results = Subject.ImportFiles(_localRom, _romFile, files, true).ToList();

            results.Count.Should().Be(1);
        }
    }
}
