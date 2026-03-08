using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.CustomFormats;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Organizer;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class FileNameBuilderFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _episode1;
        private RomFile _romFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episode1 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _romFile = new RomFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "PlayarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        private void GivenProper()
        {
            _romFile.Quality.Revision.Version = 2;
        }

        private void GivenReal()
        {
            _romFile.Quality.Revision.Real = 1;
        }

        [Test]
        public void should_replace_Series_space_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_replace_Series_underscore_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series_Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South_Park");
        }

        [Test]
        public void should_replace_Series_dot_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park");
        }

        [Test]
        public void should_replace_Series_dash_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Game-Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South-Park");
        }

        [Test]
        public void should_replace_SERIES_TITLE_with_all_caps()
        {
            _namingConfig.StandardEpisodeFormat = "{GAME TITLE}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("SOUTH PARK");
        }

        [Test]
        public void should_replace_SERIES_TITLE_with_random_casing_should_keep_original_casing()
        {
            _namingConfig.StandardEpisodeFormat = "{sErIES-tItLE}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(_series.Title.Replace(' ', '-'));
        }

        [Test]
        public void should_replace_series_title_with_all_lower_case()
        {
            _namingConfig.StandardEpisodeFormat = "{game title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("south park");
        }

        [Test]
        public void should_cleanup_Series_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.CleanTitle}";
            _series.Title = "South Park (1997)";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.1997");
        }

        [Test]
        public void should_replace_episode_title()
        {
            _namingConfig.StandardEpisodeFormat = "{Rom Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("City Sushi");
        }

        [Test]
        public void should_replace_episode_title_if_pattern_has_random_casing()
        {
            _namingConfig.StandardEpisodeFormat = "{ePisOde-TitLe}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("City-Sushi");
        }

        [Test]
        public void should_replace_season_number_with_single_digit()
        {
            _episode1.PlatformNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{platform}x{rom}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("1x6");
        }

        [Test]
        public void should_replace_season00_number_with_two_digits()
        {
            _episode1.PlatformNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{platform:00}x{rom}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("01x6");
        }

        [Test]
        public void should_replace_episode_number_with_single_digit()
        {
            _episode1.PlatformNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{platform}x{rom}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("1x6");
        }

        [Test]
        public void should_replace_episode00_number_with_two_digits()
        {
            _episode1.PlatformNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{platform}x{rom:00}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("1x06");
        }

        [Test]
        public void should_replace_quality_title()
        {
            _namingConfig.StandardEpisodeFormat = "{Quality Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("HDTV-720p");
        }

        [Test]
        public void should_replace_quality_proper_with_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Quality Proper}";
            GivenProper();

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("Proper");
        }

        [Test]
        public void should_replace_quality_real_with_real()
        {
            _namingConfig.StandardEpisodeFormat = "{Quality Real}";
            GivenReal();

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("REAL");
        }

        [Test]
        public void should_replace_all_contents_in_pattern()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} [{Quality Title}]";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 - City Sushi [HDTV-720p]");
        }

        [TestCase("Some Escaped {{ String", "Some Escaped { String")]
        [TestCase("Some Escaped }} String", "Some Escaped } String")]
        [TestCase("Some Escaped {{Game Title}} String", "Some Escaped {Game Title} String")]
        [TestCase("Some Escaped {{{Game Title}}} String", "Some Escaped {South Park} String")]
        public void should_escape_token_in_format(string format, string expected)
        {
            _namingConfig.StandardEpisodeFormat = format;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(expected);
        }

        [Test]
        public void should_escape_token_in_title()
        {
            _namingConfig.StandardEpisodeFormat = "Some Unescaped {Game Title} String";
            _series.Title = "My {Quality Full} Title";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("Some Unescaped My {Quality Full} Title String");
        }

        [Test]
        public void use_file_name_when_sceneName_is_null()
        {
            _namingConfig.RenameEpisodes = false;
            _romFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_romFile.RelativePath));
        }

        [Test]
        public void use_path_when_sceneName_and_relative_path_are_null()
        {
            _namingConfig.RenameEpisodes = false;
            _romFile.RelativePath = null;
            _romFile.Path = @"C:\Test\Unsorted\Game - S01E01 - Test".AsOsAgnostic();

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_romFile.Path));
        }

        [Test]
        public void use_file_name_when_sceneName_is_not_null()
        {
            _namingConfig.RenameEpisodes = false;
            _romFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _romFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("30.Rock.S01E01.xvid-LOL");
        }

        [Test]
        public void should_replace_illegal_characters_when_renaming_is_disabled()
        {
            _namingConfig.RenameEpisodes = false;
            _namingConfig.ReplaceIllegalCharacters = true;
            _namingConfig.ColonReplacementFormat = ColonReplacementFormat.Smart;

            _romFile.SceneName = "30.Rock.S01E01.xvid:LOL";
            _romFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                .Should().Be("30.Rock.S01E01.xvid-LOL");
        }

        [Test]
        public void should_use_airDate_if_series_isDaily_and_not_a_special()
        {
            _namingConfig.DailyEpisodeFormat = "{Game Title} - {air-date} - {Rom Title}";

            _series.Title = "The Daily Show with Jon Stewart";
            _series.SeriesType = GameTypes.Daily;

            _episode1.AirDate = "2012-12-13";
            _episode1.Title = "Kristen Stewart";
            _episode1.PlatformNumber = 1;
            _episode1.EpisodeNumber = 5;

            _romFile.PlatformNumber = 1;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("The Daily Show with Jon Stewart - 2012-12-13 - Kristen Stewart");
        }

        [Test]
        public void should_use_standard_if_series_isDaily_special()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";

            _series.Title = "The Daily Show with Jon Stewart";
            _series.SeriesType = GameTypes.Daily;

            _episode1.AirDate = "2012-12-13";
            _episode1.Title = "Kristen Stewart";
            _episode1.PlatformNumber = 0;
            _episode1.EpisodeNumber = 5;

            _romFile.PlatformNumber = 0;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("The Daily Show with Jon Stewart - S00E05 - Kristen Stewart");
        }

        [Test]
        public void should_set_airdate_to_unknown_if_not_available()
        {
            _namingConfig.DailyEpisodeFormat = "{Game Title} - {Air-Date} - {Rom Title}";

            _series.Title = "The Daily Show with Jon Stewart";
            _series.SeriesType = GameTypes.Daily;

            _episode1.AirDate = null;
            _episode1.Title = "Kristen Stewart";
            _episode1.PlatformNumber = 1;
            _episode1.EpisodeNumber = 5;

            _romFile.PlatformNumber = 1;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("The Daily Show with Jon Stewart - Unknown - Kristen Stewart");
        }

        [Test]
        public void should_not_clean_episode_title_if_there_is_only_one()
        {
            var title = "City Sushi (1)";
            _episode1.Title = title;

            _namingConfig.StandardEpisodeFormat = "{Rom Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(title);
        }

        [Test]
        public void should_should_replace_release_group()
        {
            _namingConfig.StandardEpisodeFormat = "{Release Group}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(_romFile.ReleaseGroup);
        }

        [Test]
        public void should_be_able_to_use_original_title()
        {
            _series.Title = "30 Rock";
            _namingConfig.StandardEpisodeFormat = "{Game Title} - {Original Title}";

            _romFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _romFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("30 Rock - 30.Rock.S01E01.xvid-LOL");
        }

        [Test]
        public void should_trim_periods_from_end_of_episode_title()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Scene;

            var rom = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "Part 1.")
                            .With(e => e.PlatformNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Rom> { rom }, new Game { Title = "30 Rock" }, _romFile)
                   .Should().Be("30 Rock - S06E06 - Part 1");
        }

        [Test]
        public void should_trim_question_marks_from_end_of_episode_title()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Scene;

            var rom = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "Part 1?")
                            .With(e => e.PlatformNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Rom> { rom }, new Game { Title = "30 Rock" }, _romFile)
                   .Should().Be("30 Rock - S06E06 - Part 1");
        }

        [Test]
        public void should_replace_double_period_with_single_period()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}";

            var rom = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "Part 1")
                            .With(e => e.PlatformNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Rom> { rom }, new Game { Title = "Chicago P.D." }, _romFile)
                   .Should().Be("Chicago.P.D.S06E06.Part.1");
        }

        [Test]
        public void should_replace_triple_period_with_single_period()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}";

            var rom = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "Part 1")
                            .With(e => e.PlatformNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Rom> { rom }, new Game { Title = "Chicago P.D.." }, _romFile)
                   .Should().Be("Chicago.P.D.S06E06.Part.1");
        }

        [Test]
        public void should_not_replace_absolute_numbering_when_series_is_not_anime()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.S{platform:00}E{rom:00}.{absolute:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.S15E06.City.Sushi");
        }

        [Test]
        public void should_replace_standard_and_absolute_numbering_when_series_is_anime()
        {
            _series.SeriesType = GameTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Game.Title}.S{platform:00}E{rom:00}.{absolute:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.S15E06.100.City.Sushi");
        }

        [Test]
        public void should_replace_standard_numbering_when_series_is_anime()
        {
            _series.SeriesType = GameTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.S15E06.City.Sushi");
        }

        [Test]
        public void should_replace_absolute_numbering_when_series_is_anime()
        {
            _series.SeriesType = GameTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Game.Title}.{absolute:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.100.City.Sushi");
        }

        [Test]
        public void should_replace_duplicate_numbering_individually()
        {
            _series.SeriesType = GameTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Game.Title}.{platform}x{rom:00}.{absolute:000}\\{Game.Title}.S{platform:00}E{rom:00}.{absolute:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.15x06.100\\South.Park.S15E06.100.City.Sushi".AsOsAgnostic());
        }

        [Test]
        public void should_replace_individual_season_episode_tokens()
        {
            _series.SeriesType = GameTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Game Title} Platform {platform:0000} Rom {rom:0000}\\{Game.Title}.S{platform:00}E{rom:00}.{absolute:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park Platform 0015 Rom 0006\\South.Park.S15E06.100.City.Sushi".AsOsAgnostic());
        }

        [Test]
        public void should_use_standard_naming_when_anime_episode_has_no_absolute_number()
        {
            _series.SeriesType = GameTypes.Anime;
            _episode1.AbsoluteEpisodeNumber = null;

            _namingConfig.StandardEpisodeFormat = "{Game Title} - {platform:0}x{rom:00} - {Rom Title}";
            _namingConfig.AnimeEpisodeFormat = "{Game Title} - {absolute:000} - {Rom Title}";

            Subject.BuildFileName(new List<Rom> { _episode1, }, _series, _romFile)
                   .Should().Be("South Park - 15x06 - City Sushi");
        }

        [Test]
        public void should_include_affixes_if_value_not_empty()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.S{platform:00}E{rom:00}{_Episode.Title_}{Quality.Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.S15E06_City.Sushi_HDTV-720p");
        }

        [Test]
        public void should_not_include_affixes_if_value_empty()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.S{platform:00}E{rom:00}{_Episode.Title_}";

            _episode1.Title = "";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.S15E06");
        }

        [Test]
        public void should_format_mediainfo_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}.{MEDIAINFO.FULL}";

            _romFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioStreams =
                [
                    new MediaInfoAudioStreamModel
                    {
                        Format = "dts",
                        Language = "eng",
                    },
                    new MediaInfoAudioStreamModel
                    {
                        Format = "dts",
                        Language = "spa",
                    },
                ],
                SubtitleStreams =
                [
                    new MediaInfoSubtitleStreamModel { Language = "eng" },
                    new MediaInfoSubtitleStreamModel { Language = "spa" },
                    new MediaInfoSubtitleStreamModel { Language = "ita" },
                ],
            };

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.S15E06.City.Sushi.H264.DTS[EN+ES].[EN+ES+IT]");
        }

        [TestCase("nob", "NB")]
        [TestCase("swe", "SV")]
        [TestCase("zho", "ZH")]
        [TestCase("chi", "ZH")]
        [TestCase("fre", "FR")]
        [TestCase("rum", "RO")]
        [TestCase("per", "FA")]
        [TestCase("ger", "DE")]
        [TestCase("gsw", "DE")]
        [TestCase("cze", "CS")]
        [TestCase("ice", "IS")]
        [TestCase("dut", "NL")]
        [TestCase("nor", "NO")]
        [TestCase("geo", "KA")]
        [TestCase("kat", "KA")]
        public void should_format_languagecodes_properly(string language, string code)
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}.{MEDIAINFO.FULL}";

            _romFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioStreams =
                [
                    new MediaInfoAudioStreamModel
                    {
                        Format = "dts",
                        Channels = 6,
                        Language = "eng",
                    },
                ],
                SubtitleStreams =
                [
                    new MediaInfoSubtitleStreamModel { Language = language },
                ],
                SchemaRevision = 3
            };

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be($"South.Park.S15E06.City.Sushi.H264.DTS.[{code}]");
        }

        [Test]
        public void should_exclude_english_in_mediainfo_audio_language()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}.{MEDIAINFO.FULL}";

            _romFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioStreams =
                [
                    new MediaInfoAudioStreamModel
                    {
                        Format = "dts",
                        Language = "eng",
                    },
                ],
                SubtitleStreams =
                [
                    new MediaInfoSubtitleStreamModel { Language = "eng" },
                    new MediaInfoSubtitleStreamModel { Language = "spa" },
                    new MediaInfoSubtitleStreamModel { Language = "ita" },
                ],
            };

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.S15E06.City.Sushi.H264.DTS.[EN+ES+IT]");
        }

        [Ignore("not currently supported")]
        [Test]
        public void should_remove_duplicate_non_word_characters()
        {
            _series.Title = "Venture Bros.";
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.{platform}x{rom:00}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("Venture.Bros.15x06");
        }

        [Test]
        public void should_use_existing_filename_when_scene_name_is_not_available()
        {
            _namingConfig.RenameEpisodes = true;
            _namingConfig.StandardEpisodeFormat = "{Original Title}";

            _romFile.SceneName = null;
            _romFile.RelativePath = "existing.file.mkv";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_romFile.RelativePath));
        }

        [Test]
        public void should_be_able_to_use_only_original_title()
        {
            _series.Title = "30 Rock";
            _namingConfig.StandardEpisodeFormat = "{Original Title}";

            _romFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _romFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("30.Rock.S01E01.xvid-LOL");
        }

        [Test]
        public void should_allow_period_between_season_and_episode()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.S{platform:00}.E{rom:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South.Park.S15.E06.City.Sushi");
        }

        [Test]
        public void should_allow_space_between_season_and_episode()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00} E{rom:00} - {Rom Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park - S15 E06 - City Sushi");
        }

        [Test]
        public void should_replace_quality_proper_with_v2_for_anime_v2()
        {
            _series.SeriesType = GameTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Quality Proper}";

            GivenProper();

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("v2");
        }

        [Test]
        public void should_not_include_quality_proper_when_release_is_not_a_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Quality Title} {Quality Proper}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("HDTV-720p");
        }

        [Test]
        public void should_wrap_proper_in_square_brackets()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} [{Quality Title}] {[Quality Proper]}";

            GivenProper();

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p] [Proper]");
        }

        [Test]
        public void should_not_wrap_proper_in_square_brackets_when_not_a_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} [{Quality Title}] {[Quality Proper]}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_only_when_not_a_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} [{Quality Full}]";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_and_proper_only_when_a_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} [{Quality Full}]";

            GivenProper();

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p Proper]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_and_real_when_a_real()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} [{Quality Full}]";
            GivenReal();

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p REAL]");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_end_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardEpisodeFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}", separator);

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("HDTV-720p");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_middle_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardEpisodeFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}{0}{{Rom{0}Title}}", separator);

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(string.Format("HDTV-720p{0}City{0}Sushi", separator));
        }

        [Test]
        public void should_not_require_a_separator_between_tokens()
        {
            _series.SeriesType = GameTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "[{Release Group}]{Game.CleanTitle}.{absolute:000}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("[PlayarrTest]South.Park.100");
        }

        [Test]
        public void should_be_able_to_use_original_filename_only()
        {
            _series.Title = "30 Rock";
            _namingConfig.StandardEpisodeFormat = "{Original Filename}";

            _romFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _romFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("30 Rock - S01E01 - Test");
        }

        [Test]
        public void should_use_Playarr_as_release_group_when_not_available()
        {
            _romFile.ReleaseGroup = null;
            _namingConfig.StandardEpisodeFormat = "{Release Group}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("Playarr");
        }

        [TestCase("{Rom Title}{-Release Group}", "City Sushi")]
        [TestCase("{Rom Title}{ Release Group}", "City Sushi")]
        [TestCase("{Rom Title}{ [Release Group]}", "City Sushi")]
        public void should_not_use_Playarr_as_release_group_if_pattern_has_separator(string pattern, string expectedFileName)
        {
            _romFile.ReleaseGroup = null;
            _namingConfig.StandardEpisodeFormat = pattern;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(expectedFileName);
        }

        [TestCase("0SEC")]
        [TestCase("2HD")]
        [TestCase("IMMERSE")]
        public void should_use_existing_casing_for_release_group(string releaseGroup)
        {
            _romFile.ReleaseGroup = releaseGroup;
            _namingConfig.StandardEpisodeFormat = "{Release Group}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(releaseGroup);
        }

        [TestCase("en-US")]
        [TestCase("fr-FR")]
        [TestCase("az")]
        [TestCase("tr-TR")]
        public void should_replace_all_tokens_for_different_cultures(string culture)
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

                _romFile.ReleaseGroup = null;

                GivenMediaInfoModel(audioLanguages: "eng/deu");

                _namingConfig.StandardEpisodeFormat = "{MediaInfo AudioLanguages}";

                Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                       .Should().Be("[EN+DE]");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }
        }

        [TestCase("eng", "")]
        [TestCase("eng/deu", "[EN+DE]")]
        public void should_format_audio_languages(string audioLanguages, string expected)
        {
            _romFile.ReleaseGroup = null;

            GivenMediaInfoModel(audioLanguages: audioLanguages);

            _namingConfig.StandardEpisodeFormat = "{MediaInfo AudioLanguages}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(expected);
        }

        [TestCase("eng", "[EN]")]
        [TestCase("eng/deu", "[EN+DE]")]
        public void should_format_audio_languages_all(string audioLanguages, string expected)
        {
            _romFile.ReleaseGroup = null;

            GivenMediaInfoModel(audioLanguages: audioLanguages);

            _namingConfig.StandardEpisodeFormat = "{MediaInfo AudioLanguagesAll}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(expected);
        }

        [TestCase("eng/deu", "", "[EN+DE]")]
        [TestCase("eng/nld/deu", "", "[EN+NL+DE]")]
        [TestCase("eng/deu", ":DE", "[DE]")]
        [TestCase("eng/nld/deu", ":EN+NL", "[EN+NL]")]
        [TestCase("eng/nld/deu", ":NL+EN", "[NL+EN]")]
        [TestCase("eng/nld/deu", ":-NL", "[EN+DE]")]
        [TestCase("eng/nld/deu", ":DE+", "[DE+-]")]
        [TestCase("eng/nld/deu", ":DE+NO.", "[DE].")]
        [TestCase("eng/nld/deu", ":-EN-", "[NL+DE]-")]
        public void should_format_subtitle_languages_all(string subtitleLanguages, string format, string expected)
        {
            _romFile.ReleaseGroup = null;

            GivenMediaInfoModel(subtitles: subtitleLanguages);

            _namingConfig.StandardEpisodeFormat = "{MediaInfo SubtitleLanguages" + format + "}End";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(expected + "End");
        }

        [TestCase(HdrFormat.None, "South.Park.S15E06.City.Sushi")]
        [TestCase(HdrFormat.Hlg10, "South.Park.S15E06.City.Sushi.HDR")]
        [TestCase(HdrFormat.Hdr10, "South.Park.S15E06.City.Sushi.HDR")]
        public void should_include_hdr_for_mediainfo_videodynamicrange_with_valid_properties(HdrFormat hdrFormat, string expectedName)
        {
            _namingConfig.StandardEpisodeFormat =
                "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(hdrFormat: hdrFormat);

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                .Should().Be(expectedName);
        }

        [Test]
        public void should_update_media_info_if_token_configured_and_revision_is_old()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 3);

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_romFile, _series), Times.Once());
        }

        [Test]
        public void should_not_update_media_info_if_no_series_path_available()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 3);
            _series.Path = null;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_romFile, _series), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_token_not_configured_and_revision_is_old()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}";

            GivenMediaInfoModel(schemaRevision: 3);

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_romFile, _series), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_token_configured_and_revision_is_current()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 5);

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_romFile, _series), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_token_configured_and_revision_is_newer()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 8);

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_romFile, _series), Times.Never());
        }

        [TestCase("{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}.{MediaInfo VideoDynamicRange}")]
        [TestCase("{Game.Title}.S{platform:00}E{rom:00}.{Rom.Title}.{MediaInfo.VideoDynamicRange}")]
        public void should_use_updated_media_info_if_token_configured_and_revision_is_old(string standardEpisodeFormat)
        {
            _namingConfig.StandardEpisodeFormat = standardEpisodeFormat;

            GivenMediaInfoModel(schemaRevision: 3);

            Mocker.GetMock<IUpdateMediaInfo>()
                .Setup(u => u.Update(_romFile, _series))
                .Callback((RomFile e, Game s) => e.MediaInfo = new MediaInfoModel
                {
                    VideoFormat = "AVC",
                    AudioStreams =
                    [
                        new MediaInfoAudioStreamModel
                        {
                            Format = "dts",
                            Channels = 6,
                            Language = "eng",
                        },
                    ],
                    SubtitleStreams =
                    [
                        new MediaInfoSubtitleStreamModel { Language = "eng" },
                        new MediaInfoSubtitleStreamModel { Language = "esp" },
                        new MediaInfoSubtitleStreamModel { Language = "ita" },
                    ],
                    VideoBitDepth = 10,
                    VideoColourPrimaries = "bt2020",
                    VideoTransferCharacteristics = "PQ",
                    VideoHdrFormat = HdrFormat.Pq10,
                    SchemaRevision = 5
                });

            var result = Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile);

            result.Should().EndWith("HDR");
        }

        [Test]
        public void should_replace_release_hash_with_stored_hash()
        {
            _namingConfig.StandardEpisodeFormat = "{Release Hash}";

            _romFile.ReleaseHash = "ABCDEFGH";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("ABCDEFGH");
        }

        [Test]
        public void should_replace_null_release_hash_with_empty_string()
        {
            _namingConfig.StandardEpisodeFormat = "{Release Hash}";

            _romFile.ReleaseHash = null;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be(string.Empty);
        }

        [Test]
        public void should_maintain_ellipsis_in_naming_format()
        {
            _namingConfig.StandardEpisodeFormat = "{Game.Title}.S{platform:00}.E{rom:00}...{Rom.CleanTitle}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                .Should().Be("South.Park.S15.E06...City.Sushi");
        }

        private void GivenMediaInfoModel(string videoCodec = "h264",
                                         string audioCodec = "dts",
                                         int audioChannels = 6,
                                         int videoBitDepth = 8,
                                         HdrFormat hdrFormat = HdrFormat.None,
                                         string audioLanguages = "eng",
                                         string subtitles = "eng/spa/ita",
                                         int schemaRevision = 5)
        {
            _romFile.MediaInfo = new MediaInfoModel
            {
                VideoFormat = videoCodec,
                AudioStreams = audioLanguages.Split('/')
                    .Select(language => new MediaInfoAudioStreamModel
                    {
                        Format = audioCodec,
                        Channels = audioChannels,
                        Language = language,
                    }).ToList(),
                SubtitleStreams = subtitles.Split('/')
                    .Select(language => new MediaInfoSubtitleStreamModel
                    {
                        Language = language
                    }).ToList(),
                VideoBitDepth = videoBitDepth,
                VideoHdrFormat = hdrFormat,
                SchemaRevision = schemaRevision
            };
        }
    }
}
