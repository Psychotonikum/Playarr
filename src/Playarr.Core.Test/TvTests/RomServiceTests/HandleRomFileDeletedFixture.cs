using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Datastore;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.TvTests.RomServiceTests
{
    [TestFixture]
    public class HandleRomFileDeletedFixture : CoreTest<RomService>
    {
        private Game _series;
        private RomFile _romFile;
        private List<Rom> _episodes;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                .CreateNew()
                .Build();

            _romFile = Builder<RomFile>
                .CreateNew()
                .With(e => e.GameId = _series.Id)
                .Build();
        }

        private void GivenSingleRomFile()
        {
            _episodes = Builder<Rom>
                .CreateListOfSize(1)
                .All()
                .With(e => e.GameId = _series.Id)
                .With(e => e.Monitored = true)
                .Build()
                .ToList();

            Mocker.GetMock<IRomRepository>()
                  .Setup(s => s.GetEpisodeByFileId(_romFile.Id))
                  .Returns(_episodes);
        }

        private void GivenMultiRomFile()
        {
            _episodes = Builder<Rom>
                .CreateListOfSize(2)
                .All()
                .With(e => e.GameId = _series.Id)
                .With(e => e.Monitored = true)
                .Build()
                .ToList();

            Mocker.GetMock<IRomRepository>()
                  .Setup(s => s.GetEpisodeByFileId(_romFile.Id))
                  .Returns(_episodes);
        }

        [Test]
        public void should_set_RomFileId_to_zero()
        {
            GivenSingleRomFile();

            Subject.Handle(new RomFileDeletedEvent(_romFile, DeleteMediaFileReason.MissingFromDisk));

            Mocker.GetMock<IRomRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Rom>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_update_each_episode_for_file()
        {
            GivenMultiRomFile();

            Subject.Handle(new RomFileDeletedEvent(_romFile, DeleteMediaFileReason.MissingFromDisk));

            Mocker.GetMock<IRomRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Rom>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [Test]
        public void should_set_monitored_to_false_if_autoUnmonitor_is_true_and_is_not_for_an_upgrade()
        {
            GivenSingleRomFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(true);

            Subject.Handle(new RomFileDeletedEvent(_romFile, DeleteMediaFileReason.MissingFromDisk));
            Subject.HandleAsync(new SeriesScannedEvent(_series, new List<string>()));

            Mocker.GetMock<IRomRepository>()
                .Verify(v => v.SetMonitored(It.IsAny<IEnumerable<int>>(), false), Times.Once());
        }

        [Test]
        public void should_leave_monitored_if_autoUnmonitor_is_true_and_missing_episode_is_replaced()
        {
            GivenSingleRomFile();

            var newRomFile = _romFile.JsonClone();
            newRomFile.Id = 123;
            newRomFile.Roms = new LazyLoaded<List<Rom>>(_episodes);

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                .Returns(true);

            Subject.Handle(new RomFileDeletedEvent(_romFile, DeleteMediaFileReason.MissingFromDisk));
            Subject.Handle(new RomFileAddedEvent(newRomFile));
            Subject.HandleAsync(new SeriesScannedEvent(_series, new List<string>()));

            Mocker.GetMock<IRomRepository>()
                .Verify(v => v.SetMonitored(It.IsAny<IEnumerable<int>>(), false), Times.Never());
        }

        [Test]
        public void should_leave_monitored_to_true_if_autoUnmonitor_is_false()
        {
            GivenSingleRomFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(false);

            Subject.Handle(new RomFileDeletedEvent(_romFile, DeleteMediaFileReason.Upgrade));

            Mocker.GetMock<IRomRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Rom>(), false), Times.Once());
        }

        [Test]
        public void should_leave_monitored_to_true_if_autoUnmonitor_is_true_and_is_for_an_upgrade()
        {
            GivenSingleRomFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(true);

            Subject.Handle(new RomFileDeletedEvent(_romFile, DeleteMediaFileReason.Upgrade));

            Mocker.GetMock<IRomRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Rom>(), false), Times.Once());
        }

        [Test]
        public void should_leave_monitored_to_true_if_autoUnmonitor_is_true_and_is_for_manual_override()
        {
            GivenSingleRomFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(true);

            Subject.Handle(new RomFileDeletedEvent(_romFile, DeleteMediaFileReason.ManualOverride));

            Mocker.GetMock<IRomRepository>()
                  .Verify(v => v.ClearFileId(It.IsAny<Rom>(), false), Times.Once());
        }
    }
}
