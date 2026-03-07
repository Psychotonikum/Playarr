using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Parser;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.ParserTests
{
    [TestFixture]
    public class UrlFixture : CoreTest
    {
        [TestCase("[www.test.com] - Game.S03E14.720p.HDTV.X264-DIMENSION", "Game")]
        [TestCase("test.net - Game.S03E14.720p.HDTV.X264-DIMENSION", "Game")]
        [TestCase("[www.test-hyphen.com] - Game.S03E14.720p.HDTV.X264-DIMENSION", "Game")]
        [TestCase("www.test123.org - Game.S03E14.720p.HDTV.X264-DIMENSION", "Game")]
        [TestCase("[test.co.uk] - Game.S03E14.720p.HDTV.X264-DIMENSION", "Game")]
        [TestCase("www.test-hyphen.net.au - Game.S03E14.720p.HDTV.X264-DIMENSION", "Game")]
        [TestCase("[www.test123.co.nz] - Game.S03E14.720p.HDTV.X264-DIMENSION", "Game")]
        [TestCase("test-hyphen123.org.au - Game.S03E14.720p.HDTV.X264-DIMENSION", "Game")]
        [TestCase("[www.test123.de] - Mad Game - Platform 1 [Bluray720p]", "Mad Game")]
        [TestCase("www.test-hyphen.de - Mad Game - Platform 1 [Bluray1080p]", "Mad Game")]
        [TestCase("[test-hyphen123.co.za] - The Daily Game - 2023-05-26", "The Daily Game")]
        [TestCase("www.test123.co.za - The Game Bros. (2006) - S01E01", "The Game Bros. (2006)")]
        [TestCase("[www.test-hyphen.ca] - Game (2011) S01", "Game (2011)")]
        [TestCase("test123.ca - Game Time S02 720p HDTV x264 CRON", "Game Time")]
        [TestCase("[www.test-hyphen123.co.za] - Game Title S01E01", "Game Title")]
        [TestCase("(seriesawake.com) Game Super - 57 [720p] [English Subbed]", "Game Super")]

        public void should_not_parse_url_in_name(string postTitle, string title)
        {
            var result = Parser.Parser.ParseSeriesName(postTitle).CleanGameTitle();
            result.Should().Be(title.CleanGameTitle());
        }

        [TestCase("Game.2009.S01E14.English.HDTV.XviD-LOL[www.abb.com]", "LOL")]
        [TestCase("Game 2009 S01E14 English HDTV XviD LOL[www.academy.org]", null)]
        [TestCase("Game Now S05 EXTRAS DVDRip XviD RUNNER[www.aetna.net]", null)]
        [TestCase("Game.Title.S01.EXTRAS.DVDRip.XviD-RUNNER[www.alfaromeo.io]", "RUNNER")]
        [TestCase("2020.Game.2011.12.02.PDTV.XviD-C4TV[rarbg.to]", "C4TV")]
        [TestCase("Game.Title.S01E14.English.HDTV.XviD-LOL[www.abbott.gov]", "LOL")]
        [TestCase("Game 2020 S01E14 English HDTV XviD LOL[www.actor.org]", null)]
        [TestCase("Game Live S05 EXTRAS DVDRip XviD RUNNER[www.agency.net]", null)]
        [TestCase("Game.Title.S02.EXTRAS.DVDRip.XviD-RUNNER[www.airbus.io]", "RUNNER")]
        [TestCase("2021.Game.2012.12.02.PDTV.XviD-C4TV[rarbg.to]", "C4TV")]
        [TestCase("Game.2020.S01E14.English.HDTV.XviD-LOL[www.afl.com]", "LOL")]
        [TestCase("Game 2021 S01E14 English HDTV XviD LOL[www.adult.org]", null)]
        [TestCase("Game Future S05 EXTRAS DVDRip XviD RUNNER[www.allstate.net]", null)]
        [TestCase("Game.Title.S03.EXTRAS.DVDRip.XviD-RUNNER[www.ally.io]", "RUNNER")]
        [TestCase("2022.Game.2013.12.02.PDTV.XviD-C4TV[rarbg.to]", "C4TV")]

        public void should_not_parse_url_in_group(string title, string expected)
        {
            Parser.ReleaseGroupParser.ParseReleaseGroup(title).Should().Be(expected);
        }
    }
}
