using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Test.Framework;
using Playarr.Test.Common;

namespace Playarr.Core.Test.ParserTests
{
    [TestFixture]
    public class PathParserFixture : CoreTest
    {
        [TestCase(@"z:\tv shows\game title (2003)\Platform 3\S03E05 - Title.mkv", 3, 5)]
        [TestCase(@"z:\tv showsseries title\Platform 16\S16E03 - The Title.mkv", 16, 3)]
        [TestCase(@"z:\tv shows\game title\Specials\S00E16 - Dear Title - SD TV.avi", 0, 16)]
        [TestCase(@"D:\shares\TV Shows\game title\Platform 2\S02E21 - 94 Title - 720p TV.mkv", 2, 21)]
        [TestCase(@"D:\shares\TV Shows\Game (2003)\Platform 2\S02E21.avi", 2, 21)]
        [TestCase("C:/Test/TV/Game.4x05.HDTV.XviD-LOL", 4, 5)]
        [TestCase(@"P:\TV Shows\Game\Platform 6\S06E13 - 5 to 9 - 720p BluRay.mkv", 6, 13)]
        [TestCase(@"S:\TV Drop\Game - 10x11 - Title [SDTV]\1011 - Title.avi", 10, 11)]
        [TestCase(@"/TV Drop/Game - 10x11 - Title [SDTV]/1011 - Title.avi", 10, 11)]
        [TestCase(@"S:\TV Drop\Game Title - 10x12 - 24 Hours of Development [SDTV]\1012 - Hours of Development.avi", 10, 12)]
        [TestCase(@"/TV Drop/Game Title - 10x12 - 24 Hours of Development [SDTV]/1012 - Hours of Development.avi", 10, 12)]
        [TestCase(@"S:\TV Drop\Game Title - 10x12 - 24 Hours of Development [SDTV]\Hours of Development.avi", 10, 12)]
        [TestCase(@"/TV Drop/Game Title - 10x12 - 24 Hours of Development [SDTV]/Hours of Development.avi", 10, 12)]
        [TestCase(@"E:\Downloads\tv\Game.Title.S01E01.720p.HDTV\ajifajjjeaeaeqwer_eppj.avi", 1, 1)]
        [TestCase(@"C:\Test\Unsorted\Game.Title.S01E01.720p.HDTV\tbbt101.avi", 1, 1)]
        [TestCase(@"C:\Test\Unsorted\Game.Title.S02E19.720p.BluRay.x264-SiNNERS-RP\ba27283b17c00d01193eacc02a8ba98eeb523a76.mkv", 2, 19)]
        [TestCase(@"C:\Test\Unsorted\Game.Title.S02E18.720p.BluRay.x264-SiNNERS-RP\45a55debe3856da318cc35882ad07e43cd32fd15.mkv", 2, 18)]
        [TestCase(@"C:\Test\Game\Platform 01\01 Pilot (1080p HD).mkv", 1, 1)]
        [TestCase(@"C:\Test\Game\Platform 01\1 Pilot (1080p HD).mkv", 1, 1)]
        [TestCase(@"C:\Test\Game\Platform 1\02 Honor Thy Father (1080p HD).m4v", 1, 2)]
        [TestCase(@"C:\Test\Game\Platform 1\2 Honor Thy Developer (1080p HD).m4v", 1, 2)]
        [TestCase(@"C:\Test\Game\Platform 2 - Total Game Action\01. Total Game Action - Rom 1 - Monster Cash.mkv", 2, 1)]
        [TestCase(@"C:\Test\Game\Platform 2\01. Total Game Action - Rom 1 - Monster Cash.mkv", 2, 1)]
        [TestCase(@"C:\Test\Game\Platform 1\02.04.24 - S01E01 - The Rabbit Hole", 1, 1)]
        [TestCase(@"C:\Test\Game\Platform 1\8 Game Rules - S01E01 - Pilot", 1, 1)]

        // [TestCase(@"C:\game.state.S02E04.720p.WEB-DL.DD5.1.H.264\73696S02-04.mkv", 2, 4)] //Gets treated as S01E04 (because it gets parsed as anime); 2020-01 broken test case: Expected result.RomNumbers to contain 1 item(s), but found 0
        public void should_parse_from_path(string path, int platform, int rom)
        {
            var result = Parser.Parser.ParsePath(path.AsOsAgnostic());

            result.RomNumbers.Should().HaveCount(1);
            result.PlatformNumber.Should().Be(platform);
            result.RomNumbers[0].Should().Be(rom);
            result.AbsoluteRomNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("01-03\\The Game Title (2010) - 1x01-02-03 - Rom Title HDTV-720p Proper", "The Game Title (2010)", 1, new[] { 1, 2, 3 })]
        [TestCase("Platform 2\\E05-06 - Rom Title HDTV-720p Proper", "", 2, new[] { 5, 6 })]
        public void should_parse_multi_episode_from_path(string path, string title, int platform, int[] roms)
        {
            var result = Parser.Parser.ParsePath(path.AsOsAgnostic());

            result.GameTitle.Should().Be(title);
            result.RomNumbers.Should().HaveCount(roms.Length);
            result.PlatformNumber.Should().Be(platform);
            result.RomNumbers.Should().BeEquivalentTo(roms);
            result.AbsoluteRomNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();

            ExceptionVerification.IgnoreWarns();
        }
    }
}
