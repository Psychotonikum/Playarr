using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.DataAugmentation.Xem;
using Playarr.Core.DataAugmentation.Xem.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;
using Playarr.Test.Common;

namespace Playarr.Core.Test.DataAugmentation.SceneNumbering
{
    [TestFixture]
    public class XemServiceFixture : CoreTest<XemService>
    {
        private Game _series;
        private List<int> _theXemGameIds;
        private List<XemSceneTvdbMapping> _theXemTvdbMappings;
        private List<Rom> _episodes;

        [SetUp]
        public void SetUp()
        {
            _series = Builder<Game>.CreateNew()
                .With(v => v.TvdbId = 10)
                .With(v => v.UseSceneNumbering = false)
                .BuildNew();

            _theXemGameIds = new List<int> { 120 };
            Mocker.GetMock<IXemProxy>()
                  .Setup(v => v.GetXemGameIds())
                  .Returns(_theXemGameIds);

            _theXemTvdbMappings = new List<XemSceneTvdbMapping>();
            Mocker.GetMock<IXemProxy>()
                  .Setup(v => v.GetSceneTvdbMappings(10))
                  .Returns(_theXemTvdbMappings);

            _episodes = new List<Rom>();
            _episodes.Add(new Rom { SeasonNumber = 1, EpisodeNumber = 1 });
            _episodes.Add(new Rom { SeasonNumber = 1, EpisodeNumber = 2 });
            _episodes.Add(new Rom { SeasonNumber = 2, EpisodeNumber = 1 });
            _episodes.Add(new Rom { SeasonNumber = 2, EpisodeNumber = 2 });
            _episodes.Add(new Rom { SeasonNumber = 2, EpisodeNumber = 3 });
            _episodes.Add(new Rom { SeasonNumber = 2, EpisodeNumber = 4 });
            _episodes.Add(new Rom { SeasonNumber = 2, EpisodeNumber = 5 });
            _episodes.Add(new Rom { SeasonNumber = 3, EpisodeNumber = 1 });
            _episodes.Add(new Rom { SeasonNumber = 3, EpisodeNumber = 2 });

            Mocker.GetMock<IRomService>()
                  .Setup(v => v.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(_episodes);
        }

        private void GivenTvdbMappings()
        {
            _theXemGameIds.Add(10);

            AddTvdbMapping(1, 1, 1, 1, 1, 1); // 1x01 -> 1x01
            AddTvdbMapping(2, 1, 2, 2, 1, 2); // 1x02 -> 1x02
            AddTvdbMapping(3, 2, 1, 3, 2, 1); // 2x01 -> 2x01
            AddTvdbMapping(4, 2, 2, 4, 2, 2); // 2x02 -> 2x02
            AddTvdbMapping(5, 2, 3, 5, 2, 3); // 2x03 -> 2x03
            AddTvdbMapping(6, 3, 1, 6, 2, 4); // 3x01 -> 2x04
            AddTvdbMapping(7, 3, 2, 7, 2, 5); // 3x02 -> 2x05
        }

        private void GivenExistingMapping()
        {
            _series.UseSceneNumbering = true;

            _episodes[0].SceneSeasonNumber = 1;
            _episodes[0].SceneEpisodeNumber = 1;
            _episodes[1].SceneSeasonNumber = 1;
            _episodes[1].SceneEpisodeNumber = 2;
            _episodes[2].SceneSeasonNumber = 2;
            _episodes[2].SceneEpisodeNumber = 1;
            _episodes[3].SceneSeasonNumber = 2;
            _episodes[3].SceneEpisodeNumber = 2;
            _episodes[4].SceneSeasonNumber = 2;
            _episodes[4].SceneEpisodeNumber = 3;
            _episodes[5].SceneSeasonNumber = 3;
            _episodes[5].SceneEpisodeNumber = 1;
            _episodes[6].SceneSeasonNumber = 3;
            _episodes[6].SceneEpisodeNumber = 1;
        }

        private void AddTvdbMapping(int sceneAbsolute, int sceneSeason, int sceneEpisode, int tvdbAbsolute, int tvdbSeason, int tvdbEpisode)
        {
            _theXemTvdbMappings.Add(new XemSceneTvdbMapping
            {
                Scene = new XemValues { Absolute = sceneAbsolute, Platform = sceneSeason, Rom = sceneEpisode },
                Tvdb  = new XemValues { Absolute = tvdbAbsolute, Platform = tvdbSeason, Rom = tvdbEpisode },
            });
        }

        [Test]
        public void should_not_fetch_scenenumbering_if_not_listed()
        {
            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<IXemProxy>()
                  .Verify(v => v.GetSceneTvdbMappings(10), Times.Never());

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Game>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_fetch_scenenumbering()
        {
            GivenTvdbMappings();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.UseSceneNumbering == true), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_clear_scenenumbering_if_removed_from_thexem()
        {
            GivenExistingMapping();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Game>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_not_clear_scenenumbering_if_no_results_at_all_from_thexem()
        {
            GivenExistingMapping();

            _theXemGameIds.Clear();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Game>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_clear_scenenumbering_if_thexem_throws()
        {
            GivenExistingMapping();

            Mocker.GetMock<IXemProxy>()
                  .Setup(v => v.GetXemGameIds())
                  .Throws(new InvalidOperationException());

            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Game>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_flag_unknown_future_episodes_if_existing_season_is_mapped()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Tvdb.Platform == 2 && v.Tvdb.Rom == 5);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 5);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
        }

        [Test]
        public void should_flag_unknown_future_season_if_future_season_is_shifted()
        {
            GivenTvdbMappings();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.SeasonNumber == 3 && v.EpisodeNumber == 1);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
        }

        [Test]
        public void should_not_flag_unknown_future_season_if_future_season_is_not_shifted()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Scene.Platform == 3);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.SeasonNumber == 3 && v.EpisodeNumber == 1);

