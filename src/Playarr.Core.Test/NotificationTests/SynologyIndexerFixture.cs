using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Playarr.Core.MediaFiles;
using Playarr.Core.Notifications;
using Playarr.Core.Notifications.Synology;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.NotificationTests
{
    [TestFixture]
    public class SynologyIndexerFixture : CoreTest<SynologyIndexer>
    {
        private Game _series;
        private DownloadMessage _upgrade;

        [SetUp]
        public void SetUp()
        {
            _series = new Game()
            {
                Path = @"C:\Test\".AsOsAgnostic()
            };

            _upgrade = new DownloadMessage()
            {
                Game = _series,

                RomFile = new RomFile
                {
                    RelativePath = "file1.S01E01E02.mkv"
                },

                OldFiles = new List<DeletedRomFile>
                {
                    new DeletedRomFile(new RomFile
                    {
                        RelativePath = "file1.S01E01.mkv"
                    },
                        null),
                    new DeletedRomFile(new RomFile
                    {
                        RelativePath = "file1.S01E02.mkv"
                    },
                        null)
                }
            };

            Subject.Definition = new NotificationDefinition
            {
                Settings = new SynologyIndexerSettings
                {
                   UpdateLibrary = true
                }
            };
        }

        [Test]
        public void should_not_update_library_if_disabled()
        {
            (Subject.Definition.Settings as SynologyIndexerSettings).UpdateLibrary = false;

            Subject.OnRename(_series, new List<RenamedRomFile>());

            Mocker.GetMock<ISynologyIndexerProxy>()
                .Verify(v => v.UpdateFolder(_series.Path), Times.Never());
        }

        [Test]
        public void should_remove_old_episodes_on_upgrade()
        {
            Subject.OnDownload(_upgrade);

            Mocker.GetMock<ISynologyIndexerProxy>()
                .Verify(v => v.DeleteFile(@"C:\Test\file1.S01E01.mkv".AsOsAgnostic()), Times.Once());

            Mocker.GetMock<ISynologyIndexerProxy>()
                .Verify(v => v.DeleteFile(@"C:\Test\file1.S01E02.mkv".AsOsAgnostic()), Times.Once());
        }

        [Test]
        public void should_add_new_episode_on_upgrade()
        {
            Subject.OnDownload(_upgrade);

            Mocker.GetMock<ISynologyIndexerProxy>()
                .Verify(v => v.AddFile(@"C:\Test\file1.S01E01E02.mkv".AsOsAgnostic()), Times.Once());
        }

        [Test]
        public void should_update_entire_series_folder_on_rename()
        {
            Subject.OnRename(_series, new List<RenamedRomFile>());

            Mocker.GetMock<ISynologyIndexerProxy>()
                .Verify(v => v.UpdateFolder(@"C:\Test\".AsOsAgnostic()), Times.Once());
        }
    }
}
