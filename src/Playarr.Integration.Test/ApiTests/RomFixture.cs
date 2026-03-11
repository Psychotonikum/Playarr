using NUnit.Framework;

namespace Playarr.Integration.Test.ApiTests
{
    [TestFixture]
    [Ignore("Games do not auto-create ROMs from metadata like TV episodes - ROM files are created by disk scan")]
    public class EpisodeFixture : IntegrationTest
    {
        [Test]
        public void placeholder()
        {
            Assert.Pass("ROM tests require disk-based ROM files, not metadata-derived episodes");
        }
    }
}