            rom.UnverifiedSceneNumbering.Should().BeFalse();
        }

        [Test]
        public void should_not_flag_past_episodes_if_not_causing_overlaps()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Scene.Platform == 2);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 1);

            rom.UnverifiedSceneNumbering.Should().BeFalse();
        }

        [Test]
        public void should_flag_past_episodes_if_causing_overlap()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Scene.Platform == 2 && v.Tvdb.Rom <= 1);
            _theXemTvdbMappings.First(v => v.Scene.Platform == 2 && v.Scene.Rom == 2).Scene.Rom = 1;

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 1);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
        }

        [Test]
        public void should_not_extrapolate_season_with_specials()
        {
            GivenTvdbMappings();
            var specialMapping = _theXemTvdbMappings.First(v => v.Tvdb.Platform == 2 && v.Tvdb.Rom == 5);
            specialMapping.Tvdb.Platform = 0;
            specialMapping.Tvdb.Rom = 1;

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 5);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
            rom.SceneSeasonNumber.Should().NotHaveValue();
            rom.SceneEpisodeNumber.Should().NotHaveValue();
        }

        [Test]
        public void should_extrapolate_season_with_future_episodes()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Tvdb.Platform == 2 && v.Tvdb.Rom == 5);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 5);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
            rom.SceneSeasonNumber.Should().Be(3);
            rom.SceneEpisodeNumber.Should().Be(2);
        }

        [Test]
        public void should_extrapolate_season_with_shifted_episodes()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Tvdb.Platform == 2 && v.Tvdb.Rom == 5);
            var dualMapping = _theXemTvdbMappings.First(v => v.Tvdb.Platform == 2 && v.Tvdb.Rom == 4);
            dualMapping.Scene.Platform = 2;
            dualMapping.Scene.Rom = 3;

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 5);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
            rom.SceneSeasonNumber.Should().Be(2);
            rom.SceneEpisodeNumber.Should().Be(4);
        }

        [Test]
        public void should_extrapolate_shifted_future_seasons()
        {
            GivenTvdbMappings();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.SeasonNumber == 3 && v.EpisodeNumber == 2);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
            rom.SceneSeasonNumber.Should().Be(4);
            rom.SceneEpisodeNumber.Should().Be(2);
        }

        [Test]
        public void should_not_extrapolate_matching_future_seasons()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Scene.Platform != 1);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.SeasonNumber == 3 && v.EpisodeNumber == 2);

            rom.UnverifiedSceneNumbering.Should().BeFalse();
            rom.SceneSeasonNumber.Should().NotHaveValue();
            rom.SceneEpisodeNumber.Should().NotHaveValue();
        }

        [Test]
        public void should_skip_mapping_when_scene_information_is_all_zero()
        {
            GivenTvdbMappings();

            AddTvdbMapping(0, 0, 0, 8, 3, 1); // 3x01 -> 3x01
            AddTvdbMapping(0, 0, 0, 9, 3, 2); // 3x02 -> 3x02

            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.UpdateEpisodes(It.Is<List<Rom>>(e => e.Any(c => c.SceneAbsoluteEpisodeNumber == 0 && c.SceneSeasonNumber == 0 && c.SceneEpisodeNumber == 0))), Times.Never());
        }
    }
}
