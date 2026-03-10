using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Indexers;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class BlockedIndexerSpecificationFixture : CoreTest<BlockedIndexerSpecification>
    {
        private RemoteRom _remoteRom;

        [SetUp]
        public void Setup()
        {
            _remoteRom = new RemoteRom
            {
                Release = new ReleaseInfo { IndexerId = 1 }
            };

            Mocker.GetMock<IIndexerStatusService>()
                  .Setup(v => v.GetBlockedProviders())
                  .Returns(new List<IndexerStatus>());
        }

        private void WithBlockedIndexer()
        {
            Mocker.GetMock<IIndexerStatusService>()
                  .Setup(v => v.GetBlockedProviders())
                  .Returns(new List<IndexerStatus> { new IndexerStatus { ProviderId = 1, DisabledTill = DateTime.UtcNow } });
        }

        [Test]
        public void should_return_true_if_no_blocked_indexer()
        {
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_blocked_indexer()
        {
            WithBlockedIndexer();

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
            Subject.Type.Should().Be(RejectionType.Temporary);
        }
    }
}
