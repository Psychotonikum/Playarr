using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Datastore;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Download;
using Playarr.Core.IndexerSearch;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.IndexerSearchTests
{
    [TestFixture]
    public class SeriesSearchServiceFixture : CoreTest<SeriesSearchService>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _series = new Game
                      {
                          Id = 1,
                          Title = "Title",
                          Platforms = new List<Platform>(),
                          QualityProfile = new LazyLoaded<QualityProfile>(Builder<QualityProfile>.CreateNew().With(q => q.UpgradeAllowed = true).Build())
                      };

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetSeries(It.IsAny<int>()))
                  .Returns(_series);

            Mocker.GetMock<ISearchForReleases>()
                  .Setup(s => s.SeasonSearch(_series.Id, It.IsAny<int>(), false, false, true, false))
                  .Returns(Task.FromResult(new List<DownloadDecision>()));

            Mocker.GetMock<IProcessDownloadDecisions>()
                  .Setup(s => s.ProcessDecisions(It.IsAny<List<DownloadDecision>>()))
                  .Returns(Task.FromResult(new ProcessedDecisions(new List<DownloadDecision>(), new List<DownloadDecision>(), new List<DownloadDecision>())));
        }

        [Test]
        public void should_only_include_monitored_seasons()
        {
            _series.Platforms = new List<Platform>
                              {
                                  new Platform { SeasonNumber = 0, Monitored = false },
                                  new Platform { SeasonNumber = 1, Monitored = true }
                              };

            Subject.Execute(new SeriesSearchCommand { SeriesId = _series.Id, Trigger = CommandTrigger.Manual });

            Mocker.GetMock<ISearchForReleases>()
                .Verify(v => v.SeasonSearch(_series.Id, It.IsAny<int>(), false, true, true, false), Times.Exactly(_series.Platforms.Count(s => s.Monitored)));
        }

        [Test]
        public void should_only_search_missing_if_profile_does_not_allow_upgrades()
        {
            _series.Platforms = new List<Platform>
            {
                new Platform { SeasonNumber = 0, Monitored = false },
                new Platform { SeasonNumber = 1, Monitored = true }
            };

            _series.QualityProfile.Value.UpgradeAllowed = false;

            Subject.Execute(new SeriesSearchCommand { SeriesId = _series.Id, Trigger = CommandTrigger.Manual });

            Mocker.GetMock<ISearchForReleases>()
                .Verify(v => v.SeasonSearch(_series.Id, It.IsAny<int>(), true, true, true, false), Times.Exactly(_series.Platforms.Count(s => s.Monitored)));
        }

        [Test]
        public void should_start_with_lower_seasons_first()
        {
            var seasonOrder = new List<int>();

            _series.Platforms = new List<Platform>
                              {
                                  new Platform { SeasonNumber = 3, Monitored = true },
                                  new Platform { SeasonNumber = 1, Monitored = true },
                                  new Platform { SeasonNumber = 2, Monitored = true }
                              };

            Mocker.GetMock<ISearchForReleases>()
                  .Setup(s => s.SeasonSearch(_series.Id, It.IsAny<int>(), false, true, true, false))
                  .Returns(Task.FromResult(new List<DownloadDecision>()))
                  .Callback<int, int, bool, bool, bool, bool>((gameId, platformNumber, missingOnly, monitoredOnly, userInvokedSearch, interactiveSearch) => seasonOrder.Add(platformNumber));

            Subject.Execute(new SeriesSearchCommand { SeriesId = _series.Id, Trigger = CommandTrigger.Manual });

            seasonOrder.First().Should().Be(_series.Platforms.OrderBy(s => s.SeasonNumber).First().SeasonNumber);
        }
    }
}
