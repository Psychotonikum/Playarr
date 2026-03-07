using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Core.MediaFiles;
using Playarr.Core.Notifications;
using Playarr.Core.Notifications.Xbmc;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class OnDownloadFixture : CoreTest<Notifications.Xbmc.Xbmc>
    {
        private DownloadMessage _downloadMessage;

        [SetUp]
        public void Setup()
        {
            var game = Builder<Game>.CreateNew()
                                        .Build();

            var romFile = Builder<RomFile>.CreateNew()
                                                   .Build();

            _downloadMessage = Builder<DownloadMessage>.CreateNew()
                                                       .With(d => d.Game = game)
                                                       .With(d => d.RomFile = romFile)
                                                       .With(d => d.OldFiles = new List<DeletedRomFile>())
                                                       .Build();

            Subject.Definition = new NotificationDefinition();
            Subject.Definition.Settings = new XbmcSettings
                                          {
                                              Host = "localhost",
                                              UpdateLibrary = true
                                          };
        }

        private void GivenOldFiles()
        {
            _downloadMessage.OldFiles = Builder<DeletedRomFile>
                .CreateListOfSize(1)
                .All()
                .WithFactory(() => new DeletedRomFile(Builder<RomFile>.CreateNew().Build(), null))
                .Build()
                .ToList();

            Subject.Definition.Settings = new XbmcSettings
                                          {
                                              Host = "localhost",
                                              UpdateLibrary = true,
                                              CleanLibrary = true
                                          };
        }

        [Test]
        public void should_not_clean_if_no_episode_was_replaced()
        {
            Subject.OnDownload(_downloadMessage);
            Subject.ProcessQueue();

            Mocker.GetMock<IXbmcService>().Verify(v => v.Clean(It.IsAny<XbmcSettings>()), Times.Never());
        }

        [Test]
        public void should_clean_if_episode_was_replaced()
        {
            GivenOldFiles();
            Subject.OnDownload(_downloadMessage);
            Subject.ProcessQueue();

            Mocker.GetMock<IXbmcService>().Verify(v => v.Clean(It.IsAny<XbmcSettings>()), Times.Once());
        }
    }
}
