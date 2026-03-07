using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications.Search;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.DecisionEngineTests.Search
{
    [TestFixture]
    public class SeriesSpecificationFixture : TestBase<SeriesSpecification>
    {
        private Game _series1;
        private Game _series2;
        private RemoteEpisode _remoteRom = new();
        private SearchCriteriaBase _searchCriteria = new SingleEpisodeSearchCriteria();
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _series1 = Builder<Game>.CreateNew().With(s => s.Id = 1).Build();
            _series2 = Builder<Game>.CreateNew().With(s => s.Id = 2).Build();

            _remoteRom.Game = _series1;
            _information = new ReleaseDecisionInformation(false, _searchCriteria);
        }

        [Test]
        public void should_return_false_if_series_doesnt_match()
        {
            _searchCriteria.Game = _series2;

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_series_ids_match()
        {
            _searchCriteria.Game = _series1;

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();
        }
    }
}
