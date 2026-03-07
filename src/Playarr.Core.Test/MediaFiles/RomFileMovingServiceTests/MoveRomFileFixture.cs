using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.CustomFormats;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;
using Playarr.Core.RootFolders;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.RomFileMovingServiceTests
{
    [TestFixture]
    public class MoveRomFileFixture : CoreTest<RomFileMovingService>
    {
        private Game _series;
        private RomFile _romFile;
        private LocalEpisode _localRom;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\TV\Game".AsOsAgnostic())
                                     .Build();

            _romFile = Builder<RomFile>.CreateNew()
                                               .With(f => f.Path = null)
                                               .With(f => f.RelativePath = @"Platform 1\File.avi")
                                               .Build();

            _localRom = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Game = _series)
                                                 .With(l => l.Roms = Builder<Rom>.CreateListOfSize(1).Build().ToList())
                                                 .Build();

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildFilePath(It.IsAny<List<Rom>>(), It.IsAny<Game>(), It.IsAny<RomFile>(), It.IsAny<string>(), It.IsAny<NamingConfig>(), It.IsAny<List<CustomFormat>>()))
                  .Returns(@"C:\Test\TV\Game\Platform 01\File Name.avi".AsOsAgnostic());

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildSeasonPath(It.IsAny<Game>(), It.IsAny<int>()))
                  .Returns(@"C:\Test\TV\Game\Platform 01".AsOsAgnostic());

            var rootFolder = @"C:\Test\TV\".AsOsAgnostic();

            Mocker.GetMock<IRootFolderService>()
                .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                .Returns(rootFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(rootFolder))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(It.IsAny<string>()))
                  .Returns(true);
        }

        [Test]
        public void should_catch_UnauthorizedAccessException_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<string>()))
                  .Throws<UnauthorizedAccessException>();

            Subject.MoveRomFile(_romFile, _localRom);
        }

        [Test]
        public void should_catch_InvalidOperationException_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<string>()))
                  .Throws<InvalidOperationException>();

            Subject.MoveRomFile(_romFile, _localRom);
        }

        [Test]
        public void should_notify_on_series_folder_creation()
        {
            Subject.MoveRomFile(_romFile, _localRom);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<EpisodeFolderCreatedEvent>(It.Is<EpisodeFolderCreatedEvent>(p =>
                      p.GameFolder.IsNotNullOrWhiteSpace())),
                      Times.Once());
        }

        [Test]
        public void should_notify_on_season_folder_creation()
        {
            Subject.MoveRomFile(_romFile, _localRom);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<EpisodeFolderCreatedEvent>(It.Is<EpisodeFolderCreatedEvent>(p =>
                      p.PlatformFolder.IsNotNullOrWhiteSpace())),
                      Times.Once());
        }

        [Test]
        public void should_not_notify_if_series_folder_already_exists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_series.Path))
                  .Returns(true);

            Subject.MoveRomFile(_romFile, _localRom);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<EpisodeFolderCreatedEvent>(It.Is<EpisodeFolderCreatedEvent>(p =>
                      p.GameFolder.IsNotNullOrWhiteSpace())),
                      Times.Never());
        }
    }
}
