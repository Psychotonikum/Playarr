using System;
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
    public class StandardEpisodeSearch : TestBase<SingleEpisodeSearchMatchSpecification>
    {
        private RemoteRom _remoteRom = new();
        private SingleEpisodeSearchCriteria _searchCriteria = new();
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _remoteRom.ParsedRomInfo = new ParsedRomInfo();
            _remoteRom.ParsedRomInfo.PlatformNumber = 5;
            _remoteRom.ParsedRomInfo.RomNumbers = new[] { 1 };
            _remoteRom.MappedPlatformNumber = 5;

            _searchCriteria.PlatformNumber = 5;
            _searchCriteria.EpisodeNumber = 1;
            _information = new ReleaseDecisionInformation(false, _searchCriteria);
        }

        [Test]
        public void should_return_false_if_season_does_not_match()
        {
            _remoteRom.ParsedRomInfo.PlatformNumber = 10;
            _remoteRom.MappedPlatformNumber = 10;

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_season_matches_after_scenemapping()
        {
            _remoteRom.ParsedRomInfo.PlatformNumber = 10;
            _remoteRom.MappedPlatformNumber = 5; // 10 -> 5 mapping
            _searchCriteria.PlatformNumber = 10; // searching by igdb 5 = 10 scene

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_season_does_not_match_after_scenemapping()
        {
            _remoteRom.ParsedRomInfo.PlatformNumber = 10;
            _remoteRom.MappedPlatformNumber = 6; // 9 -> 5 mapping
            _searchCriteria.PlatformNumber = 9; // searching by igdb 5 = 9 scene

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_full_season_result_for_single_episode_search()
        {
            _remoteRom.ParsedRomInfo.RomNumbers = Array.Empty<int>();

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_episode_number_does_not_match_search_criteria()
        {
            _remoteRom.ParsedRomInfo.RomNumbers = new[] { 2 };

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_full_season_result_for_full_season_search()
        {
            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();
        }
    }
}
