using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.ParserTests
{
    [TestFixture]
    public class MultiEpisodeParserFixture : CoreTest
    {
        [TestCase("Game.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", "Game", 3, new[] { 1, 2, 3, 4, 5, 6 })]
        [TestCase("Game.Title.103.104.720p.HDTV.X264-DIMENSION", "Game Title", 1, new[] { 3, 4 })]
        [TestCase("Game.S03E01.S03E02.720p.HDTV.X264-DIMENSION", "Game", 3, new[] { 1, 2 })]
        [TestCase("The Game S01e01 e02 ShoHD On Demand 1080i DD5 1 ALANiS", "The Game", 1, new[] { 1, 2 })]
        [TestCase("Game.Title.2x04.2x05.720p.BluRay-FUTV", "Game Title", 2, new[] { 4, 5 })]
        [TestCase("Game.Title.S07E22E23.720p.HDTV.X264-DIMENSION", "Game Title", 7, new[] { 22, 23 })]
        [TestCase("Game Title - S07E22 - S07E23 - And Lots of Security.. [HDTV-720p].mkv", "Game Title", 7, new[] { 22, 23 })]
        [TestCase("S03E01.S03E02.720p.HDTV.X264-DIMENSION", "", 3, new[] { 1, 2 })]
        [TestCase("2x04x05.720p.BluRay-FUTV", "", 2, new[] { 4, 5 })]
        [TestCase("S02E04E05.720p.BluRay-FUTV", "", 2, new[] { 4, 5 })]
        [TestCase("S02E03-04-05.720p.BluRay-FUTV", "", 2, new[] { 3, 4, 5 })]
        [TestCase("Game.Kings.S02E09-E10.HDTV.x264-ASAP", "Game Kings", 2, new[] { 9, 10 })]
        [TestCase("Game Kings - 2x9-2x10 - Served Code [SDTV] ", "Game Kings", 2, new[] { 9, 10 })]
        [TestCase("Game Kings - 2x09-2x10 - Served Code [SDTV] ", "Game Kings", 2, new[] { 9, 10 })]
        [TestCase("Hell on Game S02E09 E10 HDTV x264 EVOLVE", "Hell on Game", 2, new[] { 9, 10 })]
        [TestCase("Hell.on.Game.S02E09-E10.720p.HDTV.x264-EVOLVE", "Hell on Game", 2, new[] { 9, 10 })]
        [TestCase("Game's Playarr - 8x01_02 - Free Falling", "Game's Playarr", 8, new[] { 1, 2 })]
        [TestCase("8x01_02 - Free Falling", "", 8, new[] { 1, 2 })]
        [TestCase("Game.S01E91-E100", "Game", 1, new[] { 91, 92, 93, 94, 95, 96, 97, 98, 99, 100 })]
        [TestCase("Game.S29E161-E165.PDTV.x264-FQM", "Game", 29, new[] { 161, 162, 163, 164, 165 })]
        [TestCase("Shortland.Game.S22E5363-E5366.HDTV.x264-FiHTV", "Shortland Game", 22, new[] { 5363, 5364, 5365, 5366 })]
        [TestCase("the.Game.101.102.hdtv-lol", "the Game", 1, new[] { 1, 2 })]
        [TestCase("Game.10708.hdtv-lol.mp4", "Game", 1, new[] { 7, 8 })]
        [TestCase("Game.10910.hdtv-lol.mp4", "Game", 1, new[] { 9, 10 })]
        [TestCase("E.010910.HDTVx264REPACKLOL.mp4", "E", 1, new[] { 9, 10 })]
        [TestCase("World Game of Playarr - 2010x15 - 2010x16 - HD TV.mkv", "World Game of Playarr", 2010, new[] { 15, 16 })]
        [TestCase("The Game US S01E01-E02 720p HDTV x264", "The Game US", 1, new[] { 1, 2 })]
        [TestCase("Game Title Platform 01 Rom 05-06 720p", "Game Title", 1, new[] { 5, 6 })]

        // [TestCase("My Name Is Playarr - S03E01-E02 - My Name Is Code 28301-016 [SDTV]", "My Name Is Playarr", 3, new[] { 1, 2 })]
        // [TestCase("Adventure Game - 5x01 - x02 - Dev the Human (2) & Playarr the Robot (3)", "Adventure Game", 5, new [] { 1, 2 })]
        [TestCase("The Game And The Code - S42 Ep10718 - Ep10722", "The Game And The Code", 42, new[] { 10718, 10719, 10720, 10721, 10722 })]
        [TestCase("The Game And The Code - S42 Ep10688 - Ep10692", "The Game And The Code", 42, new[] { 10688, 10689, 10690, 10691, 10692 })]
        [TestCase("Game.S01E02E03.1080p.BluRay.x264-DeBTViD", "Game", 1, new[] { 2, 3 })]
        [TestCase("grp-zoos01e11e12-1080p", "grp-zoo", 1, new[] { 11, 12 })]
        [TestCase("grp-zoo-s01e11e12-1080p", "grp-zoo", 1, new[] { 11, 12 })]
        [TestCase("Game Title.S6.E1.E2.Rom Name.1080p.WEB-DL", "Game Title", 6, new[] { 1, 2 })]
        [TestCase("Game Title.S6E1-E2.Rom Name.1080p.WEB-DL", "Game Title", 6, new[] { 1, 2 })]
        [TestCase("Game Title.S6E1-S6E2.Rom Name.1080p.WEB-DL", "Game Title", 6, new[] { 1, 2 })]
        [TestCase("Game Title.S6E1E2.Rom Name.1080p.WEB-DL", "Game Title", 6, new[] { 1, 2 })]
        [TestCase("Game Title.S6E1-E2-E3.Rom Name.1080p.WEB-DL", "Game Title", 6, new[] { 1, 2, 3 })]
        [TestCase("Game Title.S6.E1E3.Rom Name.1080p.WEB-DL", "Game Title", 6, new[] { 1, 2, 3 })]
        [TestCase("Game Title.S6.E1-E2.Rom Name.1080p.WEB-DL", "Game Title", 6, new[] { 1, 2 })]
        [TestCase("Game Title.S6.E1-S6E2.Rom Name.1080p.WEB-DL", "Game Title", 6, new[] { 1, 2 })]
        [TestCase("Game Title.S6.E1E2.Rom Name.1080p.WEB-DL", "Game Title", 6, new[] { 1, 2 })]
        [TestCase("Game Title.S6.E1-E2-E3.Rom Name.1080p.WEB-DL", "Game Title", 6, new[] { 1, 2, 3 })]
        [TestCase("Game.Title.S05E01-E02.720p.5.1Ch.BluRay", "Game Title", 5, new[] { 1, 2 })]
        [TestCase("Game.Title.S05E01-02.720p.5.1Ch.BluRay", "Game Title", 5, new[] { 1, 2 })]
        [TestCase("S01E01-E03 - Rom Title.HDTV-720p", "", 1, new[] { 1, 2, 3 })]
        [TestCase("1x01-x03 - Rom Title.HDTV-720p", "", 1, new[] { 1, 2, 3 })]
        [TestCase("Game.Title.E07-E08.180612.1080p-NEXT", "Game Title", 1, new[] { 7, 8 })]
        [TestCase("Game Title? E11-E12 1080p HDTV AAC H.264-NEXT", "Game Title", 1, new[] { 11, 12 })]
        [TestCase("The Game Title (2010) - [S01E01-02-03] - Rom Title", "The Game Title (2010)", 1, new[] { 1, 2, 3 })]
        [TestCase("[AqusiQ-TorrentS.pl]The.Name.of.the.Game.S01E05-06.PL.2160p-Ralf[shogho]", "The Name of the Game", 1, new[] { 5, 6 })]
        [TestCase("[AgusiQ-TorrentS.pl] The.Name.of.the.Game.S01E05-E06.PL.1080i.Ralf [jans12]", "The Name of the Game", 1, new[] { 5, 6 })]
        [TestCase("The.Name.of.the.Game.S01E05-6.PL.1080p.WEBRip.x264-666", "The Name of the Game", 1, new[] { 5, 6 })]
        [TestCase("Game Title - S15E06-07 - City Sushi HDTV-720p", "Game Title", 15, new[] { 6, 7 })]
        [TestCase("Game Title - S01E01-02-03 - Rom Title HDTV-720p", "Game Title", 1, new[] { 1, 2, 3 })]
        [TestCase("Game Title - [02x01x02] - Rom 1", "Game Title", 2, new[] { 1, 2 })]
        [TestCase("Game Title - [02x01-x02] - Rom 1", "Game Title", 2, new[] { 1, 2 })]
        [TestCase("Game Title - [02x01-02] - Rom 1", "Game Title", 2, new[] { 1, 2 })]
        [TestCase("Game Title (2011) - S01E23-E24 - ...E i nuovi orizzonti [HDTV 360p] [ITA].mkv", "Game Title (2011)", 1, new[] { 23, 24 })]
        [TestCase("The Game Title! - S01E01-02-03", "The Game Title!", 1, new[] { 1, 2, 3 })]
        [TestCase("Game Title! (2013) - S04E44-E45 - Il 200 spettacolare episodio da narcisisti!", "Game Title! (2013)", 4, new[] { 44, 45 })]
        [TestCase("Game Title! (2013) - S04E44-E45 - Il 200 spettacolare episodio da narcisisti! [NetflixHD 720p HEVC] [ITA+ENG].mkv", "Game Title! (2013)", 4, new[] { 44, 45 })]
        [TestCase("13 Game Se.1 afl.2-3-4 [VTM]", "13 Game", 1, new[] { 2, 3, 4 })]
        [TestCase("Game T Se.3 afl.3 en 4", "Game T", 3, new[] { 3, 4 })]
        [TestCase("Game Title (S15E06-08) City Sushi", "Game Title", 15, new[] { 6, 7, 8 })]
        [TestCase("Game Title (S05E06-08 of 24) City Sushi", "Game Title", 5, new[] { 6, 7, 8 })]
        [TestCase("Game Title (S05E06-08_of_24) City Sushi", "Game Title", 5, new[] { 6, 7, 8 })]
        [TestCase("Game Title (S5E6-8.of.24) City Sushi", "Game Title", 5, new[] { 6, 7, 8 })]
        [TestCase("Game Title (S5E6-8.of.XX) City Sushi", "Game Title", 5, new[] { 6, 7, 8 })]
        [TestCase("Босх: Спадок (S2E1-4) / Game: Legacy (S2E1-4) (2023) WEB-DL 1080p Ukr/Eng | sub Eng", "Game: Legacy", 2, new[] { 1, 2, 3, 4 })]
        [TestCase("Босх: Спадок / Game: Legacy / S2E1-4 of 10 (2023) WEB-DL 1080p Ukr/Eng | sub Eng", "Game: Legacy", 2, new[] { 1, 2, 3, 4 })]
        [TestCase("Game Title - S26E96-97-98-99-100 - Rom 5931 + Rom 5932 + Rom 5933 + Rom 5934 + Rom 5935", "Game Title", 26, new[] { 96, 97, 98, 99, 100 })]
        [TestCase("Game falls - Temporada 1 [HDTV][Cap.111_120]", "Game falls", 1, new[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 })]
        [TestCase("Game.S101E01-E02.Some.Title.Name.1080p.WEB-DL.AAC2.0.H.264-Release", "Game", 101, new[] { 1, 2 })]
        [TestCase("Game.2006.S101E01-E02.Some.Title.Name.1080p.WEB-DL.AAC2.0.H.264-Release", "Game 2006", 101, new[] { 1, 2 })]

        // [TestCase("", "", , new [] {  })]
        public void should_parse_multiple_episodes(string postTitle, string title, int platform, int[] roms)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.PlatformNumber.Should().Be(platform);
            result.RomNumbers.Should().BeEquivalentTo(roms);
            result.GameTitle.Should().Be(title);
            result.AbsoluteRomNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();
        }
    }
}
