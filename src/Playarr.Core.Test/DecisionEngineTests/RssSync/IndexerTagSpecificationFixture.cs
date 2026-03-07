using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Datastore;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications.RssSync;
using Playarr.Core.Indexers;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class IndexerTagSpecificationFixture : CoreTest<IndexerTagSpecification>
    {
        private IndexerTagSpecification _specification;

        private RemoteEpisode _parseResultMulti;
        private IndexerDefinition _fakeIndexerDefinition;
        private Game _fakeSeries;
        private Rom _firstEpisode;
        private Rom _secondEpisode;
        private ReleaseInfo _fakeRelease;

        [SetUp]
        public void Setup()
        {
            _fakeIndexerDefinition = new IndexerDefinition
            {
                Tags = new HashSet<int>()
            };

            Mocker
                .GetMock<IIndexerFactory>()
                .Setup(m => m.Get(It.IsAny<int>()))
                .Throws(new ModelNotFoundException(typeof(IndexerDefinition), -1));

            Mocker
                .GetMock<IIndexerFactory>()
                .Setup(m => m.Get(1))
                .Returns(_fakeIndexerDefinition);

            _specification = Mocker.Resolve<IndexerTagSpecification>();

            _fakeSeries = Builder<Game>.CreateNew()
                .With(c => c.Monitored = true)
                .With(c => c.Tags = new HashSet<int>())
                .Build();

            _fakeRelease = new ReleaseInfo
            {
                IndexerId = 1
            };

            _firstEpisode = new Rom { Monitored = true };
            _secondEpisode = new Rom { Monitored = true };

            var doubleEpisodeList = new List<Rom> { _firstEpisode, _secondEpisode };

            _parseResultMulti = new RemoteEpisode
            {
                Game = _fakeSeries,
                Roms = doubleEpisodeList,
                Release = _fakeRelease
            };
        }

        [Test]
        public void indexer_and_series_without_tags_should_return_true()
        {
            _fakeIndexerDefinition.Tags = new HashSet<int>();
            _fakeSeries.Tags = new HashSet<int>();

            _specification.IsSatisfiedBy(_parseResultMulti, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = true })).Accepted.Should().BeTrue();
        }

        [Test]
        public void indexer_with_tags_series_without_tags_should_return_false()
        {
            _fakeIndexerDefinition.Tags = new HashSet<int> { 123 };
            _fakeSeries.Tags = new HashSet<int>();

            _specification.IsSatisfiedBy(_parseResultMulti, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = true })).Accepted.Should().BeFalse();
        }

        [Test]
        public void indexer_without_tags_series_with_tags_should_return_true()
        {
            _fakeIndexerDefinition.Tags = new HashSet<int>();
            _fakeSeries.Tags = new HashSet<int> { 123 };

            _specification.IsSatisfiedBy(_parseResultMulti, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = true })).Accepted.Should().BeTrue();
        }

        [Test]
        public void indexer_with_tags_series_with_matching_tags_should_return_true()
        {
            _fakeIndexerDefinition.Tags = new HashSet<int> { 123, 456 };
            _fakeSeries.Tags = new HashSet<int> { 123, 789 };

            _specification.IsSatisfiedBy(_parseResultMulti, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = true })).Accepted.Should().BeTrue();
        }

        [Test]
        public void indexer_with_tags_series_with_different_tags_should_return_false()
        {
            _fakeIndexerDefinition.Tags = new HashSet<int> { 456 };
            _fakeSeries.Tags = new HashSet<int> { 123, 789 };

            _specification.IsSatisfiedBy(_parseResultMulti, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = true })).Accepted.Should().BeFalse();
        }

        [Test]
        public void release_without_indexerid_should_return_true()
        {
            _fakeIndexerDefinition.Tags = new HashSet<int> { 456 };
            _fakeSeries.Tags = new HashSet<int> { 123, 789 };
            _fakeRelease.IndexerId = 0;

            _specification.IsSatisfiedBy(_parseResultMulti, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = true })).Accepted.Should().BeTrue();
        }

        [Test]
        public void release_with_invalid_indexerid_should_return_true()
        {
            _fakeIndexerDefinition.Tags = new HashSet<int> { 456 };
            _fakeSeries.Tags = new HashSet<int> { 123, 789 };
            _fakeRelease.IndexerId = 2;

            _specification.IsSatisfiedBy(_parseResultMulti, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = true })).Accepted.Should().BeTrue();
        }
    }
}
