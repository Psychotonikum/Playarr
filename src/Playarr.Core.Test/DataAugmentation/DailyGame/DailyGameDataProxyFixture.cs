using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.DataAugmentation.DailySeries;
using Playarr.Core.Test.Framework;
using Playarr.Test.Common.Categories;

namespace Playarr.Core.Test.DataAugmentation.DailySeries
{
    [TestFixture]
    [IntegrationTest]
    public class DailySeriesDataProxyFixture : CoreTest<DailySeriesDataProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [Test]
        public void should_get_list_of_daily_series()
        {
            var list = Subject.GetDailyGameIds();
            list.Should().NotBeEmpty();
            list.Should().OnlyHaveUniqueItems();
        }
    }
}
