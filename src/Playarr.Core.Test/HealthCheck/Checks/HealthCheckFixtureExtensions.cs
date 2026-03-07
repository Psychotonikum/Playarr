using FluentAssertions;
using Playarr.Common.Extensions;
using Playarr.Core.HealthCheck;

namespace Playarr.Core.Test.HealthCheck.Checks
{
    public static class HealthCheckFixtureExtensions
    {
        public static void ShouldBeOk(this Core.HealthCheck.HealthCheck result)
        {
            result.Type.Should().Be(HealthCheckResult.Ok);
        }

        public static void ShouldBeNotice(this Core.HealthCheck.HealthCheck result, string message = null)
        {
            result.Type.Should().Be(HealthCheckResult.Notice);

            if (message.IsNotNullOrWhiteSpace())
            {
                result.Message.Should().Contain(message);
            }
        }

        public static void ShouldBeWarning(this Core.HealthCheck.HealthCheck result, string message = null, string wikiFragment = null)
        {
            result.Type.Should().Be(HealthCheckResult.Warning);

            if (message.IsNotNullOrWhiteSpace())
            {
                result.Message.Should().Contain(message);
            }

            if (wikiFragment.IsNotNullOrWhiteSpace())
            {
                result.WikiUrl.Fragment.Should().Be(wikiFragment);
            }
        }

        public static void ShouldBeError(this Core.HealthCheck.HealthCheck result, string message = null, string wikiFragment = null)
        {
            result.Type.Should().Be(HealthCheckResult.Error);

            if (message.IsNotNullOrWhiteSpace())
            {
                result.Message.Should().Contain(message);
            }

            if (wikiFragment.IsNotNullOrWhiteSpace())
            {
                result.WikiUrl.Fragment.Should().Be(wikiFragment);
            }
        }
    }
}
