using System;
using System.Text;
using Moq;
using NUnit.Framework;
using Playarr.Common.Cloud;
using Playarr.Common.Http;
using Playarr.Common.Serializer;
using Playarr.Core.HealthCheck.Checks;
using Playarr.Core.Localization;
using Playarr.Core.Test.Framework;
using Playarr.Test.Common;

namespace Playarr.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class SystemTimeCheckFixture : CoreTest<SystemTimeCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IPlayarrCloudRequestBuilder>(new PlayarrCloudRequestBuilder());

            Mocker.GetMock<ILocalizationService>()
                .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                .Returns("Some Warning Message");
        }

        private void GivenServerTime(DateTime dateTime)
        {
            var json = new ServiceTimeResponse { DateTimeUtc = dateTime }.ToJson();

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), Encoding.ASCII.GetBytes(json)));
        }

        [Test]
        public void should_not_return_error_when_system_time_is_close_to_server_time()
        {
            GivenServerTime(DateTime.UtcNow);

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_error_when_system_time_is_more_than_one_day_from_server_time()
        {
            GivenServerTime(DateTime.UtcNow.AddDays(2));

            Subject.Check().ShouldBeError();
            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
