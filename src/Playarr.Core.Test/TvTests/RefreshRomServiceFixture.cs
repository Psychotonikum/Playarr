using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.MetadataSource.SkyHook;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.TvTests
{
    [TestFixture]
    public class RefreshRomServiceFixture : CoreTest<RefreshRomService>
    {
        private List<Rom> _insertedEpisodes;
        private List<Rom> _updatedEpisodes;
        private List<Rom> _deletedEpisodes;
        private Tuple<Game, List<Rom>> _gameOfThrones;

        [OneTimeSetUp]
        public void TestFixture()
        {
            UseRealHttp();

            _gameOfThrones = Mocker.Resolve<SkyHookProxy>().GetSeriesInfo(121361); // Game of thrones

            // Remove specials.
            _gameOfThrones.Item2.RemoveAll(v => v.PlatformNumber == 0);
        }

        private List<Rom> GetEpisodes()
        {
            return _gameOfThrones.Item2.JsonClone();
        }

        private Game GetSeries()
        {
            var game = _gameOfThrones.Item1.JsonClone();

            return game;
        }

        private Game GetAnimeSeries()
        {
            var game = Builder<Game>.CreateNew().Build();
            game.SeriesType = GameTypes.Anime;

            return game;
        }

        [SetUp]
        public void Setup()
        {
            _insertedEpisodes = new List<Rom>();
            _updatedEpisodes = new List<Rom>();
            _deletedEpisodes = new List<Rom>();

            Mocker.GetMock<IRomService>().Setup(c => c.InsertMany(It.IsAny<List<Rom>>()))
                .Callback<List<Rom>>(e => _insertedEpisodes = e);

            Mocker.GetMock<IRomService>().Setup(c => c.UpdateMany(It.IsAny<List<Rom>>()))
                .Callback<List<Rom>>(e => _updatedEpisodes = e);

            Mocker.GetMock<IRomService>().Setup(c => c.DeleteMany(It.IsAny<List<Rom>>()))
                .Callback<List<Rom>>(e => _deletedEpisodes = e);
        }

        [Test]
        public void should_create_all_when_no_existing_episodes()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            Subject.RefreshRomInfo(GetSeries(), GetEpisodes());

            _insertedEpisodes.Should().HaveSameCount(GetEpisodes());
            _updatedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().BeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_update_all_when_all_existing_episodes()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(GetEpisodes());

            Subject.RefreshRomInfo(GetSeries(), GetEpisodes());

            _insertedEpisodes.Should().BeEmpty();
            _updatedEpisodes.Should().HaveSameCount(GetEpisodes());
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_delete_all_when_all_existing_episodes_are_gone_from_datasource()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(GetEpisodes());

            Subject.RefreshRomInfo(GetSeries(), new List<Rom>());

            _insertedEpisodes.Should().BeEmpty();
            _updatedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().HaveSameCount(GetEpisodes());
        }

        [Test]
        public void should_delete_duplicated_episodes_based_on_season_episode_number()
        {
            var duplicateEpisodes = GetEpisodes().Skip(5).Take(2).ToList();

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(GetEpisodes().Union(duplicateEpisodes).ToList());

            Subject.RefreshRomInfo(GetSeries(), GetEpisodes());

            _insertedEpisodes.Should().BeEmpty();
            _updatedEpisodes.Should().HaveSameCount(GetEpisodes());
            _deletedEpisodes.Should().HaveSameCount(duplicateEpisodes);
        }

        [Test]
        public void should_not_change_monitored_status_for_existing_episodes()
        {
            var game = GetSeries();
            game.Platforms = new List<Platform>();
            game.Platforms.Add(new Platform { PlatformNumber = 1, Monitored = false });

            var roms = GetEpisodes();

            roms.ForEach(e => e.Monitored = true);

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(roms);

            Subject.RefreshRomInfo(game, GetEpisodes());

            _updatedEpisodes.Should().HaveSameCount(GetEpisodes());
            _updatedEpisodes.Should().OnlyContain(e => e.Monitored == true);
        }

        [Test]
        public void should_not_set_monitored_status_for_old_episodes_to_false_if_episodes_existed()
        {
            var game = GetSeries();
            game.Platforms = new List<Platform>();
            game.Platforms.Add(new Platform { PlatformNumber = 1, Monitored = true });

            var roms = GetEpisodes().OrderBy(v => v.PlatformNumber).ThenBy(v => v.EpisodeNumber).Take(5).ToList();

            roms[1].AirDateUtc = DateTime.UtcNow.AddDays(-15);
            roms[2].AirDateUtc = DateTime.UtcNow.AddDays(-10);
            roms[3].AirDateUtc = DateTime.UtcNow.AddDays(1);

            var existingRoms = roms.Skip(4).ToList();

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existingRoms);

            Subject.RefreshRomInfo(game, roms);

            _insertedEpisodes = _insertedEpisodes.OrderBy(v => v.EpisodeNumber).ToList();

            _insertedEpisodes.Should().HaveCount(4);
            _insertedEpisodes[0].Monitored.Should().Be(true);
            _insertedEpisodes[1].Monitored.Should().Be(true);
            _insertedEpisodes[2].Monitored.Should().Be(true);
            _insertedEpisodes[3].Monitored.Should().Be(true);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_remove_duplicate_remote_episodes_before_processing()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            var roms = Builder<Rom>.CreateListOfSize(5)
                                           .TheFirst(2)
                                           .With(e => e.PlatformNumber = 1)
                                           .With(e => e.EpisodeNumber = 1)
                                           .Build()
                                           .ToList();

            Subject.RefreshRomInfo(GetSeries(), roms);

            _insertedEpisodes.Should().HaveCount(roms.Count - 1);
            _updatedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_set_absolute_episode_number_for_anime()
        {
            var roms = Builder<Rom>.CreateListOfSize(3).Build().ToList();

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            Subject.RefreshRomInfo(GetAnimeSeries(), roms);

            _insertedEpisodes.All(e => e.AbsoluteEpisodeNumber.HasValue).Should().BeTrue();
            _updatedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_set_absolute_episode_number_even_if_not_previously_set_for_anime()
        {
            var roms = Builder<Rom>.CreateListOfSize(3).Build().ToList();

            var existingRoms = roms.JsonClone();
            existingRoms.ForEach(e => e.AbsoluteEpisodeNumber = null);

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existingRoms);

            Subject.RefreshRomInfo(GetAnimeSeries(), roms);

            _insertedEpisodes.Should().BeEmpty();
            _updatedEpisodes.All(e => e.AbsoluteEpisodeNumber.HasValue).Should().BeTrue();
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_ignore_episodes_with_no_absolute_episode_in_distinct_by_absolute()
        {
            var roms = Builder<Rom>.CreateListOfSize(10)
                                           .Build()
                                           .ToList();

            roms[0].AbsoluteEpisodeNumber = null;
            roms[1].AbsoluteEpisodeNumber = null;
            roms[2].AbsoluteEpisodeNumber = null;
            roms[3].AbsoluteEpisodeNumber = null;
            roms[4].AbsoluteEpisodeNumber = null;

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            Subject.RefreshRomInfo(GetAnimeSeries(), roms);

            _insertedEpisodes.Should().HaveCount(roms.Count);
        }

        [Test]
        public void should_override_empty_airdate_for_direct_to_dvd()
        {
            var game = GetSeries();
            game.Status = GameStatusType.Ended;

            var roms = Builder<Rom>.CreateListOfSize(10)
                                           .All()
                                           .With(v => v.AirDateUtc = null)
                                           .BuildListOfNew();

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            List<Rom> updateEpisodes = null;
            Mocker.GetMock<IRomService>().Setup(c => c.InsertMany(It.IsAny<List<Rom>>()))
                .Callback<List<Rom>>(c => updateEpisodes = c);

            Subject.RefreshRomInfo(game, roms);

            updateEpisodes.Should().NotBeNull();
            updateEpisodes.Should().NotBeEmpty();
            updateEpisodes.All(v => v.AirDateUtc.HasValue).Should().BeTrue();
        }

        [Test]
        public void should_use_tba_for_episode_title_when_null()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            var roms = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.Title = null)
                                           .Build()
                                           .ToList();

            Subject.RefreshRomInfo(GetSeries(), roms);

            _insertedEpisodes.First().Title.Should().Be("TBA");
        }

        [Test]
        public void should_update_air_date_when_multiple_episodes_air_on_the_same_day()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            var now = DateTime.UtcNow;
            var game = GetSeries();

            var roms = Builder<Rom>.CreateListOfSize(2)
                                           .All()
                                           .With(e => e.PlatformNumber = 1)
                                           .With(e => e.AirDate = now.ToShortDateString())
                                           .With(e => e.AirDateUtc = now)
                                           .Build()
                                           .ToList();

            Subject.RefreshRomInfo(game, roms);

            _insertedEpisodes.First().AirDateUtc.Value.ToString("s").Should().Be(roms.First().AirDateUtc.Value.ToString("s"));
            _insertedEpisodes.Last().AirDateUtc.Value.ToString("s").Should().Be(roms.First().AirDateUtc.Value.AddMinutes(game.Runtime).ToString("s"));
        }

        [Test]
        public void should_not_update_air_date_when_more_than_three_episodes_air_on_the_same_day()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            var now = DateTime.UtcNow;
            var game = GetSeries();

            var roms = Builder<Rom>.CreateListOfSize(4)
                                           .All()
                                           .With(e => e.PlatformNumber = 1)
                                           .With(e => e.AirDate = now.ToShortDateString())
                                           .With(e => e.AirDateUtc = now)
                                           .Build()
                                           .ToList();

            Subject.RefreshRomInfo(game, roms);

            _insertedEpisodes.Should().OnlyContain(e => e.AirDateUtc.Value.ToString("s") == roms.First().AirDateUtc.Value.ToString("s"));
        }

        [Test]
        public void should_match_anime_episodes_by_season_and_episode_numbers()
        {
            var roms = Builder<Rom>.CreateListOfSize(2)
                .Build()
                .ToList();

            roms[0].AbsoluteEpisodeNumber = null;
            roms[0].PlatformNumber.Should().NotBe(roms[1].PlatformNumber);
            roms[0].EpisodeNumber.Should().NotBe(roms[1].EpisodeNumber);

            var existingRom = new Rom
            {
                PlatformNumber = roms[0].PlatformNumber,
                EpisodeNumber = roms[0].EpisodeNumber,
                AbsoluteEpisodeNumber = roms[1].AbsoluteEpisodeNumber
            };

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom> { existingRom });

            Subject.RefreshRomInfo(GetAnimeSeries(), roms);

            _updatedEpisodes.First().PlatformNumber.Should().Be(roms[0].PlatformNumber);
            _updatedEpisodes.First().EpisodeNumber.Should().Be(roms[0].EpisodeNumber);
            _updatedEpisodes.First().AbsoluteEpisodeNumber.Should().Be(roms[0].AbsoluteEpisodeNumber);

            _insertedEpisodes.First().PlatformNumber.Should().Be(roms[1].PlatformNumber);
            _insertedEpisodes.First().EpisodeNumber.Should().Be(roms[1].EpisodeNumber);
            _insertedEpisodes.First().AbsoluteEpisodeNumber.Should().Be(roms[1].AbsoluteEpisodeNumber);
        }

        [Test]
        public void should_mark_updated_episodes_that_have_newly_added_absolute_episode_number()
        {
            var roms = Builder<Rom>.CreateListOfSize(3)
                .Build()
                .ToList();

            var existingRoms = new List<Rom>
            {
                roms[0],
                roms[1]
            };

            existingRoms[0].AbsoluteEpisodeNumber = null;

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existingRoms);

            Subject.RefreshRomInfo(GetAnimeSeries(), roms);

            _updatedEpisodes.First().PlatformNumber.Should().Be(roms[1].PlatformNumber);
            _updatedEpisodes.First().EpisodeNumber.Should().Be(roms[1].EpisodeNumber);
            _updatedEpisodes.First().AbsoluteEpisodeNumber.Should().NotBeNull();
            _updatedEpisodes.First().AbsoluteRomNumberAdded.Should().BeTrue();

            _insertedEpisodes.Any(e => e.AbsoluteRomNumberAdded).Should().BeFalse();
        }

        [Test]
        public void should_monitor_new_episode_if_season_is_monitored()
        {
            var game = GetSeries();
            game.Platforms = new List<Platform>();
            game.Platforms.Add(new Platform { PlatformNumber = 1, Monitored = true });

            var roms = Builder<Rom>.CreateListOfSize(2)
                .All()
                .With(e => e.PlatformNumber = 1)
                .Build()
                .ToList();

            var existingRom = new Rom
            {
                PlatformNumber = roms[0].PlatformNumber,
                EpisodeNumber = roms[0].EpisodeNumber,
                Monitored = true
            };

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom> { existingRom });

            Subject.RefreshRomInfo(game, roms);

            _updatedEpisodes.Should().HaveCount(1);
            _insertedEpisodes.Should().HaveCount(1);
            _insertedEpisodes.Should().OnlyContain(e => e.Monitored == true);
        }

        [Test]
        public void should_not_monitor_new_episode_if_season_is_not_monitored()
        {
            var game = GetSeries();
            game.Platforms = new List<Platform>();
            game.Platforms.Add(new Platform { PlatformNumber = 1, Monitored = false });

            var roms = Builder<Rom>.CreateListOfSize(2)
                .All()
                .With(e => e.PlatformNumber = 1)
                .Build()
                .ToList();

            var existingRom = new Rom
            {
                PlatformNumber = roms[0].PlatformNumber,
                EpisodeNumber = roms[0].EpisodeNumber,
                Monitored = true
            };

            Mocker.GetMock<IRomService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Rom> { existingRom });

            Subject.RefreshRomInfo(game, roms);

            _updatedEpisodes.Should().HaveCount(1);
            _insertedEpisodes.Should().HaveCount(1);
            _insertedEpisodes.Should().OnlyContain(e => e.Monitored == false);
        }
    }
}
