using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Languages;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetEpisodesFixture : TestBase<ParsingService>
    {
        private Game _series;
        private List<Rom> _episodes;
        private ParsedRomInfo _parsedRomInfo;
        private SingleEpisodeSearchCriteria _singleEpisodeSearchCriteria;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                .With(s => s.Title = "30 Stone")
                .With(s => s.CleanTitle = "stone")
                .Build();

            _episodes = Builder<Rom>.CreateListOfSize(1)
                                        .All()
                                        .With(e => e.AirDate = DateTime.Today.ToString(Rom.AIR_DATE_FORMAT))
                                        .Build()
                                        .ToList();

            _parsedRomInfo = new ParsedRomInfo
            {
                GameTitle = _series.Title,
                SeasonNumber = 1,
                RomNumbers = new[] { 1 },
                AbsoluteRomNumbers = Array.Empty<int>(),
                Languages = new List<Language> { Language.English }
            };

            _singleEpisodeSearchCriteria = new SingleEpisodeSearchCriteria
            {
                Game = _series,
                EpisodeNumber = _episodes.First().EpisodeNumber,
                SeasonNumber = _episodes.First().SeasonNumber,
                Roms = _episodes
            };

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.FindByTitle(It.IsAny<string>()))
                  .Returns(_series);
        }

        private void GivenDailySeries()
        {
            _series.SeriesType = GameTypes.Daily;
        }

        private void GivenDailyParseResult()
        {
            _parsedRomInfo.AirDate = DateTime.Today.ToString(Rom.AIR_DATE_FORMAT);
        }

        private void GivenSceneNumberingSeries()
        {
            _series.UseSceneNumbering = true;
        }

        private void GivenAbsoluteNumberingSeries()
        {
            _parsedRomInfo.AbsoluteRomNumbers = new[] { 1 };
        }

        private void GivenFullSeason()
        {
            _parsedRomInfo.FullSeason = true;
            _parsedRomInfo.RomNumbers = Array.Empty<int>();
        }

        [Test]
        public void should_get_daily_episode_episode_when_search_criteria_is_null()
        {
            GivenDailySeries();
            GivenDailyParseResult();

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>(), null), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_episode_when_it_matches_daily()
        {
            GivenDailySeries();
            GivenDailyParseResult();

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>(), null), Times.Never());
        }

        [Test]
        public void should_fallback_to_daily_episode_lookup_when_search_criteria_episode_doesnt_match()
        {
            GivenDailySeries();
            _parsedRomInfo.AirDate = DateTime.Today.AddDays(-5).ToString(Rom.AIR_DATE_FORMAT);

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>(), null), Times.Once());
        }

        [Test]
        public void should_get_daily_episode_episode_should_lookup_including_daily_part()
        {
            GivenDailySeries();
            GivenDailyParseResult();
            _parsedRomInfo.DailyPart = 1;

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>(), 1), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_episode_when_it_matches_absolute()
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(new List<Rom>());

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>(), null), Times.Never());
        }

        [Test]
        public void should_use_scene_numbering_when_series_uses_scene_numbering()
        {
            GivenSceneNumberingSeries();

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_match_search_criteria_by_scene_numbering()
        {
            GivenSceneNumberingSeries();

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_fallback_to_findEpisode_when_search_criteria_match_fails_for_scene_numbering()
        {
            GivenSceneNumberingSeries();
            _episodes.First().SceneEpisodeNumber = 10;

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_find_episode()
        {
            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_match_episode_with_search_criteria()
        {
            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_fallback_to_findEpisode_when_search_criteria_match_fails()
        {
            _episodes.First().EpisodeNumber = 10;

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_look_for_episode_in_season_zero_if_absolute_special()
        {
            GivenAbsoluteNumberingSeries();

            _parsedRomInfo.Special = true;

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), 0, It.IsAny<int>()), Times.Never());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), 0, It.IsAny<int>()), Times.Once());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_use_scene_numbering_when_scene_season_number_has_value(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetScenePlatformNumber(_parsedRomInfo.GameTitle, It.IsAny<string>()))
                  .Returns(platformNumber);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(new List<Rom>());

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_find_episode_by_season_and_scene_absolute_episode_number(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetScenePlatformNumber(_parsedRomInfo.GameTitle, It.IsAny<string>()))
                  .Returns(platformNumber);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(new List<Rom> { _episodes.First() });

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(2)]
        [TestCase(20)]
        public void should_find_episode_by_parsed_season_and_absolute_episode_number_when_season_number_is_2_or_higher(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();
            _parsedRomInfo.SeasonNumber = platformNumber;
            _parsedRomInfo.RomNumbers = Array.Empty<int>();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(new List<Rom> { _episodes.First() });

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(2)]
        [TestCase(20)]
        public void should_find_episode_by_parsed_season_and_absolute_episode_number_when_season_number_is_2_or_higher_and_scene_season_number_lookup_failed(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();
            _parsedRomInfo.SeasonNumber = platformNumber;
            _parsedRomInfo.RomNumbers = Array.Empty<int>();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(new List<Rom>());

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisode(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(_episodes.First());

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());
        }

        [TestCase(2)]
        [TestCase(20)]
        public void should_not_find_episode_by_parsed_season_and_absolute_episode_number_when_season_number_is_2_or_higher_and_a_episode_number_was_parsed(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();
            _parsedRomInfo.SeasonNumber = platformNumber;
            _parsedRomInfo.RomNumbers = new[] { 1 };

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(new List<Rom> { _episodes.First() });

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Never());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_return_episodes_when_scene_absolute_episode_number_returns_multiple_results(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetScenePlatformNumber(_parsedRomInfo.GameTitle, It.IsAny<string>()))
                  .Returns(platformNumber);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(Builder<Rom>.CreateListOfSize(5).Build().ToList());

            var result = Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            result.Should().HaveCount(5);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_find_episode_by_season_and_absolute_episode_number_when_scene_absolute_episode_number_returns_no_results(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetScenePlatformNumber(_parsedRomInfo.GameTitle, It.IsAny<string>()))
                  .Returns(platformNumber);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(new List<Rom>());

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_use_tvdb_season_number_when_available_and_a_scene_source()
        {
            const int tvdbPlatformNumber = 5;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(v => v.FindSceneMapping(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns<string, string, int>((s, r, sn) => new SceneMapping { SceneSeasonNumber = 1, SeasonNumber = tvdbPlatformNumber });

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(_series.Id, _parsedRomInfo.SeasonNumber, _parsedRomInfo.RomNumbers.First()), Times.Never());

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(_series.Id, tvdbPlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Once());
        }

        [Test]
        public void should_not_use_tvdb_season_number_when_available_for_a_different_season_and_a_scene_source()
        {
            const int tvdbPlatformNumber = 5;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(v => v.FindSceneMapping(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns<string, string, int>((s, r, sn) => new SceneMapping { SceneSeasonNumber = 101, SeasonNumber = tvdbPlatformNumber });

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(_series.Id, tvdbPlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Never());

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(_series.Id, _parsedRomInfo.SeasonNumber, _parsedRomInfo.RomNumbers.First()), Times.Once());
        }

        [Test]
        public void should_not_use_tvdb_season_when_not_a_scene_source()
        {
            const int tvdbPlatformNumber = 5;

            Subject.GetEpisodes(_parsedRomInfo, _series, false, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(_series.Id, tvdbPlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Never());

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(_series.Id, _parsedRomInfo.SeasonNumber, _parsedRomInfo.RomNumbers.First()), Times.Once());
        }

        [Test]
        public void should_not_use_tvdb_season_when_tvdb_season_number_is_less_than_zero()
        {
            const int tvdbPlatformNumber = -1;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.FindSceneMapping(_parsedRomInfo.GameTitle, It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(new SceneMapping { SeasonNumber = tvdbPlatformNumber, SceneSeasonNumber = _parsedRomInfo.SeasonNumber });

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(_series.Id, tvdbPlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Never());

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindEpisode(_series.Id, _parsedRomInfo.SeasonNumber, _parsedRomInfo.RomNumbers.First()), Times.Once());
        }

        [Test]
        public void should_lookup_full_season_by_season_number_if_series_does_not_use_scene_numbering()
        {
            GivenFullSeason();

            Mocker.GetMock<IRomService>()
                .Setup(s => s.GetEpisodesBySeason(_series.Id, _parsedRomInfo.SeasonNumber))
                .Returns(_episodes);

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetEpisodesBySceneSeason(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void should_lookup_full_season_by_scene_season_number_if_series_uses_scene_numbering()
        {
            GivenSceneNumberingSeries();
            GivenFullSeason();

            Mocker.GetMock<IRomService>()
                .Setup(s => s.GetEpisodesBySceneSeason(_series.Id, _parsedRomInfo.SeasonNumber))
                .Returns(_episodes);

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetEpisodesBySceneSeason(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void should_fallback_to_lookup_full_season_by_season_number_if_series_uses_scene_numbering_and_no_epsiodes_are_found_by_scene_season_number()
        {
            GivenSceneNumberingSeries();
            GivenFullSeason();

            Mocker.GetMock<IRomService>()
                .Setup(s => s.GetEpisodesBySceneSeason(_series.Id, _parsedRomInfo.SeasonNumber))
                .Returns(new List<Rom>());

            Mocker.GetMock<IRomService>()
                .Setup(s => s.GetEpisodesBySeason(_series.Id, _parsedRomInfo.SeasonNumber))
                .Returns(_episodes);

            Subject.GetEpisodes(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetEpisodesBySceneSeason(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void should_use_season_zero_when_looking_up_is_partial_special_episode_found_by_title()
        {
            _series.UseSceneNumbering = false;
            _parsedRomInfo.SeasonNumber = 1;
            _parsedRomInfo.RomNumbers = new int[] { 0 };
            _parsedRomInfo.ReleaseTitle = "Game.Title.S01E00.My.Special.Rom.1080p.AMZN.WEB-DL.DDP5.1.H264-TEPES";

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisodeByTitle(_series.TvdbId, 0, _parsedRomInfo.ReleaseTitle))
                  .Returns(
                      Builder<Rom>.CreateNew()
                                      .With(e => e.SeasonNumber = 0)
                                      .With(e => e.EpisodeNumber = 1)
                                      .Build());

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(_series.TvdbId, 0, 1), Times.Once());
        }

        [Test]
        public void should_use_original_parse_result_when_special_episode_lookup_by_title_fails()
        {
            _series.UseSceneNumbering = false;
            _parsedRomInfo.SeasonNumber = 1;
            _parsedRomInfo.RomNumbers = new int[] { 0 };
            _parsedRomInfo.ReleaseTitle = "Game.Title.S01E00.My.Special.Rom.1080p.AMZN.WEB-DL.DDP5.1.H264-TEPES";

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindEpisodeByTitle(_series.TvdbId, 0, _parsedRomInfo.ReleaseTitle))
                  .Returns((Rom)null);

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindEpisode(_series.TvdbId, _parsedRomInfo.SeasonNumber, _parsedRomInfo.RomNumbers.First()), Times.Once());
        }
    }
}
