using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications.RssSync;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class MonitoredEpisodeSpecificationFixture : CoreTest<MonitoredEpisodeSpecification>
    {
        private MonitoredEpisodeSpecification _monitoredEpisodeSpecification;

        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private Game _fakeSeries;
        private Rom _firstEpisode;
        private Rom _secondEpisode;

        [SetUp]
        public void Setup()
        {
            _monitoredEpisodeSpecification = Mocker.Resolve<MonitoredEpisodeSpecification>();

            _fakeSeries = Builder<Game>.CreateNew()
                .With(c => c.Monitored = true)
                .Build();

            _firstEpisode = new Rom { Monitored = true };
            _secondEpisode = new Rom { Monitored = true };

            var singleEpisodeList = new List<Rom> { _firstEpisode };
            var doubleEpisodeList = new List<Rom> { _firstEpisode, _secondEpisode };

            _parseResultMulti = new RemoteEpisode
            {
                Game = _fakeSeries,
                Roms = doubleEpisodeList
            };

            _parseResultSingle = new RemoteEpisode
            {
                Game = _fakeSeries,
                Roms = singleEpisodeList
            };
        }

        private void WithFirstEpisodeUnmonitored()
        {
            _firstEpisode.Monitored = false;
        }

        private void WithSecondEpisodeUnmonitored()
        {
            _secondEpisode.Monitored = false;
        }

        [Test]
        public void setup_should_return_monitored_episode_should_return_true()
        {
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_episode_not_monitored_should_return_false()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void both_episodes_not_monitored_should_return_false()
        {
            WithFirstEpisodeUnmonitored();
            WithSecondEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_first_episode_not_monitored_should_return_false()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_second_episode_not_monitored_should_return_false()
        {
            WithSecondEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_single_episode_search()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_episode_is_monitored_for_season_search()
        {
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new SeasonSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_episode_is_not_monitored_for_season_search()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new SeasonSearchCriteria { MonitoredEpisodesOnly = true })).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_episode_is_not_monitored_and_monitoredEpisodesOnly_flag_is_false()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = false })).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_episode_is_not_monitored_and_monitoredEpisodesOnly_flag_is_true()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = true })).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_all_episodes_are_not_monitored_for_season_pack_release()
        {
            WithSecondEpisodeUnmonitored();
            _parseResultMulti.ParsedRomInfo = new ParsedRomInfo
                                                  {
                                                    FullSeason = true
                                                  };

            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }
    }
}
