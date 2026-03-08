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
        private List<XemSceneIgdbMapping> _theXemIgdbMappings;
        private List<Rom> _episodes;

        [SetUp]
        public void SetUp()
        {
            _series = Builder<Game>.CreateNew()
                .With(v => v.IgdbId = 10)
                .With(v => v.UseSceneNumbering = false)
                .BuildNew();

            _theXemGameIds = new List<int> { 120 };
            Mocker.GetMock<IXemProxy>()
                  .Setup(v => v.GetXemGameIds())
                  .Returns(_theXemGameIds);

            _theXemIgdbMappings = new List<XemSceneIgdbMapping>();
            Mocker.GetMock<IXemProxy>()
                  .Setup(v => v.GetSceneIgdbMappings(10))
                  .Returns(_theXemIgdbMappings);

            _episodes = new List<Rom>();
            _episodes.Add(new Rom { PlatformNumber = 1, EpisodeNumber = 1 });
            _episodes.Add(new Rom { PlatformNumber = 1, EpisodeNumber = 2 });
            _episodes.Add(new Rom { PlatformNumber = 2, EpisodeNumber = 1 });
            _episodes.Add(new Rom { PlatformNumber = 2, EpisodeNumber = 2 });
            _episodes.Add(new Rom { PlatformNumber = 2, EpisodeNumber = 3 });
            _episodes.Add(new Rom { PlatformNumber = 2, EpisodeNumber = 4 });
            _episodes.Add(new Rom { PlatformNumber = 2, EpisodeNumber = 5 });
            _episodes.Add(new Rom { PlatformNumber = 3, EpisodeNumber = 1 });
            _episodes.Add(new Rom { PlatformNumber = 3, EpisodeNumber = 2 });

            Mocker.GetMock<IRomService>()
                  .Setup(v => v.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(_episodes);
        }

        private void GivenIgdbMappings()
        {
            _theXemGameIds.Add(10);

            AddIgdbMapping(1, 1, 1, 1, 1, 1); // 1x01 -> 1x01
            AddIgdbMapping(2, 1, 2, 2, 1, 2); // 1x02 -> 1x02
            AddIgdbMapping(3, 2, 1, 3, 2, 1); // 2x01 -> 2x01
            AddIgdbMapping(4, 2, 2, 4, 2, 2); // 2x02 -> 2x02
            AddIgdbMapping(5, 2, 3, 5, 2, 3); // 2x03 -> 2x03
            AddIgdbMapping(6, 3, 1, 6, 2, 4); // 3x01 -> 2x04
            AddIgdbMapping(7, 3, 2, 7, 2, 5); // 3x02 -> 2x05
        }

        private void GivenExistingMapping()
        {
            _series.UseSceneNumbering = true;

            _episodes[0].ScenePlatformNumber = 1;
            _episodes[0].SceneEpisodeNumber = 1;
            _episodes[1].ScenePlatformNumber = 1;
            _episodes[1].SceneEpisodeNumber = 2;
            _episodes[2].ScenePlatformNumber = 2;
            _episodes[2].SceneEpisodeNumber = 1;
            _episodes[3].ScenePlatformNumber = 2;
            _episodes[3].SceneEpisodeNumber = 2;
            _episodes[4].ScenePlatformNumber = 2;
            _episodes[4].SceneEpisodeNumber = 3;
            _episodes[5].ScenePlatformNumber = 3;
            _episodes[5].SceneEpisodeNumber = 1;
            _episodes[6].ScenePlatformNumber = 3;
            _episodes[6].SceneEpisodeNumber = 1;
        }

        private void AddIgdbMapping(int sceneAbsolute, int sceneSeason, int sceneEpisode, int igdbAbsolute, int igdbSeason, int igdbEpisode)
        {
            _theXemIgdbMappings.Add(new XemSceneIgdbMapping
            {
                Scene = new XemValues { Absolute = sceneAbsolute, Platform = sceneSeason, Rom = sceneEpisode },
                Igdb  = new XemValues { Absolute = igdbAbsolute, Platform = igdbSeason, Rom = igdbEpisode },
            });
        }

        [Test]
        public void should_not_fetch_scenenumbering_if_not_listed()
        {
            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<IXemProxy>()
                  .Verify(v => v.GetSceneIgdbMappings(10), Times.Never());

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Game>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_fetch_scenenumbering()
        {
            GivenIgdbMappings();

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
            GivenIgdbMappings();
            _theXemIgdbMappings.RemoveAll(v => v.Igdb.Platform == 2 && v.Igdb.Rom == 5);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.PlatformNumber == 2 && v.EpisodeNumber == 5);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
        }

        [Test]
        public void should_flag_unknown_future_season_if_future_season_is_shifted()
        {
            GivenIgdbMappings();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.PlatformNumber == 3 && v.EpisodeNumber == 1);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
        }

        [Test]
        public void should_not_flag_unknown_future_season_if_future_season_is_not_shifted()
        {
            GivenIgdbMappings();
            _theXemIgdbMappings.RemoveAll(v => v.Scene.Platform == 3);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.PlatformNumber == 3 && v.EpisodeNumber == 1);

            rom.UnverifiedSceneNumbering.Should().BeFalse();
        }

        [Test]
        public void should_not_flag_past_episodes_if_not_causing_overlaps()
        {
            GivenIgdbMappings();
            _theXemIgdbMappings.RemoveAll(v => v.Scene.Platform == 2);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.PlatformNumber == 2 && v.EpisodeNumber == 1);

            rom.UnverifiedSceneNumbering.Should().BeFalse();
        }

        [Test]
        public void should_flag_past_episodes_if_causing_overlap()
        {
            GivenIgdbMappings();
            _theXemIgdbMappings.RemoveAll(v => v.Scene.Platform == 2 && v.Igdb.Rom <= 1);
            _theXemIgdbMappings.First(v => v.Scene.Platform == 2 && v.Scene.Rom == 2).Scene.Rom = 1;

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.PlatformNumber == 2 && v.EpisodeNumber == 1);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
        }

        [Test]
        public void should_not_extrapolate_season_with_specials()
        {
            GivenIgdbMappings();
            var specialMapping = _theXemIgdbMappings.First(v => v.Igdb.Platform == 2 && v.Igdb.Rom == 5);
            specialMapping.Igdb.Platform = 0;
            specialMapping.Igdb.Rom = 1;

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.PlatformNumber == 2 && v.EpisodeNumber == 5);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
            rom.ScenePlatformNumber.Should().NotHaveValue();
            rom.SceneEpisodeNumber.Should().NotHaveValue();
        }

        [Test]
        public void should_extrapolate_season_with_future_episodes()
        {
            GivenIgdbMappings();
            _theXemIgdbMappings.RemoveAll(v => v.Igdb.Platform == 2 && v.Igdb.Rom == 5);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.PlatformNumber == 2 && v.EpisodeNumber == 5);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
            rom.ScenePlatformNumber.Should().Be(3);
            rom.SceneEpisodeNumber.Should().Be(2);
        }

        [Test]
        public void should_extrapolate_season_with_shifted_episodes()
        {
            GivenIgdbMappings();
            _theXemIgdbMappings.RemoveAll(v => v.Igdb.Platform == 2 && v.Igdb.Rom == 5);
            var dualMapping = _theXemIgdbMappings.First(v => v.Igdb.Platform == 2 && v.Igdb.Rom == 4);
            dualMapping.Scene.Platform = 2;
            dualMapping.Scene.Rom = 3;

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.PlatformNumber == 2 && v.EpisodeNumber == 5);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
            rom.ScenePlatformNumber.Should().Be(2);
            rom.SceneEpisodeNumber.Should().Be(4);
        }

        [Test]
        public void should_extrapolate_shifted_future_seasons()
        {
            GivenIgdbMappings();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.PlatformNumber == 3 && v.EpisodeNumber == 2);

            rom.UnverifiedSceneNumbering.Should().BeTrue();
            rom.ScenePlatformNumber.Should().Be(4);
            rom.SceneEpisodeNumber.Should().Be(2);
        }

        [Test]
        public void should_not_extrapolate_matching_future_seasons()
        {
            GivenIgdbMappings();
            _theXemIgdbMappings.RemoveAll(v => v.Scene.Platform != 1);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var rom = _episodes.First(v => v.PlatformNumber == 3 && v.EpisodeNumber == 2);

            rom.UnverifiedSceneNumbering.Should().BeFalse();
            rom.ScenePlatformNumber.Should().NotHaveValue();
            rom.SceneEpisodeNumber.Should().NotHaveValue();
        }

        [Test]
        public void should_skip_mapping_when_scene_information_is_all_zero()
        {
            GivenIgdbMappings();

            AddIgdbMapping(0, 0, 0, 8, 3, 1); // 3x01 -> 3x01
            AddIgdbMapping(0, 0, 0, 9, 3, 2); // 3x02 -> 3x02

            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.UpdateEpisodes(It.Is<List<Rom>>(e => e.Any(c => c.SceneAbsoluteEpisodeNumber == 0 && c.ScenePlatformNumber == 0 && c.SceneEpisodeNumber == 0))), Times.Never());
        }
    }
}
