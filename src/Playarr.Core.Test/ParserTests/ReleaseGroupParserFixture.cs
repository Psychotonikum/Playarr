using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.ParserTests
{
    [TestFixture]
    public class ReleaseGroupParserFixture : CoreTest
    {
        [TestCase("Game.2009.S01E14.English.HDTV.XviD-LOL", "LOL")]
        [TestCase("Game 2009 S01E14 English HDTV XviD LOL", null)]
        [TestCase("Game Now S05 EXTRAS DVDRip XviD RUNNER", null)]
        [TestCase("Game.Title.S01.EXTRAS.DVDRip.XviD-RUNNER", "RUNNER")]
        [TestCase("2020.Game.2011.12.02.PDTV.XviD-C4TV", "C4TV")]
        [TestCase("The.Game.S03E115.DVDRip.XviD-OSiTV", "OSiTV")]
        [TestCase("Game Title - S01E01 - Pilot [HTDV-480p]", null)]
        [TestCase("Game Title - S01E01 - Pilot [HTDV-720p]", null)]
        [TestCase("Game Title - S01E01 - Pilot [HTDV-1080p]", null)]
        [TestCase("The.Game.Title.S04E13.720p.WEB-DL.AAC2.0.H.264-Cyphanix", "Cyphanix")]
        [TestCase("Game.S02E01.720p.WEB-DL.DD5.1.H.264.mkv", null)]
        [TestCase("Game Title S01E01 Rom Title", null)]
        [TestCase("The Game Title - 2014-06-02 - Thomas Piketty.mkv", null)]
        [TestCase("The Game Title S12E17 May 23, 2014.mp4", null)]
        [TestCase("Reizen Waes - S01E08 - Transistri\u00EB, Zuid-Osseti\u00EB en Abchazi\u00EB SDTV.avi", null)]
        [TestCase("The Game Title 10x11 - Wild Devs Cant Be Broken [rl].avi", "rl")]
        [TestCase("[ www.Torrenting.com ] - Game.S03E14.720p.HDTV.X264-DIMENSION", "DIMENSION")]
        [TestCase("Game S02E09 HDTV x264-2HD [eztv]-[rarbg.com]", "2HD")]
        [TestCase("7s-Game-s02e01-720p.mkv", null)]
        [TestCase("The.Game.S09E13.720p.HEVC.x265-MeGusta-Pre", "MeGusta")]
        [TestCase("Game Title - S01E01 - Rom Title [RlsGroup]", "RlsGroup")]
        [TestCase("Red Game S01 E01-E02 1080p AMZN WEBRip DDP5.1 x264 monkee", null)]
        [TestCase("Game.Title.S01E05.The-Aniversary.WEBDL-1080p.mkv", null)]
        [TestCase("Game.Title.S01E05.The-Aniversary.HDTV-1080p.mkv", null)]
        [TestCase("Game US (2010) S04 (1080p BDRip x265 10bit DTS-HD MA 5 1 - WEM)[TAoE]", "TAoE")]
        [TestCase("The.Game.S03E04.2160p.Amazon.WEBRip.DTS-HD.MA.5.1.x264", null)]
        [TestCase("SomeShow.S20E13.1080p.BluRay.DTS-X.MA.5.1.x264", null)]
        [TestCase("SomeShow.S20E13.1080p.BluRay.DTS-MA.5.1.x264", null)]
        [TestCase("SomeShow.S20E13.1080p.BluRay.DTS-ES.5.1.x264", null)]
        [TestCase("SomeShow.S20E13.1080p.Blu-Ray.DTS-ES.5.1.x264", null)]
        [TestCase("SomeShow.S20E13.1080p.Blu-Ray.DTS-ES.5.1.x264-ROUGH [PublicHD]", "ROUGH")]
        [TestCase("SomeShow S01E168 1080p WEB-DL AAC 2.0 x264-Erai-raws", "Erai-raws")]
        [TestCase("The.Good.Game.S05E03.Game.of.Intelligence.1080p.10bit.AMZN.WEB-DL.DDP5.1.HEVC-Vyndros", "Vyndros")]
        [TestCase("[Tenrai-Sensei] Game [BD][1080p][HEVC 10bit x265][Dual Audio]", "Tenrai-Sensei")]
        [TestCase("[Erai-raws] Game - 0955 ~ 1005 [1080p]", "Erai-raws")]
        [TestCase("[Exiled-Destiny] Game Title", "Exiled-Destiny")]
        [TestCase("Game.Title.S01E09.1080p.DSNP.WEB-DL.DDP2.0.H.264-VARYG", "VARYG")]
        [TestCase("Game (1997) - S01E01-02 - Children of the Gods (Showtime) (1080p.BD.DD5.1.x265-TheSickle[TAoE])", "TAoE")]
        [TestCase("Game Title S01 [2160p REMUX] [HEVC DV HYBRID HDR10+ Dolby TrueHD Atmos 7 1 24-bit Audio English] [Data Lass]", null)]
        [TestCase("Game Title S01 [2160p REMUX] [HEVC DV HYBRID HDR10+ Dolby TrueHD Atmos 7 1 24-bit Audio English]-DataLass", "DataLass")]
        [TestCase("Game Title S01 REMUX Dual Audio AVC 1080p 8-Bit-ZR-", "ZR")]
        [TestCase("Show.Name.2009.S01.1080p.BluRay.DTS5.1.x264-D-Z0N3", "D-Z0N3")]
        [TestCase("Game Stampede S01 MULTi 1080p BD x265 Opus AAC -Báleygr", "Báleygr")]
        [TestCase("Game S01E01 VOSTFR 1080p WEB x265 EAC3 -Hveðrungr", "Hveðrungr")]
        public void should_parse_release_group(string title, string expected)
        {
            Parser.ReleaseGroupParser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("Show.Name.S01E01.1080p.WEB-DL.H264.Fight-BB.mkv", "Fight-BB")]
        [TestCase("Show Name (2021) Platform 1 S01 (1080p BluRay x265 HEVC 10bit AAC 5.1 Tigole) [QxR]", "QxR")]
        [TestCase("Show Name (2021) Platform 1 S01 (1080p BluRay x265 HEVC 10bit AAC 2.0 afm72) [QxR]", "QxR")]
        [TestCase("Show Name (2021) Platform 1 S01 (1080p DSNP WEB-DL x265 HEVC 10bit EAC3 5.1 Silence) [QxR]", "QxR")]
        [TestCase("Show Name (2021) Platform 1 S01 (1080p BluRay x265 HEVC 10bit AAC 2.0 Panda) [QxR]", "QxR")]
        [TestCase("Show Name (2020) Platform 1 S01 (1080p AMZN WEB-DL x265 HEVC 10bit EAC3 2.0 Ghost) [QxR]", "QxR")]
        [TestCase("Show Name (2020) Platform 1 S01 (1080p WEB-DL x265 HEVC 10bit AC3 5.1 MONOLITH) [QxR]", "QxR")]
        [TestCase("The Show S08E09 The Game.1080p.AMZN.WEB-DL.x265.10bit.EAC3.6.0-Qman[UTR]", "UTR")]
        [TestCase("The Show S03E07 Fire and Game[1080p x265 10bit S87 Joy]", "Joy")]
        [TestCase("The Show (2016) - S02E01 - Soul Game #1 (1080p NF WEBRip x265 ImE)", "ImE")]
        [TestCase("The Show (2020) - S02E03 - Fighting His Game(1080p ATVP WEB-DL x265 t3nzin)", "t3nzin")]
        [TestCase("[Anime Time] A Show [BD][Dual Audio][1080p][HEVC 10bit x265][AAC][Eng Sub] [Batch] Title)", "Anime Time")]
        [TestCase("[Project Angel] Anime Game [DVD 480p] [10-bit x265 HEVC | Opus]", "Project Angel")]
        [TestCase("[Hakata Ramen] Show Title - Platform 2 - Revival of The Commandments", "Hakata Ramen")]
        [TestCase("Show Name (2022) S01 (2160p DSNP WEB-DL H265 DV HDR DDP Atmos 5.1 English - HONE)", "HONE")]
        [TestCase("Show Title (2021) S01 (2160p ATVP WEB-DL Hybrid H265 DV HDR10+ DDP Atmos 5.1 English - HONE)", "HONE")]
        [TestCase("Game.Title.S01E09.1080p.DSNP.WEB-DL.DDP2.0.H.264-VARYG (Blue Lock, Multi-Subs)", "VARYG")]
        [TestCase("Game.Title (2014) S09E10 (1080p AMZN WEB-DL x265 HEVC 10bit DDP 5.1 Vyndros)", "Vyndros")]
        [TestCase("Game Title S02E03 Title 4k to 1080p DSNP WEBrip x265 DDP 5 1 Releaser[SEV]", "SEV")]
        [TestCase("Game Title Platform 01 S01 1080p AMZN UHD WebRip x265 DDP 5.1 Atmos Releaser-SEV", "SEV")]
        [TestCase("Game Title - S01.E06 - Title 1080p AMZN WebRip x265 DDP 5.1 Atmos Releaser [SEV]", "SEV")]
        [TestCase("Game Title (2005) - S01E01 - A Hard Day's Night (1080p DSNP WEB-DL x265 Garshasp).mkv", "Garshasp")]
        [TestCase("Game Title (2015) - S02E04 - Smoke & Mirrors (1080p BluRay x265 Kappa).mkv", "Kappa")]
        [TestCase("Game Title (2020) - S02E03 - A Great Odyssey (1080p BluRay x265 Kappa).mkv", "Kappa")]
        [TestCase("Game Title (2019) - S01E01 - Rom 1 (1080p WEB-DL x265 Natty).mkv", "Natty")]
        [TestCase("Game Title (1999) - S03E01-E02 - Mermaid Man and Barnacle Boy IV & Doing Time (1080p AMZN WEB-DL x265 RCVR).mkv", "RCVR")]
        [TestCase("Game Title (2021) - S01E02 - Here Goes Nothing (1080p WEB-DL x265 SAMPA).mkv", "SAMPA")]
        [TestCase("Game Title (2021) - S01E01 - Aftermath (1080p DSNP WEB-DL x265 YOGI).mkv", "YOGI")]
        [TestCase("Game Title (2012) - S01E01 - Rom 1 (1080p BluRay x265 r00t).mkv", "r00t")]
        [TestCase("Game Title - S01E01 - Girls Gone Wild Exposed (720p x265 EDGE2020).mkv", "EDGE2020")]
        [TestCase("Game.Title.S01E02.1080p.BluRay.Remux.AVC.FLAC.2.0-E.N.D", "E.N.D")]
        [TestCase("Show Name (2016) Platform 1 S01 (1080p AMZN WEB-DL x265 HEVC 10bit EAC3 5 1 RZeroX) QxR", "QxR")]
        [TestCase("Game Title S01 1080p Blu-ray Remux AVC FLAC 2.0 - KRaLiMaRKo", "KRaLiMaRKo")]
        [TestCase("Game Title S01 1080p Blu-ray Remux AVC DTS-HD MA 2.0 - BluDragon", "BluDragon")]
        [TestCase("Example (2013) S01E01 (1080p iP WEBRip x265 SDR AAC 2.0 English - DarQ)", "DarQ")]
        [TestCase("Game.Title.S08E03.720p.WEB.DL.AAC2.0.H.264.KCRT", "KCRT")]
        [TestCase("Game Title S02E05 2160p WEB-DL DV HDR ENG DDP5.1 Atmos H265 MP4-BEN THE MEN", "BEN THE MEN")]
        [TestCase("Game Title S02E05 2160p AMZN WEB-DL DV HDR10 PLUS DDP5 1 Atmos H265 MKV-BEN THE MEN-xpost", "BEN THE MEN")]
        [TestCase("Game.S01E05.1080p.WEB-DL.DDP5.1.H264-BEN.THE.MEN", "BEN.THE.MEN")]
        [TestCase("Game (2022) S01 (1080p BluRay x265 SDR DDP 5.1 English - JBENT TAoE)", "TAoE")]
        [TestCase("Game (2005) S21E12 (1080p AMZN WEB-DL x265 SDR DDP 5.1 English - Goki TAoE)", "TAoE")]
        [TestCase("Game (2022) S03E12 (1080p AMZN Webrip x265 10 bit EAC3 5 1 - Ainz)[TAoE]", "TAoE")]
        [TestCase("Game Things (2016) S04 Part 1 (1080p Webrip NF x265 10bit EAC3 5 1 - AJJMIN) [TAoE]", "TAoE")]
        [TestCase("Game Soup (2024) S01 (1080p NF Webrip x265 10bit EAC3 5 1 Multi - ANONAZ)[TAoE]", "TAoE")]
        [TestCase("Game (2022) S01 (1080p NF Webrip x265 10bit EAC3 5 1 Atmos - ArcX)[TAoE]", "TAoE")]
        [TestCase("Game - King of Titles (2021) S01 (1080p HMAX Webrip x265 10bit AC3 5 1 - bccornfo) [TAoE]", "TAoE")]
        [TestCase("Welcome to Game (2022) S04 (1080p AMZN Webrip x265 10bit EAC3 5 1 - DNU)[TAoE]", "TAoE")]
        [TestCase("Game Who (2005) S01 (1080p BDRip x265 10bit AC3 5 1 - DrainedDay)[TAoE]", "TAoE")]
        [TestCase("Game Down (2019) (1080p AMZN Webrip x265 10bit EAC3 5 1 - DUHiT)[TAoE]", "TAoE")]
        [TestCase("Game (2016) S09 (1080p CRAV Webrip x265 10bit EAC3 5 1 - Erie) [TAoE]", "TAoE")]
        [TestCase("Common Game Effects (2025) S01 (1080p AMZN Webrip x265 10bit EAC3 2 0 - Frys) [TAoE]", "TAoE")]
        [TestCase("Murderbot (2025) S01 (2160p HDR10 DV Hybrid ATVP Webrip x265 10bit EAC3 5 1 Atmos - Goki)[TAoE]", "TAoE")]
        [TestCase("Game In Real Life (2019) S01 REPACK (1080p DSNP Webrip x265 10bit AAC 2 0 - HxD)[TAoE]", "TAoE")]
        [TestCase("Game Discovery (2017) S02 (1080p BDRip x265 10bit DTS-HD MA 5 1 - jb2049) [TAoE]", "TAoE")]
        [TestCase("Game (2021) S03 (1080p DS4K NF Webrip x265 10bit EAC3 5 1 Atmos English - JBENT)[TAoE]", "TAoE")]
        [TestCase("SuSeriespergirl (2015) S04 (1080p BDRip x265 10bit AC3 5 1 - Nostradamus)[TAoE]", "TAoE")]
        [TestCase("Game (2019) S02 (4Kto1080p ATVP Webrip x265 10bit AC3 5 1 - r0b0t) [TAoE]", "TAoE")]
        [TestCase("v (1970) S01 (2160p AIUS HDR10 DV Hybrid BDRip x265 10bit DTS-HD MA 5 1 - Species180) [TAoE]", "TAoE")]
        [TestCase("Game (2024) S02 (1080p ATVP Webrip x265 10bit EAC3 5 1 - TheSickle)[TAoE]", "TAoE")]
        [TestCase("Game (2016) S05 Part 02 (1080p NF Webrip x265 10bit EAC3 5 1 - xtrem3x) [TAoE]", "TAoE")]
        [TestCase("Game (2013) S01 (1080p BDRip x265 10bit DTS-HD MA 5 1 - WEM)[TAoE]", "TAoE")]
        [TestCase("The.Game.1989.S00E65.1080p.DSNP.Webrip.x265.10bit.EAC3.5.1.Goki.TAoE", "TAoE")]
        [TestCase("Game (2008) S05E01 (1080p BDRip DDP2.0 x265) - Vialle", "Vialle")]
        [TestCase("The Game (2023) S03E01 (1080p DS4K NF Webrip DV HDR DDP5.1 x265) - Vialle.mkv", "Vialle")]
        [TestCase("The Show (2020) - S02E03 - Fighting His Game(1080p ATVP WEB-DL x265 t3nzinFAKE)", null)]
        [TestCase("The Game (2023) S03E01 (1080p DS4K NF Webrip DV HDR DDP5.1 x265) - VialleFAKE.mkv", null)]
        public void should_parse_exception_release_group(string title, string expected)
        {
            Parser.ReleaseGroupParser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [Test]
        public void should_not_include_extension_in_release_group()
        {
            const string path = @"C:\Test\Doctor.Game.2005.s01e01.internal.bdrip.x264-archivist.mkv";

            Parser.Parser.ParsePath(path).ReleaseGroup.Should().Be("archivist");
        }

        [TestCase("Game.Title.S02E04.720p.WEBRip.x264-SKGTV English", "SKGTV")]
        [TestCase("Game.Title.S02E04.720p.WEBRip.x264-SKGTV_English", "SKGTV")]
        [TestCase("Game.Title.S02E04.720p.WEBRip.x264-SKGTV.English", "SKGTV")]

        // [TestCase("", "")]
        public void should_not_include_language_in_release_group(string title, string expected)
        {
            Parser.ReleaseGroupParser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("Game.Title.S02E04.720p.WEB-DL.AAC2.0.H.264-EVL-RP", "EVL")]
        [TestCase("Game.Title.S02E04.720p.WEB-DL.AAC2.0.H.264-EVL-RP-RP", "EVL")]
        [TestCase("Game.Title.S02E04.720p.WEB-DL.AAC2.0.H.264-EVL-Obfuscated", "EVL")]
        [TestCase("Game.Title.S04E04.720p.BluRay.x264-xHD-NZBgeek", "xHD")]
        [TestCase("Game.Title.S05E11.720p.HDTV.X264-DIMENSION-NZBgeek", "DIMENSION")]
        [TestCase("Game.Title.S04E04.720p.BluRay.x264-xHD-1", "xHD")]
        [TestCase("Game.Title.S05E11.720p.HDTV.X264-DIMENSION-1", "DIMENSION")]
        [TestCase("game.title.s40e11.kevin.hart_sia.720p.hdtv.x264-w4f-sample.mkv", "w4f")]
        [TestCase("The.Game.2017.S05E02.1080p.WEB-DL.DD5.1.H264-EVL-Scrambled", "EVL")]
        [TestCase("Game.S01E08.Haunted.Hayride.720p.AMZN.WEBRip.DDP5.1.x264-NTb-postbot", "NTb")]
        [TestCase("Game.S01E08.Haunted.Hayride.720p.AMZN.WEBRip.DDP5.1.x264-NTb-xpost", "NTb")]
        [TestCase("Game.Title.S08E05.The.Forgotten.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-Rakuv", "NTb")]
        [TestCase("The.Game.S30E01.Devs.Not.Dead.1080p.AMZN.WEB-DL.DDP5.1.H264-QOQ-Rakuv02", "QOQ")]
        [TestCase("Lie.To.Developers.S01E13.720p.BluRay.x264-SiNNERS-Rakuvfinhel", "SiNNERS")]
        [TestCase("Who.is.Playarr.S01E01.INTERNAL.720p.HDTV.x264-aAF-RakuvUS-Obfuscated", "aAF")]
        [TestCase("Deadly.Development.S01E10.Sink.With.Code.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTG-WhiteRev", "NTG")]
        [TestCase("The.Playarr.Game.S09E12.Developers.REPACK.1080p.AMZN.WEB-DL.DD.5.1.H.264-CasStudio-BUYMORE", "CasStudio")]
        [TestCase("2.Tired.Developers.S02E24.1080p.AMZN.WEBRip.DD5.1.x264-CasStudio-AsRequested", "CasStudio")]
        [TestCase("Game.S04E11.Lines.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-AlternativeToRequested", "NTb")]
        [TestCase("Game.S16E04.Third.Wheel.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-GEROV", "NTb")]
        [TestCase("Game.and.Title.S10E06.Dev.n.Play.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-Z0iDS3N", "NTb")]
        [TestCase("Absolute.Game.S02E06.The.House.of.Playarr.DVDRip.x264-MaG-Chamele0n", "MaG")]
        [TestCase("The.Game.Title.S08E08.1080p.BluRay.x264-ROVERS-4P", "ROVERS")]
        [TestCase("Game.Title.S01E02.720p.BluRay.X264-REWARD-4Planet", "REWARD")]
        [TestCase("Game.S01E01.Rites.of.Passage.1080p.BluRay.x264-DON-AlteZachen", "DON")]
        [TestCase("Game.Title.S04E06.Rom.Name.720p.WEB-DL.DD5.1.H.264-HarrHD-RePACKPOST", "HarrHD")]
        public void should_not_include_repost_in_release_group(string title, string expected)
        {
            Parser.ReleaseGroupParser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("[FFF] Game Title!! - S01E11 - Someday, With Playarr", "FFF")]
        [TestCase("[HorribleSubs] Game Title!! - S01E12 - Playarr Going Well!!", "HorribleSubs")]
        [TestCase("[Anime-Koi] Game Title - S01E06 - Guys From Playarr", "Anime-Koi")]
        [TestCase("[Anime-Koi] Game Title - S01E07 - A High-Grade Playarr", "Anime-Koi")]
        [TestCase("[Anime-Koi] Game Title 2 - 01 [h264-720p][28D54E2C]", "Anime-Koi")]

        // [TestCase("Tokyo.Ghoul.02x01.013.HDTV-720p-Anime-Koi", "Anime-Koi")]
        // [TestCase("", "")]
        public void should_parse_anime_release_groups(string title, string expected)
        {
            Parser.ReleaseGroupParser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("Terrible.Anime.Title.001.DBOX.480p.x264-iKaos [v3] [6AFFEF6B]")]
        public void should_not_parse_anime_hash_as_release_group(string title)
        {
            Parser.ReleaseGroupParser.ParseReleaseGroup(title).Should().BeNull();
        }

        [TestCase("Some TV (1979) - S02E10 - Hart-Shaped Murder [SDTV][AAC 2.0][x264]")]
        [TestCase("Some TV (1979) - S02E10 - Hart-Shaped Murder [HDTV-480p][AAC 2.0][x264]")]
        [TestCase("Some TV (1979) - S02E10 - Hart-Shaped Murder [480p-HDTV][AAC 2.0][x264]")]
        public void should_not_parse_episode_title_as_release_group(string title)
        {
            Parser.ReleaseGroupParser.ParseReleaseGroup(title).Should().BeNull();
        }
    }
}
