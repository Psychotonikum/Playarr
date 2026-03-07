using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.ParserTests
{
    [TestFixture]
    public class SeasonParserFixture : CoreTest
    {
        [TestCase("30.Game.Platform.04.HDTV.XviD-DIMENSION", "30 Game", 4)]
        [TestCase("Playarr.and.Game.S02.720p.x264-DIMENSION", "Playarr and Game", 2)]
        [TestCase("The.Game.US.S03.720p.x264-DIMENSION", "The Game US", 3)]
        [TestCase(@"Game.of.Playarr.S03.720p.BluRay-CLUE\REWARD", "Game of Playarr", 3)]
        [TestCase("Game Time S02 720p HDTV x264 CRON", "Game Time", 2)]
        [TestCase("Game.2021.S04.iNTERNAL.DVDRip.XviD-VCDVaULT", "Game 2021", 4)]
        [TestCase("Game Five 0 S01 720p WEB DL DD5 1 H 264 NT", "Game Five 0", 1)]
        [TestCase("30 Game S03 WS PDTV XviD FUtV", "30 Game", 3)]
        [TestCase("The Game Platform 4 WS PDTV XviD FUtV", "The Game", 4)]
        [TestCase("Game Platform 1 720p WEB DL DD 5 1 h264 TjHD", "Game", 1)]
        [TestCase("The Game Season4 WS PDTV XviD FUtV", "The Game", 4)]
        [TestCase("Game S 01 720p WEB DL DD 5 1 h264 TjHD", "Game", 1)]
        [TestCase("Game Confidential   Platform 3", "Game Confidential", 3)]
        [TestCase("Game.S01.720p.WEBDL.DD5.1.H.264-NTb", "Game", 1)]
        [TestCase("Game.Makes.It.Right.S02.720p.HDTV.AAC5.1.x265-NOGRP", "Game Makes It Right", 2)]
        [TestCase("My.Game.S2014.720p.HDTV.x264-ME", "My Game", 2014)]
        [TestCase("Game.Saison3.VOSTFR.HDTV.XviD-NOTAG", "Game", 3)]
        [TestCase("Game.SAISON.1.VFQ.PDTV.H264-ACC-ROLLED", "Game", 1)]
        [TestCase("Game Title - Game 1 (1970) DivX", "Game Title", 1)]
        [TestCase("GameTitle.S03.540p.AMZN.WEB-DL.DD+2.0.x264-RTN", "GameTitle", 3)]
        [TestCase("Game.Title.S01.576p.BluRay.DD5.1.x264-HiSD", "Game Title", 1)]
        [TestCase("Game.Stagione.3.HDTV.XviD-NOTAG", "Game", 3)]
        [TestCase("Game.Stagione.3.HDTV.XviD-NOTAG", "Game", 3)]
        [TestCase("Game No More S01 2023 1080p WEB-DL AVC AC3 2.0 Dual Audio -ZR-", "Game No More", 1)]
        [TestCase("Game Title / S1E1-8 of 8 [2024, WEB-DL 1080p] + Original + RUS", "Game Title", 1)]
        [TestCase("Game Title / S2E1-16 of 16 [2022, WEB-DL] RUS", "Game Title", 2)]
        [TestCase("[hchcsen] Mobile Game 00 S01 [BD Remux Dual Audio 1080p AVC 2xFLAC] (Kidou Senshi Gundam 00 Platform 1)", "Mobile Game 00", 1)]
        [TestCase("[HorribleRips] Mobile Game 00 S1 [1080p]", "Mobile Game 00", 1)]
        [TestCase("[Zoombie] Game 100: Bucket List S01 [Web][MKV][h265 10-bit][1080p][AC3 2.0][Softsubs (Zoombie)]", "Game 100: Bucket List", 1)]
        [TestCase("[GROUP] Game: Title (2023) (Platform 1) [BDRip] [1080p Dual Audio HEVC 10 bits DDP] (serie) (Batch)", "Game: Title (2023)", 1)]
        [TestCase("[GROUP] Game: Title (2023) (Platform 1) [BDRip] [1080p Dual Audio HEVC 10-bits DDP] (serie) (Batch)", "Game: Title (2023)", 1)]
        [TestCase("[GROUP] Game: Title (2023) (Platform 1) [BDRip] [1080p Dual Audio HEVC 10-bit DDP] (serie) (Batch)", "Game: Title (2023)", 1)]
        [TestCase("Seriesless (2016/S01/WEB-DL/1080p/AC3 5.1/DUAL/SUB)", "Seriesless (2016)", 1)]
        [TestCase("Game (1994) - Temporada 10", "Game (1994)", 10)]
        [TestCase("Game S01 1080p HMAX WEB-DL DD+ 5.1 H.265", "Game", 1)]
        [TestCase("Game S01 1440p HMAX WEB-DL DD+ 5.1 H.265", "Game", 1)]
        public void should_parse_full_season_release(string postTitle, string title, int platform)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(platform);
            result.GameTitle.Should().Be(title);
            result.RomNumbers.Should().BeEmpty();
            result.AbsoluteRomNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeTrue();
        }

        [TestCase("Acropolis Game S05 EXTRAS DVDRip XviD RUNNER", "Acropolis Game", 5)]
        [TestCase("Punky Game S01 EXTRAS DVDRip XviD RUNNER", "Punky Game", 1)]
        [TestCase("Instant Game S03 EXTRAS DVDRip XviD OSiTV", "Instant Game", 3)]
        [TestCase("The.Game.S03.Extras.01.Deleted.Scenes.720p", "The Game", 3)]
        [TestCase("The.Game.S03.Extras.02.720p", "The Game", 3)]
        public void should_parse_season_extras(string postTitle, string title, int platform)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(platform);
            result.GameTitle.Should().Be(title);
            result.RomNumbers.Should().BeEmpty();
            result.AbsoluteRomNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeTrue();
            result.IsSeasonExtra.Should().BeTrue();
        }

        [TestCase("Game.to.Me.S03.SUBPACK.DVDRip.XviD-REWARD", "Game to Me", 3)]
        [TestCase("The.Game.S02.SUBPACK.DVDRip.XviD-REWARD", "The Game", 2)]
        [TestCase("Game.S11.SUBPACK.DVDRip.XviD-REWARD", "Game", 11)]
        public void should_parse_season_subpack(string postTitle, string title, int platform)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(platform);
            result.GameTitle.Should().Be(title);
            result.RomNumbers.Should().BeEmpty();
            result.AbsoluteRomNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeTrue();
            result.IsSeasonExtra.Should().BeTrue();
        }

        [TestCase("The.Game.2016.S02.Part.1.1080p.NF.WEBRip.DD5.1.x264-NTb", "The Game 2016", 2, 1)]
        [TestCase("The.Game.S07.Vol.1.1080p.NF.WEBRip.DD5.1.x264-NTb", "The Game", 7, 1)]
        [TestCase("The.Game.S06.P1.1080p.Blu-Ray.10-Bit.Dual-Audio.TrueHD.x265-iAHD", "The Game", 6, 1)]
        public void should_parse_partial_season_release(string postTitle, string title, int platform, int seasonPart)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(platform);
            result.GameTitle.Should().Be(title);
            result.RomNumbers.Should().BeEmpty();
            result.AbsoluteRomNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();
            result.IsPartialSeason.Should().BeTrue();
            result.SeasonPart.Should().Be(seasonPart);
        }

        [TestCase("The Game S01-05 WS BDRip X264-REWARD-No Rars", "The Game", 1)]
        [TestCase("Game.Title.S01-S09.1080p.AMZN.WEB-DL.DDP2.0.H.264-NTb", "Game Title", 1)]
        [TestCase("Game Title S01 - S07 BluRay 1080p x264 REPACK -SacReD", "Game Title", 1)]
        [TestCase("Game Title Platform 01-07 BluRay 1080p x264 REPACK -SacReD", "Game Title", 1)]
        [TestCase("Game Title Platform 01 - Platform 07 BluRay 1080p x264 REPACK -SacReD", "Game Title", 1)]
        [TestCase("Game Title Complete Game S01 S04 (1080p BluRay x265 HEVC 10bit AAC 5.1 Vyndros)", "Game Title", 1)]
        [TestCase("Game Title S01 S04 (1080p BluRay x265 HEVC 10bit AAC 5.1 Vyndros)", "Game Title", 1)]
        [TestCase("Game Title S01 04 (1080p BluRay x265 HEVC 10bit AAC 5.1 Vyndros)", "Game Title", 1)]
        public void should_parse_multi_season_release(string postTitle, string title, int firstSeason)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(firstSeason);
            result.GameTitle.Should().Be(title);
            result.RomNumbers.Should().BeEmpty();
            result.AbsoluteRomNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeTrue();
            result.IsPartialSeason.Should().BeFalse();
            result.IsMultiSeason.Should().BeTrue();
        }

        [Test]
        public void should_not_parse_season_folders()
        {
            var result = Parser.Parser.ParseTitle("Platform 3");
            result.Should().BeNull();
        }
    }
}
