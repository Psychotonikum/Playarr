using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Test.Framework;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Test.ThingiProviderTests
{
    [TestFixture]
    public class NullConfigFixture : CoreTest<NullConfig>
    {
        [Test]
        public void should_be_valid()
        {
            Subject.Validate().IsValid.Should().BeTrue();
        }
    }
}
