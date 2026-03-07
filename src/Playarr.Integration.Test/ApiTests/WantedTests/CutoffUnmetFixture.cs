using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Qualities;

namespace Playarr.Integration.Test.ApiTests.WantedTests
{
    [TestFixture]
    public class CutoffUnmetFixture : IntegrationTest
    {
        [Test]
        [Order(1)]
        public void cutoff_should_have_monitored_items()
        {
            EnsureQualityProfileCutoff(1, Quality.HDTV720p, true);
            var game = EnsureSeries(266189, "The Blacklist", true);
            EnsureRomFile(game, 1, 1, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "airDateUtc", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(1)]
        public void cutoff_should_not_have_unmonitored_items()
        {
            EnsureQualityProfileCutoff(1, Quality.HDTV720p, true);
            var game = EnsureSeries(266189, "The Blacklist", false);
            EnsureRomFile(game, 1, 1, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "airDateUtc", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(1)]
        public void cutoff_should_have_series()
        {
            EnsureQualityProfileCutoff(1, Quality.HDTV720p, true);
            var game = EnsureSeries(266189, "The Blacklist", true);
            EnsureRomFile(game, 1, 1, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "airDateUtc", "desc");

            result.Records.First().Game.Should().NotBeNull();
            result.Records.First().Game.Title.Should().Be("The Blacklist");
        }

        [Test]
        [Order(2)]
        public void cutoff_should_have_unmonitored_items()
        {
            EnsureQualityProfileCutoff(1, Quality.HDTV720p, true);
            var game = EnsureSeries(266189, "The Blacklist", false);
            EnsureRomFile(game, 1, 1, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "airDateUtc", "desc", "monitored", false);

            result.Records.Should().NotBeEmpty();
        }
    }
}
