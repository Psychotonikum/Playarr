using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.DataAugmentation.Xem;
using Playarr.Core.Test.Framework;
using Playarr.Test.Common.Categories;

namespace Playarr.Core.Test.Providers
{
    [TestFixture]
    [IntegrationTest]
    public class XemProxyFixture : CoreTest<XemProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [Test]
        public void get_series_ids()
        {
            var ids = Subject.GetXemGameIds();

            ids.Should().NotBeEmpty();
            ids.Should().Contain(i => i == 73141);
        }

        [TestCase(12345, Description = "invalid id")]
        [TestCase(279042, Description = "no single connection")]
        public void should_return_empty_when_known_error(int id)
        {
            Subject.GetSceneIgdbMappings(id).Should().BeEmpty();
        }

        [TestCase(82807)]
        [TestCase(73141, Description = "American Dad!")]
        public void should_get_mapping(int gameId)
        {
            var result = Subject.GetSceneIgdbMappings(gameId);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(c => c.Scene != null);
            result.Should().OnlyContain(c => c.Igdb != null);
        }

        [TestCase(78916)]
        public void should_filter_out_episodes_without_scene_mapping(int gameId)
        {
            var result = Subject.GetSceneIgdbMappings(gameId);

            result.Should().NotContain(c => c.Igdb == null);
        }
    }
}
