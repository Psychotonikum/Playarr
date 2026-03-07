using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Playarr.Core.Download;
using Playarr.Core.HealthCheck.Checks;
using Playarr.Core.Localization;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DownloadClientCheckFixture : CoreTest<DownloadClientCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        [Test]
        public void should_return_warning_when_download_client_has_not_been_configured()
        {
            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(s => s.GetDownloadClients(It.IsAny<bool>()))
                  .Returns(Array.Empty<IDownloadClient>());

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_error_when_download_client_throws()
        {
            var downloadClient = Mocker.GetMock<IDownloadClient>();
            downloadClient.Setup(s => s.Definition).Returns(new DownloadClientDefinition { Name = "Test" });

            downloadClient.Setup(s => s.GetItems())
                          .Throws<Exception>();

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(s => s.GetDownloadClients(It.IsAny<bool>()))
                  .Returns(new IDownloadClient[] { downloadClient.Object });

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_ok_when_download_client_returns()
        {
            var downloadClient = Mocker.GetMock<IDownloadClient>();

            downloadClient.Setup(s => s.GetItems())
                          .Returns(new List<DownloadClientItem>());

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(s => s.GetDownloadClients(It.IsAny<bool>()))
                  .Returns(new IDownloadClient[] { downloadClient.Object });

            Subject.Check().ShouldBeOk();
        }
    }
}
