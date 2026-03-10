using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.AutoTagging;
using Playarr.Core.Exceptions;
using Playarr.Core.MediaFiles;
using Playarr.Core.MetadataSource;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Core.Games.Commands;
using Playarr.Test.Common;

namespace Playarr.Core.Test.TvTests
{
    [TestFixture]
    public class RefreshGameServiceFixture : CoreTest<RefreshGameService>
    {
        private Game _game;

        [SetUp]
        public void Setup()
        {
            var season1 = Builder<Platform>.CreateNew()
                                         .With(s => s.PlatformNumber = 1)
                                         .Build();

            _game = Builder<Game>.CreateNew()
                                     .With(s => s.Status = GameStatusType.Continuing)
                                     .With(s => s.Platforms = new List<Platform>
                                                            {
                                                                season1
                                                            })
                                     .Build();

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetGame(_game.Id))
                  .Returns(_game);

            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetGameInfo(It.IsAny<int>()))
                  .Callback<int>(p => { throw new SeriesNotFoundException(p); });

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(_game))
                .Returns(new AutoTaggingChanges());
        }

        private void GivenNewSeriesInfo(Game game)
        {
            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetGameInfo(_game.IgdbId))
                  .Returns(new Tuple<Game, List<Rom>>(game, new List<Rom>()));
        }

        [Test]
        public void should_monitor_new_seasons_automatically_if_monitor_new_items_is_all()
        {
            _game.MonitorNewItems = NewItemMonitorTypes.All;

            var newGameInfo = _game.JsonClone();
            newGameInfo.Platforms.Add(Builder<Platform>.CreateNew()
                                         .With(s => s.PlatformNumber = 2)
                                         .Build());

            GivenNewSeriesInfo(newGameInfo);

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Platforms.Count == 2 && s.Platforms.Single(platform => platform.PlatformNumber == 2).Monitored == true), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_monitor_new_seasons_automatically_if_monitor_new_items_is_none()
        {
            _game.MonitorNewItems = NewItemMonitorTypes.None;

            var newGameInfo = _game.JsonClone();
            newGameInfo.Platforms.Add(Builder<Platform>.CreateNew()
                .With(s => s.PlatformNumber = 2)
                .Build());

            GivenNewSeriesInfo(newGameInfo);

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Platforms.Count == 2 && s.Platforms.Single(platform => platform.PlatformNumber == 2).Monitored == false), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_monitor_new_special_season_automatically()
        {
            var game = _game.JsonClone();
            game.Platforms.Add(Builder<Platform>.CreateNew()
                                         .With(s => s.PlatformNumber = 0)
                                         .Build());

            GivenNewSeriesInfo(game);

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Platforms.Count == 2 && s.Platforms.Single(platform => platform.PlatformNumber == 0).Monitored == false), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_update_tvrage_id_if_changed()
        {
            var newGameInfo = _game.JsonClone();
            newGameInfo.MobyGamesId = _game.MobyGamesId + 1;

            GivenNewSeriesInfo(newGameInfo);

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.MobyGamesId == newGameInfo.MobyGamesId), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_update_tvmaze_id_if_changed()
        {
            var newGameInfo = _game.JsonClone();
            newGameInfo.RawgId = _game.RawgId + 1;

            GivenNewSeriesInfo(newGameInfo);

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.RawgId == newGameInfo.RawgId), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_update_tmdb_id_if_changed()
        {
            var newGameInfo = _game.JsonClone();
            newGameInfo.TmdbId = _game.TmdbId + 1;

            GivenNewSeriesInfo(newGameInfo);

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.TmdbId == newGameInfo.TmdbId), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_log_error_if_igdb_id_not_found()
        {
            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Status == GameStatusType.Deleted), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_mark_as_deleted_if_igdb_id_not_found()
        {
            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Status == GameStatusType.Deleted), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_remark_as_deleted_if_igdb_id_not_found()
        {
            _game.Status = GameStatusType.Deleted;

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.IsAny<Game>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_update_if_igdb_id_changed()
        {
            var newGameInfo = _game.JsonClone();
            newGameInfo.IgdbId = _game.IgdbId + 1;

            GivenNewSeriesInfo(newGameInfo);

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.IgdbId == newGameInfo.IgdbId), It.IsAny<bool>(), It.IsAny<bool>()));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_throw_if_duplicate_season_is_in_existing_info()
        {
            var newGameInfo = _game.JsonClone();
            newGameInfo.Platforms.Add(Builder<Platform>.CreateNew()
                                         .With(s => s.PlatformNumber = 2)
                                         .Build());

            _game.Platforms.Add(Builder<Platform>.CreateNew()
                                         .With(s => s.PlatformNumber = 2)
                                         .Build());

            _game.Platforms.Add(Builder<Platform>.CreateNew()
                                         .With(s => s.PlatformNumber = 2)
                                         .Build());

            GivenNewSeriesInfo(newGameInfo);

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Platforms.Count == 2), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_filter_duplicate_seasons()
        {
            var newGameInfo = _game.JsonClone();
            newGameInfo.Platforms.Add(Builder<Platform>.CreateNew()
                                         .With(s => s.PlatformNumber = 2)
                                         .Build());

            newGameInfo.Platforms.Add(Builder<Platform>.CreateNew()
                                         .With(s => s.PlatformNumber = 2)
                                         .Build());

            GivenNewSeriesInfo(newGameInfo);

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Platforms.Count == 2), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_rescan_series_if_updating_fails()
        {
            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetGameInfo(_game.Id))
                  .Throws(new IOException());

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IDiskScanService>()
                  .Verify(v => v.Scan(_game), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_rescan_series_if_updating_fails_with_series_not_found()
        {
            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetGameInfo(_game.Id))
                  .Throws(new SeriesNotFoundException(_game.Id));

            Subject.Execute(new RefreshSeriesCommand(new List<int> { _game.Id }));

            Mocker.GetMock<IDiskScanService>()
                  .Verify(v => v.Scan(_game), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
