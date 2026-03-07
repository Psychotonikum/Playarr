using System;
using Moq;
using NUnit.Framework;
using Playarr.Core.Download;
using Playarr.Core.Download.Clients;
using Playarr.Core.HealthCheck.Checks;
using Playarr.Core.Localization;
using Playarr.Core.Test.Framework;
using Playarr.Test.Common;

namespace Playarr.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DownloadClientRemovesCompletedDownloadsCheckFixture : CoreTest<DownloadClientRemovesCompletedDownloadsCheck>
    {
        private DownloadClientInfo _clientStatus;
        private Mock<IDownloadClient> _downloadClient;

        private static Exception[] DownloadClientExceptions =
        {
            new DownloadClientUnavailableException("error"),
            new DownloadClientAuthenticationException("error"),
            new DownloadClientException("error")
        };

        [SetUp]
        public void Setup()
        {
            _clientStatus = new DownloadClientInfo
            {
                IsLocalhost = true,
                SortingMode = null,
                RemovesCompletedDownloads = true
            };

            _downloadClient = Mocker.GetMock<IDownloadClient>();
            _downloadClient.Setup(s => s.Definition)
                .Returns(new DownloadClientDefinition { Name = "Test" });

            _downloadClient.Setup(s => s.GetStatus())
                .Returns(_clientStatus);

            Mocker.GetMock<IProvideDownloadClient>()
                .Setup(s => s.GetDownloadClients(It.IsAny<bool>()))
                .Returns(new IDownloadClient[] { _downloadClient.Object });

            Mocker.GetMock<ILocalizationService>()
                .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                .Returns("Some Warning Message");
        }

        [Test]
        public void should_return_warning_if_removing_completed_downloads_is_enabled()
        {
            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_ok_if_remove_completed_downloads_is_not_enabled()
        {
            _clientStatus.RemovesCompletedDownloads = false;
            Subject.Check().ShouldBeOk();
        }

        [Test]
        [TestCaseSource("DownloadClientExceptions")]
        public void should_return_ok_if_client_throws_downloadclientexception(Exception ex)
        {
            _downloadClient.Setup(s => s.GetStatus())
                .Throws(ex);

            Subject.Check().ShouldBeOk();

            ExceptionVerification.ExpectedErrors(0);
        }
    }
}
