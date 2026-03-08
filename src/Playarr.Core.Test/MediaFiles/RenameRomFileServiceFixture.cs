using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Commands;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.MediaFiles
{
    public class RenameRomFileServiceFixture : CoreTest<RenameRomFileService>
    {
        private Game _series;
        private List<RomFile> _romFiles;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .Build();

            _romFiles = Builder<RomFile>.CreateListOfSize(2)
                                                .All()
                                                .With(e => e.GameId = _series.Id)
                                                .With(e => e.PlatformNumber = 1)
                                                .Build()
                                                .ToList();

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetSeries(_series.Id))
                  .Returns(_series);
        }

        private void GivenNoRomFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Get(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<RomFile>());
        }

        private void GivenRomFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Get(It.IsAny<IEnumerable<int>>()))
                  .Returns(_romFiles);
        }

        private void GivenMovedFiles()
        {
            Mocker.GetMock<IMoveRomFiles>()
                  .Setup(s => s.MoveRomFile(It.IsAny<RomFile>(), _series));
        }

        [Test]
        public void should_not_publish_event_if_no_files_to_rename()
        {
            GivenNoRomFiles();

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_not_publish_event_if_no_files_are_renamed()
        {
            GivenRomFiles();

            Mocker.GetMock<IMoveRomFiles>()
                  .Setup(s => s.MoveRomFile(It.IsAny<RomFile>(), It.IsAny<Game>()))
                  .Throws(new SameFilenameException("Same file name", "Filename"));

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_publish_event_if_files_are_renamed()
        {
            GivenRomFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Once());
        }

        [Test]
        public void should_update_moved_files()
        {
            GivenRomFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1 }));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<RomFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_get_episodefiles_by_ids_only()
        {
            GivenRomFiles();
            GivenMovedFiles();

            var files = new List<int> { 1 };

            Subject.Execute(new RenameFilesCommand(_series.Id, files));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Get(files), Times.Once());
        }
    }
}
