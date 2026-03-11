using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Languages;
using Playarr.Core.Parser;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.ParserTests
{
    [TestFixture]
    public class ParserFixture : CoreTest
    {
        [TestCase("Game Title - 4x05 - Title", "gametitle")]
        [TestCase("Game & Title - 4x05 - Title", "gametitle")]
        [TestCase("Bad Format", "badformat")]
        [TestCase("Mad Game - Platform 1 [Bluray720p]", "madgame")]
        [TestCase("Mad Game - Platform 1 [Bluray1080p]", "madgame")]
        [TestCase("The Daily Game -", "thedailygame")]
        [TestCase("The Game Bros. (2006)", "thegamebros2006")]
        [TestCase("Game (2011)", "game2011")]
        [TestCase("Game Time S02 720p HDTV x264 CRON", "gametime")]
        [TestCase("Game Title 0", "gametitle0")]
        [TestCase("Game of the Day", "gameday")]
        [TestCase("Game of the Day 2", "gameday2")]
        [TestCase("[ www.Torrenting.com ] - Game.S03E14.720p.HDTV.X264-DIMENSION", "game")]
        [TestCase("www.Torrenting.com - Game.S03E14.720p.HDTV.X264-DIMENSION", "game")]
        [TestCase("Game S02E09 HDTV x264-2HD [eztv]-[rarbg.com]", "game")]
        [TestCase("Game.911.S01.DVDRip.DD2.0.x264-DEEP", "game 911")]
        [TestCase("www.Torrenting.org - Game.S03E14.720p.HDTV.X264-DIMENSION", "game")]
        public void should_parse_series_name(string postTitle, string title)
        {
            var result = Parser.Parser.ParseSeriesName(postTitle).CleanGameTitle();
            result.Should().Be(title.CleanGameTitle());
        }

        [TestCase("Game S03E14 720p HDTV X264-DIMENSION", "Game")]
        [TestCase("Game.S03E14.720p.HDTV.X264-DIMENSION", "Game")]
        [TestCase("Game-S03E14-720p-HDTV-X264-DIMENSION", "Game")]
        [TestCase("Game_S03E14_720p_HDTV_X264-DIMENSION", "Game")]
        [TestCase("Game 2022 S03E14 720p HDTV X264-DIMENSION", "Game", 2022)]
        [TestCase("Game (2022) S03E14 720p HDTV X264-DIMENSION", "Game", 2022)]
        [TestCase("Game.2022.S03E14.720p.HDTV.X264-DIMENSION", "Game", 2022)]
        [TestCase("Game-2022-S03E14-720p-HDTV-X264-DIMENSION", "Game", 2022)]
        [TestCase("Game_2022_S03E14_720p_HDTV_X264-DIMENSION", "Game", 2022)]
        [TestCase("1234 S03E14 720p HDTV X264-DIMENSION", "1234")]
        [TestCase("1234.S03E14.720p.HDTV.X264-DIMENSION", "1234")]
        [TestCase("1234-S03E14-720p-HDTV-X264-DIMENSION", "1234")]
        [TestCase("1234_S03E14_720p_HDTV_X264-DIMENSION", "1234")]
        [TestCase("1234 2022 S03E14 720p HDTV X264-DIMENSION", "1234", 2022)]
        [TestCase("1234 (2022) S03E14 720p HDTV X264-DIMENSION", "1234", 2022)]
        [TestCase("1234.2022.S03E14.720p.HDTV.X264-DIMENSION", "1234", 2022)]
        [TestCase("1234-2022-S03E14-720p-HDTV-X264-DIMENSION", "1234", 2022)]
        [TestCase("1234_2022_S03E14_720p_HDTV_X264-DIMENSION", "1234", 2022)]
        public void should_parse_series_title_info(string postTitle, string titleWithoutYear, int year = 0)
        {
            var gameTitleInfo = Parser.Parser.ParseTitle(postTitle).GameTitleInfo;
            gameTitleInfo.TitleWithoutYear.Should().Be(titleWithoutYear);
            gameTitleInfo.Year.Should().Be(year);
        }

        [Test]
        public void should_remove_accents_from_title()
        {
            const string title = "Seri\u00E0es";

            title.CleanGameTitle().Should().Be("seriaes");
        }

        [TestCase("Sonar TV - Game Title : 02 Road From Code [S04].mp4")]
        public void should_clean_up_invalid_path_characters(string postTitle)
        {
            Parser.Parser.ParseTitle(postTitle);
        }

        [TestCase("[scnzbefnet][509103] 2.Developers.Game.S03E18.720p.HDTV.X264-DIMENSION", "2 Developers Game")]
        public void should_remove_request_info_from_title(string postTitle, string title)
        {
            Parser.Parser.ParseTitle(postTitle).GameTitle.Should().Be(title);
        }

        [TestCase("Game.S01E02.Chained.Title.nsp", "Game.S01E02.Chained.Title")]
        public void should_parse_releasetitle(string path, string releaseTitle)
        {
            var result = Parser.Parser.ParseTitle(path);
            result.ReleaseTitle.Should().Be(releaseTitle);
        }

        [TestCase("Босх: Спадок (S2E1) / Game: Legacy (S2E1) (2023) WEB-DL 1080p Ukr/Eng | sub Eng", "Босх: Спадок", "Game: Legacy")]
        [TestCase("Босх: Спадок / Game: Legacy / S2E1-4 of 10 (2023) WEB-DL 1080p Ukr/Eng | sub Eng", "Босх: Спадок", "Game: Legacy")]
        [TestCase("Босх: Спадок AKA Game: Legacy S02 1080p NF WEB-DL Dual- Audio DD+ 5.1 Atmos H.264-APEX", "Босх: Спадок", "Game: Legacy")]
        [TestCase("Босх.Спадок.AKA.Game.Legacy.S02.1080p.NF.WEB-DL.DUAL.DDP5.1.Atmos.H.264-APEX", "Босх Спадок", "Game Legacy")]
        public void should_parse_multiple_series_titles(string postTitle, params string[] titles)
        {
            var gameTitleInfo = Parser.Parser.ParseTitle(postTitle).GameTitleInfo;
            gameTitleInfo.AllTitles.Should().BeEquivalentTo(titles);
        }

        [TestCase("[Reza] Game in Russian - S01E08 [WEBRip 1080p HEVC AAC] (Dual Audio) (Tokidoki Bosotto Russiago de Dereru Tonari no Alya-san)", "Unknown")]
        public void should_parse_language_after_parsing_title(string postTitle, string expectedLanguage)
        {
            var result = Parser.Parser.ParseTitle(postTitle);

            result.Languages.Count.Should().Be(1);
            result.Languages.Should().Contain((Language)expectedLanguage);
        }
    }
}
