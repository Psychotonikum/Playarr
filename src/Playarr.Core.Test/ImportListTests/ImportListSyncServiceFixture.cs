using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Core.Configuration;
using Playarr.Core.ImportLists;
using Playarr.Core.ImportLists.Exclusions;
using Playarr.Core.ImportLists.ImportListItems;
using Playarr.Core.MetadataSource;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.ImportListTests
{
    public class ImportListSyncServiceFixture : CoreTest<ImportListSyncService>
    {
        private ImportListFetchResult _importListFetch;
        private List<ImportListItemInfo> _list1Series;
        private List<ImportListItemInfo> _list2Series;

        private List<Game> _existingGame;
        private List<IImportList> _importLists;
        private ImportListSyncCommand _commandAll;
        private ImportListSyncCommand _commandSingle;

        [SetUp]
        public void SetUp()
        {
            _importLists = new List<IImportList>();

            var item1 = new ImportListItemInfo()
            {
                Title = "Breaking Bad"
            };

            _list1Series = new List<ImportListItemInfo>() { item1 };

            _existingGame = Builder<Game>.CreateListOfSize(3)
                .TheFirst(1)
                .With(s => s.IgdbId = 6)
                .With(s => s.ImdbId = "6")
                .With(s => s.TmdbId = 6)
                .With(s => s.MalIds = new HashSet<int> { 6 })
                .With(s => s.AniListIds = new HashSet<int> { 6 })
                .With(s => s.Monitored = true)
                .TheNext(1)
                .With(s => s.IgdbId = 7)
                .With(s => s.ImdbId = "7")
                .With(s => s.TmdbId = 7)
                .With(s => s.MalIds = new HashSet<int> { 7 })
                .With(s => s.AniListIds = new HashSet<int> { 7 })
                .With(s => s.Monitored = true)
                .TheNext(1)
                .With(s => s.IgdbId = 8)
                .With(s => s.ImdbId = "8")
                .With(s => s.TmdbId = 8)
                .With(s => s.MalIds = new HashSet<int> { 8 })
                .With(s => s.AniListIds = new HashSet<int> { 8 })
                .With(s => s.Monitored = true)
                .Build().ToList();

            _list2Series = Builder<ImportListItemInfo>.CreateListOfSize(3)
                .TheFirst(1)
                .With(s => s.IgdbId = 6)
                .With(s => s.ImdbId = "6")
                .With(s => s.TmdbId = 6)
                .With(s => s.MalId = 6)
                .With(s => s.AniListId = 6)
                .TheNext(1)
                .With(s => s.IgdbId = 7)
                .With(s => s.ImdbId = "7")
                .With(s => s.TmdbId = 7)
                .With(s => s.MalId = 7)
                .With(s => s.AniListId = 7)
                .TheNext(1)
                .With(s => s.IgdbId = 8)
                .With(s => s.ImdbId = "8")
                .With(s => s.TmdbId = 8)
                .With(s => s.MalId = 8)
                .With(s => s.AniListId = 8)
                .Build().ToList();

            _importListFetch = new ImportListFetchResult(_list1Series, false);

            _commandAll = new ImportListSyncCommand
            {
            };

            _commandSingle = new ImportListSyncCommand
            {
                DefinitionId = 1
            };

            var mockImportList = new Mock<IImportList>();

            Mocker.GetMock<IGameService>()
                  .Setup(v => v.AllGameIgdbIds())
                  .Returns(new List<int>());

            Mocker.GetMock<IGameService>()
                .Setup(v => v.GetAllGames())
                .Returns(_existingGame);

            Mocker.GetMock<ISearchForNewSeries>()
                  .Setup(v => v.SearchForNewSeries(It.IsAny<string>()))
                  .Returns(new List<Game>());

            Mocker.GetMock<ISearchForNewSeries>()
                  .Setup(v => v.SearchForNewSeriesByImdbId(It.IsAny<string>()))
                  .Returns(new List<Game>());

            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.All())
                  .Returns(() => _importLists.Select(x => x.Definition as ImportListDefinition).ToList());

            Mocker.GetMock<IImportListFactory>()
                .Setup(v => v.GetAvailableProviders())
                .Returns(_importLists);

            Mocker.GetMock<IImportListFactory>()
                .Setup(v => v.AutomaticAddEnabled(It.IsAny<bool>()))
                .Returns(() => _importLists.Where(x => (x.Definition as ImportListDefinition).EnableAutomaticAdd).ToList());

            Mocker.GetMock<IFetchAndParseImportList>()
                  .Setup(v => v.Fetch())
                  .Returns(_importListFetch);

            Mocker.GetMock<IImportListExclusionService>()
                  .Setup(v => v.All())
                  .Returns(new List<ImportListExclusion>());

            Mocker.GetMock<IImportListItemService>()
                .Setup(s => s.All())
                .Returns(new List<ImportListItemInfo>());
        }

        private void WithIgdbId()
        {
            _list1Series.First().IgdbId = 81189;
        }

        private void WithImdbId()
        {
            _list1Series.First().ImdbId = "tt0496424";

            Mocker.GetMock<ISearchForNewSeries>()
                .Setup(s => s.SearchForNewSeriesByImdbId(_list1Series.First().ImdbId))
                .Returns(
                    Builder<Game>
                        .CreateListOfSize(1)
                        .All()
                        .With(s => s.Title = "Breaking Bad")
                        .With(s => s.IgdbId = 81189)
                        .Build()
                        .ToList());
        }

        private void WithExistingSeries()
        {
            Mocker.GetMock<IGameService>()
                  .Setup(v => v.AllGameIgdbIds())
                  .Returns(new List<int> { _list1Series.First().IgdbId });
        }

        private void WithExcludedSeries()
        {
            Mocker.GetMock<IImportListExclusionService>()
                  .Setup(v => v.All())
                  .Returns(new List<ImportListExclusion>
                    {
                      new ImportListExclusion
                        {
                          IgdbId = _list1Series.First().IgdbId
                        }
                    });
        }

        private List<ImportListItemInfo> WithImportListItems(int count)
        {
            var importListItems = Builder<ImportListItemInfo>.CreateListOfSize(count)
                .Build()
                .ToList();

            Mocker.GetMock<IImportListItemService>()
                .Setup(s => s.All())
                .Returns(importListItems);

            return importListItems;
        }

        private void WithMonitorType(MonitorTypes monitor)
        {
            _importLists.ForEach(li => (li.Definition as ImportListDefinition).ShouldMonitor = monitor);
        }

        private void WithCleanLevel(ListSyncLevelType cleanLevel, int? tagId = null)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(v => v.ListSyncLevel)
                  .Returns(cleanLevel);
            if (tagId.HasValue)
            {
                Mocker.GetMock<IConfigService>()
                    .SetupGet(v => v.ListSyncTag)
                    .Returns(tagId.Value);
            }
        }

        private void WithList(int id, bool enabledAuto, int lastSyncHoursOffset = 0, bool pendingRemovals = true, DateTime? disabledTill = null)
        {
            var importListDefinition = new ImportListDefinition { Id = id, EnableAutomaticAdd = enabledAuto };

            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.Get(id))
                  .Returns(importListDefinition);

            var mockImportList = new Mock<IImportList>();
            mockImportList.SetupGet(s => s.Definition).Returns(importListDefinition);
            mockImportList.SetupGet(s => s.MinRefreshInterval).Returns(TimeSpan.FromHours(12));

            var status = new ImportListStatus()
            {
                LastInfoSync = DateTime.UtcNow.AddHours(lastSyncHoursOffset),
                HasRemovedItemSinceLastClean = pendingRemovals,
                DisabledTill = disabledTill
            };

            if (disabledTill.HasValue)
            {
                _importListFetch.AnyFailure = true;
            }

            Mocker.GetMock<IImportListStatusService>()
                .Setup(v => v.GetListStatus(id))
                .Returns(status);

            _importLists.Add(mockImportList.Object);
        }

        private void VerifyDidAddTag(int expectedSeriesCount, int expectedTagId)
        {
            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<List<Game>>(x => x.Count == expectedSeriesCount && x.All(game => game.Tags.Contains(expectedTagId))), true), Times.Once());
        }

        [Test]
        public void should_not_clean_library_if_lists_have_not_removed_any_items()
        {
            _importListFetch.Game = _existingGame.Select(x => new ImportListItemInfo() { IgdbId = x.IgdbId }).ToList();
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true, pendingRemovals: false);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.GetAllGames(), Times.Never());

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<List<Game>>(), true), Times.Never());
        }

        [Test]
        public void should_not_clean_library_if_config_value_disable()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.Disabled);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.GetAllGames(), Times.Never());

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(new List<Game>(), true), Times.Never());
        }

        [Test]
        public void should_log_only_on_clean_library_if_config_value_logonly()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.LogOnly);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.GetAllGames(), Times.Once());

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.DeleteGame(It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(new List<Game>(), true), Times.Once());
        }

        [Test]
        public void should_unmonitor_on_clean_library_if_config_value_keepAndUnmonitor()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var monitored = _existingGame.Count(x => x.Monitored);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.GetAllGames(), Times.Once());

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.DeleteGame(It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<List<Game>>(s => s.Count == monitored && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_igdb_match()
        {
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var importListItems = WithImportListItems(_existingGame.Count - 1);
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);

            for (var i = 0; i < importListItems.Count; i++)
            {
                importListItems[i].IgdbId = _existingGame[i].IgdbId;
            }

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<List<Game>>(s => s.Count == 1 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_imdb_match()
        {
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var importListItems = WithImportListItems(_existingGame.Count - 1);
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);

            for (var i = 0; i < importListItems.Count; i++)
            {
                importListItems[i].ImdbId = _existingGame[i].ImdbId;
            }

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<List<Game>>(s => s.Count == 1 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_tmdb_match()
        {
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var importListItems = WithImportListItems(_existingGame.Count - 1);
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);

            for (var i = 0; i < importListItems.Count; i++)
            {
                importListItems[i].TmdbId = _existingGame[i].TmdbId;
            }

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<List<Game>>(s => s.Count == 1 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_malid_match()
        {
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var importListItems = WithImportListItems(_existingGame.Count - 1);
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);

            for (var i = 0; i < importListItems.Count; i++)
            {
                importListItems[i].MalId = _existingGame[i].MalIds.First();
            }

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<List<Game>>(s => s.Count == 1 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_anilistid_match()
        {
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var importListItems = WithImportListItems(_existingGame.Count - 1);
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);

            for (var i = 0; i < importListItems.Count; i++)
            {
                importListItems[i].AniListId = _existingGame[i].AniListIds.First();
            }

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                .Verify(v => v.UpdateSeries(It.Is<List<Game>>(s => s.Count == 1 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_tag_series_on_clean_library_if_config_value_keepAndTag()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndTag, 1);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.GetAllGames(), Times.Once());

            VerifyDidAddTag(_existingGame.Count, 1);
        }

        [Test]
        public void should_not_clean_if_list_failures()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true, disabledTill: DateTime.UtcNow.AddHours(1));
            WithCleanLevel(ListSyncLevelType.LogOnly);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.GetAllGames(), Times.Never());

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<List<Game>>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IGameService>()
                .Verify(v => v.DeleteGame(It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_add_new_series_from_single_list_to_library()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithIgdbId();
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.Disabled);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddGameService>()
                  .Verify(v => v.AddGame(It.Is<List<Game>>(s => s.Count == 1), true), Times.Once());
        }

        [Test]
        public void should_add_new_series_from_multiple_list_to_library()
        {
            _list2Series.ForEach(m => m.ImportListId = 2);
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            _importListFetch.Game.AddRange(_list2Series);

            WithIgdbId();
            WithList(1, true);
            WithList(2, true);

            WithCleanLevel(ListSyncLevelType.Disabled);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddGameService>()
                  .Verify(v => v.AddGame(It.Is<List<Game>>(s => s.Count == 4), true), Times.Once());
        }

        [Test]
        public void should_add_new_series_to_library_only_from_enabled_lists()
        {
            _list2Series.ForEach(m => m.ImportListId = 2);
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            _importListFetch.Game.AddRange(_list2Series);

            WithIgdbId();
            WithList(1, true);
            WithList(2, false);

            WithCleanLevel(ListSyncLevelType.Disabled);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddGameService>()
                  .Verify(v => v.AddGame(It.Is<List<Game>>(s => s.Count == 1), true), Times.Once());
        }

        [Test]
        public void should_not_add_duplicate_series_from_seperate_lists()
        {
            _list2Series.ForEach(m => m.ImportListId = 2);
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            _importListFetch.Game.AddRange(_list2Series);
            _importListFetch.Game[0].IgdbId = 6;

            WithList(1, true);
            WithList(2, true);

            WithCleanLevel(ListSyncLevelType.Disabled);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddGameService>()
                  .Verify(v => v.AddGame(It.Is<List<Game>>(s => s.Count == 3), true), Times.Once());
        }

        [Test]
        public void should_not_search_if_series_title_and_series_id()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithIgdbId();
            Subject.Execute(_commandAll);

            Mocker.GetMock<ISearchForNewSeries>()
                  .Verify(v => v.SearchForNewSeries(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_search_by_imdb_if_series_title_and_series_imdb()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);

            WithList(1, true);
            WithImdbId();

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISearchForNewSeries>()
                  .Verify(v => v.SearchForNewSeriesByImdbId(It.IsAny<string>()), Times.Once());

            Mocker.GetMock<IAddGameService>()
                .Verify(v => v.AddGame(It.Is<List<Game>>(t => t.Count == 1), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_add_if_existing_series()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithIgdbId();
            WithExistingSeries();

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddGameService>()
                  .Verify(v => v.AddGame(It.Is<List<Game>>(t => t.Count == 0), It.IsAny<bool>()));
        }

        [TestCase(MonitorTypes.None, false)]
        [TestCase(MonitorTypes.All, true)]
        public void should_add_if_not_existing_series(MonitorTypes monitor, bool expectedGameMonitored)
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithIgdbId();
            WithMonitorType(monitor);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddGameService>()
                  .Verify(v => v.AddGame(It.Is<List<Game>>(t => t.Count == 1 && t.First().Monitored == expectedGameMonitored), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_add_if_excluded_series()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithIgdbId();
            WithExcludedSeries();

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddGameService>()
                  .Verify(v => v.AddGame(It.Is<List<Game>>(t => t.Count == 0), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_fetch_if_no_lists_are_enabled()
        {
            Mocker.GetMock<IImportListFactory>()
                .Setup(v => v.AutomaticAddEnabled(It.IsAny<bool>()))
                .Returns(new List<IImportList>());

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IFetchAndParseImportList>()
                .Verify(v => v.Fetch(), Times.Never);
        }

        [Test]
        public void should_not_process_if_no_items_are_returned()
        {
            Mocker.GetMock<IFetchAndParseImportList>()
                .Setup(v => v.Fetch())
                .Returns(new ImportListFetchResult());

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IImportListExclusionService>()
                .Verify(v => v.All(), Times.Never);
        }

        [Test]
        public void should_not_add_if_igdbid_is_0()
        {
            _importListFetch.Game.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithExcludedSeries();

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddGameService>()
                .Verify(v => v.AddGame(It.Is<List<Game>>(t => t.Count == 0), It.IsAny<bool>()));
        }
    }
}
