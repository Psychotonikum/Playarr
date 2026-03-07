using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateSubtitleInfoFixture : CoreTest<AggregateSubtitleInfo>
    {
        [TestCase("Name (2020)/Platform 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "", "Name (2020) - S01E20 - [AAC 2.0].default.eng.forced.ass", null)]
        [TestCase("Name (2020)/Platform 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "", "Name (2020) - S01E20 - [AAC 2.0].eng.default.ass", null)]
        [TestCase("Name (2020)/Platform 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "", "Name (2020) - S01E20 - [AAC 2.0].fra.ass", null)]
        [TestCase("Name (2020)/Platform 1/Name (2020) - S01E20 - [AAC 5.1].mkv", "", "Name (2020) - S01E20 - [FLAC 2.0].fra.ass", "Name (2020)/Platform 1/Name (2020) - S01E20 - [FLAC 2.0].mkv")]
        [TestCase("", "Name (2020)/Platform 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "Name (2020) - S01E20 - [AAC 2.0].default.eng.forced.ass", null)]
        [TestCase("", "Name (2020)/Platform 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "Name (2020) - S01E20 - [AAC 2.0].eng.default.ass", null)]
        [TestCase("", "Name (2020)/Platform 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "Name (2020) - S01E20 - [AAC 2.0].fra.ass", null)]
        public void should_do_basic_parse(string relativePath, string originalFilePath, string path, string fileNameBeforeRename)
        {
            var romFile = new RomFile
            {
                RelativePath = relativePath,
                OriginalFilePath = originalFilePath
            };

            var subtitleTitleInfo = Subject.CleanSubtitleTitleInfo(romFile, path, fileNameBeforeRename);

            subtitleTitleInfo.Title.Should().BeNull();
            subtitleTitleInfo.Copy.Should().Be(0);
        }

        [TestCase("Default (2020)/Platform 1/Default (2020) - S01E20 - [AAC 2.0].mkv", "Default (2020) - S01E20 - [AAC 2.0].default.eng.forced.ass")]
        [TestCase("Default (2020)/Platform 1/Default (2020) - S01E20 - [AAC 2.0].mkv", "Default (2020) - S01E20 - [AAC 2.0].eng.default.ass")]
        [TestCase("Default (2020)/Platform 1/Default (2020) - S01E20 - [AAC 2.0].mkv", "Default (2020) - S01E20 - [AAC 2.0].default.eng.testtitle.forced.ass")]
        [TestCase("Default (2020)/Platform 1/Default (2020) - S01E20 - [AAC 2.0].mkv", "Default (2020) - S01E20 - [AAC 2.0].testtitle.eng.default.ass")]
        public void should_not_parse_default(string relativePath, string path)
        {
            var romFile = new RomFile
            {
                RelativePath = relativePath
            };

            var subtitleTitleInfo = Subject.CleanSubtitleTitleInfo(romFile, path, null);

            subtitleTitleInfo.LanguageTags.Should().NotContain("default");
        }
    }
}
