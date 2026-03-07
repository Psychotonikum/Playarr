using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications.Search;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser.Model;
using Playarr.Test.Common;

namespace Playarr.Core.Test.DecisionEngineTests.Search.SingleEpisodeSearchMatchSpecificationTests
{
    [TestFixture]
    public class AnimeSearchFixture : TestBase<SingleEpisodeSearchMatchSpecification>
    {
        private RemoteEpisode _remoteRom = new();
        private AnimeEpisodeSearchCriteria _searchCriteria = new();
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _remoteRom.ParsedRomInfo = new ParsedRomInfo();
            _information = new ReleaseDecisionInformation(false, _searchCriteria);
        }

        [Test]
        public void should_return_false_if_full_season_result_for_single_episode_search()
        {
            _remoteRom.ParsedRomInfo.FullSeason = true;

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_not_a_full_season_result()
        {
            _remoteRom.ParsedRomInfo.FullSeason = false;

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_full_season_result_for_full_season_search()
        {
            _remoteRom.ParsedRomInfo.FullSeason = true;
            _searchCriteria.IsSeasonSearch = true;

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();
        }
    }
}
