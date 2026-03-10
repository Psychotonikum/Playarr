using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Configuration;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.DecisionEngineTests
{
    public class MaximumSizeSpecificationFixture : CoreTest<MaximumSizeSpecification>
    {
        private RemoteRom _remoteRom;

        [SetUp]
        public void Setup()
        {
            _remoteRom = new RemoteRom() { Release = new ReleaseInfo() };
        }

        private void WithMaximumSize(int size)
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.MaximumSize).Returns(size);
        }

        private void WithSize(int size)
        {
            _remoteRom.Release.Size = size * 1024 * 1024;
        }

        [Test]
        public void should_return_true_when_maximum_size_is_set_to_zero()
        {
            WithMaximumSize(0);
            WithSize(1000);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_size_is_smaller_than_maximum_size()
        {
            WithMaximumSize(2000);
            WithSize(1999);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_size_is_equals_to_maximum_size()
        {
            WithMaximumSize(2000);
            WithSize(2000);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_size_is_bigger_than_maximum_size()
        {
            WithMaximumSize(2000);
            WithSize(2001);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_size_is_zero()
        {
            WithMaximumSize(2000);
            WithSize(0);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }
    }
}
