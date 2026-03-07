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
    public class MapFixture : TestBase<ParsingService>
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
                GameTitleInfo = new GameTitleInfo(),
                SeasonNumber = 1,
                RomNumbers = new[] { 1 },
                Languages = new List<Language> { Language.English }
            };

            _singleEpisodeSearchCriteria = new SingleEpisodeSearchCriteria
            {
                Game = _series,
                EpisodeNumber = _episodes.First().EpisodeNumber,
                SeasonNumber = _episodes.First().SeasonNumber,
                Roms = _episodes
            };
        }

        private void GivenMatchByGameTitle()
        {
            Mocker.GetMock<IGameService>()
                  .Setup(s => s.FindByTitle(It.IsAny<string>()))
                  .Returns(_series);
        }

        private void GivenMatchByIgdbId()
        {
            Mocker.GetMock<IGameService>()
                  .Setup(s => s.FindByIgdbId(It.IsAny<int>()))
                  .Returns(_series);
        }

        private void GivenMatchByMobyGamesId()
        {
            Mocker.GetMock<IGameService>()
                  .Setup(s => s.FindByMobyGamesId(It.IsAny<int>()))
                  .Returns(_series);
        }

        private void GivenParseResultSeriesDoesntMatchSearchCriteria()
        {
            _parsedRomInfo.GameTitle = "Another Name";
        }

        [Test]
        public void should_lookup_series_by_name()
        {
            GivenMatchByGameTitle();

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_use_tvdbid_when_series_title_lookup_fails()
        {
            GivenMatchByIgdbId();

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByIgdbId(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_use_tvrageid_when_series_title_lookup_fails()
        {
            GivenMatchByMobyGamesId();

            Subject.Map(_parsedRomInfo, 0, _series.MobyGamesId, null);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByMobyGamesId(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_not_use_tvrageid_when_scene_naming_exception_exists()
        {
            GivenMatchByMobyGamesId();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(v => v.FindSceneMapping(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(new SceneMapping { TvdbId = 10 });

            var result = Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByMobyGamesId(It.IsAny<int>()), Times.Never());

            result.Game.Should().BeNull();
        }

        [Test]
        public void should_use_search_criteria_series_title()
        {
            GivenMatchByGameTitle();

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_FindByTitle_when_search_criteria_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedRomInfo, 10, 10, null, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_FindByTitle_using_year_when_FindByTitle_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            _parsedRomInfo.GameTitleInfo = new GameTitleInfo
            {
                Title = "Game Title 2017",
                TitleWithoutYear = "Game Title",
                Year = 2017
            };

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.FindByTitle(_parsedRomInfo.GameTitleInfo.TitleWithoutYear, _parsedRomInfo.GameTitleInfo.Year))
                  .Returns(_series);

            Subject.Map(_parsedRomInfo, 10, 10, null, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_FindByIgdbId_when_search_criteria_and_FindByTitle_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedRomInfo, 10, 10, null, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByIgdbId(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_FindByMobyGamesId_when_search_criteria_and_FindByTitle_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedRomInfo, 0, 10, null, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByMobyGamesId(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_not_FindByMobyGamesId_when_search_criteria_and_FindByTitle_matching_fails_and_tvdb_id_is_specified()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedRomInfo, 10, 10, null, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IGameService>()
                .Verify(v => v.FindByMobyGamesId(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_FindByImdbId_when_search_criteria_and_FindByTitle_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedRomInfo, 0, 0, "tt12345", _singleEpisodeSearchCriteria);

            Mocker.GetMock<IGameService>()
                .Verify(v => v.FindByImdbId(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_not_FindByImdbId_when_search_criteria_and_FindByTitle_matching_fails_and_tvdb_id_is_specified()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedRomInfo, 10, 10, "tt12345", _singleEpisodeSearchCriteria);

            Mocker.GetMock<IGameService>()
                .Verify(v => v.FindByImdbId(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_tvdbid_matching_when_alias_is_found()
        {
            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.FindIgdbId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(_series.TvdbId);

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_tvrageid_match_from_search_criteria_when_title_match_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_scene_season_number_from_xem_mapping_if_alias_matches_a_specific_season_number()
        {
            _parsedRomInfo.SeasonNumber = 1;

            var sceneMapping = new SceneMapping
            {
                Type = "XemService",
                SceneSeasonNumber = 2
            };

            Mocker.GetMock<ISceneMappingService>()
                .Setup(s => s.FindSceneMapping(_parsedRomInfo.GameTitle, _parsedRomInfo.ReleaseTitle, _parsedRomInfo.SeasonNumber))
                .Returns(sceneMapping);

            var result = Subject.Map(_parsedRomInfo, _series);

            result.MappedPlatformNumber.Should().Be(sceneMapping.SceneSeasonNumber);
        }

        [Test]
        public void should_not_use_scene_season_number_from_xem_mapping_if_alias_matches_a_specific_season_number_but_did_not_parse_season_1()
        {
            _parsedRomInfo.SeasonNumber = 2;

            var sceneMapping = new SceneMapping
            {
                Type = "XemService",
                SceneSeasonNumber = 2
            };

            Mocker.GetMock<ISceneMappingService>()
                .Setup(s => s.FindSceneMapping(_parsedRomInfo.GameTitle, _parsedRomInfo.ReleaseTitle, _parsedRomInfo.SeasonNumber))
                .Returns(sceneMapping);

            var result = Subject.Map(_parsedRomInfo, _series);

            result.MappedPlatformNumber.Should().Be(sceneMapping.SceneSeasonNumber);
        }

        [Test]
        public void should_use_tvdbid_matching_when_alias_without_year_is_found()
        {
            var alias = "Game Alias";

            _parsedRomInfo.GameTitle = $"{alias} {_series.Year}";
            _parsedRomInfo.GameTitleInfo.TitleWithoutYear = alias;
            _parsedRomInfo.GameTitleInfo.Year = _series.Year;

            Mocker.GetMock<ISceneMappingService>()
                .Setup(s => s.FindIgdbId(alias, It.IsAny<string>(), It.IsAny<int>()))
                .Returns(_series.TvdbId);

            Mocker.GetMock<IGameService>()
                .Setup(s => s.FindByIgdbId(_series.Id))
                .Returns(_series);

            var result = Subject.Map(_parsedRomInfo, _series.TvdbId, _series.MobyGamesId, _series.ImdbId, null);

            result.Game.Should().Be(_series);

            Mocker.GetMock<IGameService>()
                .Verify(v => v.FindByIgdbId(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_not_use_tvdbid_matching_when_alias_without_year_is_found_with_wrong_year()
        {
            var alias = "Game Alias";

            _parsedRomInfo.GameTitle = $"{alias} {_series.Year}";
            _parsedRomInfo.GameTitleInfo.TitleWithoutYear = alias;
            _parsedRomInfo.GameTitleInfo.Year = _series.Year + 1;

            Mocker.GetMock<ISceneMappingService>()
                .Setup(s => s.FindIgdbId(alias, It.IsAny<string>(), It.IsAny<int>()))
                .Returns(_series.TvdbId);

            Mocker.GetMock<IGameService>()
                .Setup(s => s.FindByIgdbId(_series.Id))
                .Returns(_series);

            var result = Subject.Map(_parsedRomInfo, 0, 0, "", null);

            result.Game.Should().BeNull();

            Mocker.GetMock<IGameService>()
                .Verify(v => v.FindByIgdbId(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_use_year_when_looking_up_by_all_titles_in_release_title()
        {
            var alias = "Game Alias";
            var title = "Game Title";

            _parsedRomInfo.GameTitle = $"Game Title AKA Game Alias {_series.Year}";
            _parsedRomInfo.GameTitleInfo.AllTitles = [
                title,
                alias
            ];
            _parsedRomInfo.GameTitleInfo.Year = _series.Year;

            Mocker.GetMock<IGameService>()
                .Setup(s => s.FindByTitle(title, _series.Year))
                .Returns(_series);

            var result = Subject.Map(_parsedRomInfo, 0, 0, "", null);

            result.Game.Should().Be(_series);
        }

        [Test]
        public void should_use_title_with_year_when_looking_up_by_all_titles_in_release_title()
        {
            var alias = "Game Alias";
            var title = "Game Title";

            _parsedRomInfo.GameTitle = $"Game Title AKA Game Alias {_series.Year}";
            _parsedRomInfo.GameTitleInfo.AllTitles = [
                title,
                alias
            ];
            _parsedRomInfo.GameTitleInfo.Year = _series.Year;

            Mocker.GetMock<IGameService>()
                .Setup(s => s.FindByTitle($"{title} {_series.Year}"))
                .Returns(_series);

            var result = Subject.Map(_parsedRomInfo, 0, 0, "", null);

            result.Game.Should().Be(_series);
        }
    }
}
