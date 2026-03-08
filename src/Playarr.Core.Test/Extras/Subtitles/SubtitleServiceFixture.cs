using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Extras.Subtitles;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.Extras.Subtitles
{
    [TestFixture]
    public class SubtitleServiceFixture : CoreTest<SubtitleService>
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
                                           .With(e => e.PlatformNumber = 1)
                                           .Build()
                                           .ToList();

            _romFile = Builder<RomFile>.CreateNew()
                                               .With(f => f.Path = Path.Combine(_series.Path, "Platform 1", "Game Title - S01E01.mkv").AsOsAgnostic())
                                               .With(f => f.RelativePath = @"Platform 1\Game Title - S01E01.mkv".AsOsAgnostic())
                                               .Build();

            _localRom = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Game = _series)
                                                 .With(l => l.Roms = roms)
                                                 .With(l => l.Path = Path.Combine(_episodeFolder, "Game.Title.S01E01.mkv").AsOsAgnostic())
                                                 .With(l => l.FileRomInfo = new ParsedRomInfo
                                                 {
                                                     PlatformNumber = 1,
                                                     RomNumbers = new[] { 1 }
                                                 })
                                                 .Build();

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetParentFolder(It.IsAny<string>()))
                  .Returns((string path) => Directory.GetParent(path).FullName);

            Mocker.GetMock<IDetectSample>().Setup(s => s.IsSample(It.IsAny<Game>(), It.IsAny<string>(), It.IsAny<bool>()))
                  .Returns(DetectSampleResult.NotSample);
        }

        [Test]
        [TestCase("Game.Title.S01E01.en.nfo")]
        public void should_not_import_non_subtitle_file(string filePath)
        {
            var files = new List<string> { Path.Combine(_episodeFolder, filePath).AsOsAgnostic() };

            var results = Subject.ImportFiles(_localRom, _romFile, files, true).ToList();

            results.Count.Should().Be(0);
        }

        [Test]
        [TestCase("Game Title - S01E01.srt", "Game Title - S01E01.srt")]
        [TestCase("Game.Title.S01E01.en.srt", "Game Title - S01E01.en.srt")]
        [TestCase("Game.Title.S01E01.english.srt", "Game Title - S01E01.en.srt")]
        [TestCase("Game-Title-S01E01-fr-cc.srt", "Game Title - S01E01.fr.cc.srt")]
        [TestCase("Game Title S01E01_en_sdh_forced.srt", "Game Title - S01E01.en.sdh.forced.srt")]
        [TestCase("Series_Title_S01E01 en.srt", "Game Title - S01E01.en.srt")]
        [TestCase(@"Subs\S01E01.en.srt", "Game Title - S01E01.en.srt")]
        [TestCase(@"Subs\Game.Title.S01E01\2_en.srt", "Game Title - S01E01.en.srt")]
        public void should_import_matching_subtitle_file(string filePath, string expectedOutputPath)
        {
            var files = new List<string> { Path.Combine(_episodeFolder, filePath).AsOsAgnostic() };

            var results = Subject.ImportFiles(_localRom, _romFile, files, true).ToList();

            results.Count.Should().Be(1);

            results[0].RelativePath.AsOsAgnostic().PathEquals(Path.Combine("Platform 1", expectedOutputPath).AsOsAgnostic()).Should().Be(true);
        }

        [Test]
        public void should_import_multiple_subtitle_files_per_language()
        {
            var files = new List<string>
            {
                Path.Combine(_episodeFolder, "Game.Title.S01E01.en.srt").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Game.Title.S01E01.eng.srt").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Subs", "Series_Title_S01E01_en_forced.srt").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Subs", "Game.Title.S01E01", "2_fr.srt").AsOsAgnostic()
            };

            var expectedOutputs = new string[]
            {
                "Game Title - S01E01.1.en.srt",
                "Game Title - S01E01.2.en.srt",
                "Game Title - S01E01.en.forced.srt",
                "Game Title - S01E01.fr.srt",
            };

            var results = Subject.ImportFiles(_localRom, _romFile, files, true).ToList();

            results.Count.Should().Be(expectedOutputs.Length);

            for (var i = 0; i < expectedOutputs.Length; i++)
            {
                results[i].RelativePath.AsOsAgnostic().PathEquals(Path.Combine("Platform 1", expectedOutputs[i]).AsOsAgnostic()).Should().Be(true);
            }
        }

        [Test]
        public void should_import_multiple_subtitle_files_per_language_with_tags()
        {
            var files = new List<string>
            {
                Path.Combine(_episodeFolder, "Game.Title.S01E01.en.forced.cc.srt").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Game.Title.S01E01.other.en.forced.cc.srt").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Game.Title.S01E01.en.forced.sdh.srt").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Game.Title.S01E01.en.forced.default.srt").AsOsAgnostic(),
            };

            var expectedOutputs = new[]
            {
                "Game Title - S01E01.1.en.forced.cc.srt",
                "Game Title - S01E01.2.en.forced.cc.srt",
                "Game Title - S01E01.en.forced.sdh.srt",
                "Game Title - S01E01.en.forced.default.srt"
            };

            var results = Subject.ImportFiles(_localRom, _romFile, files, true).ToList();

            results.Count.Should().Be(expectedOutputs.Length);

            for (var i = 0; i < expectedOutputs.Length; i++)
            {
                results[i].RelativePath.AsOsAgnostic().PathEquals(Path.Combine("Platform 1", expectedOutputs[i]).AsOsAgnostic()).Should().Be(true);
            }
        }

        [Test]
        [TestCase("sub.srt", "Game Title - S01E01.srt")]
        [TestCase(@"Subs\2_en.srt", "Game Title - S01E01.en.srt")]
        public void should_import_unmatching_subtitle_file_if_only_episode(string filePath, string expectedOutputPath)
        {
            var subtitleFile = Path.Combine(_episodeFolder, filePath).AsOsAgnostic();

            var sampleFile = Path.Combine(_series.Path, "Platform 1", "Game Title - S01E01.sample.mkv").AsOsAgnostic();

            var videoFiles = new string[]
            {
                _localRom.Path,
                sampleFile
            };

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetFiles(It.IsAny<string>(), true))
                  .Returns(videoFiles);

            Mocker.GetMock<IDetectSample>().Setup(s => s.IsSample(It.IsAny<Game>(), sampleFile, It.IsAny<bool>()))
                  .Returns(DetectSampleResult.Sample);

            var results = Subject.ImportFiles(_localRom, _romFile, new List<string> { subtitleFile }, true).ToList();

            results.Count.Should().Be(1);

            results[0].RelativePath.AsOsAgnostic().PathEquals(Path.Combine("Platform 1", expectedOutputPath).AsOsAgnostic()).Should().Be(true);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        [TestCase("sub.srt")]
        [TestCase(@"Subs\2_en.srt")]
        public void should_not_import_unmatching_subtitle_file_if_multiple_episodes(string filePath)
        {
            var subtitleFile = Path.Combine(_episodeFolder, filePath).AsOsAgnostic();

            var videoFiles = new string[]
            {
                _localRom.Path,
                Path.Combine(_series.Path, "Platform 1", "Game Title - S01E01.sample.mkv").AsOsAgnostic()
            };

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetFiles(It.IsAny<string>(), true))
                  .Returns(videoFiles);

            var results = Subject.ImportFiles(_localRom, _romFile, new List<string> { subtitleFile }, true).ToList();

            results.Count.Should().Be(0);
        }
    }
}
