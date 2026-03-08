using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Core.Organizer;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Core.Games.Commands;
using Playarr.Test.Common;

namespace Playarr.Core.Test.TvTests
{
    [TestFixture]
    public class MoveGameServiceFixture : CoreTest<MoveGameService>
    {
        private Game _series;
        private MoveGameCommand _command;
        private BulkMoveGameCommand _bulkCommand;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                .CreateNew()
                .Build();

            _command = new MoveGameCommand
                       {
                           GameId = 1,
                           SourcePath = @"C:\Test\TV\Game".AsOsAgnostic(),
                           DestinationPath = @"C:\Test\TV2\Game".AsOsAgnostic()
                       };

            _bulkCommand = new BulkMoveGameCommand
                       {
                           Game = new List<BulkMoveGame>
                                    {
                                        new BulkMoveGame
                                        {
                                            GameId = 1,
                                            SourcePath = @"C:\Test\TV\Game".AsOsAgnostic()
                                        }
                                    },
                           DestinationRootFolder = @"C:\Test\TV2".AsOsAgnostic()
                       };

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetSeries(It.IsAny<int>()))
                  .Returns(_series);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);
        }

        private void GivenFailedMove()
        {
            Mocker.GetMock<IDiskTransferService>()
                  .Setup(s => s.TransferFolder(It.IsAny<string>(), It.IsAny<string>(), TransferMode.Move))
                  .Throws<IOException>();
        }

        [Test]
        public void should_log_error_when_move_throws_an_exception()
        {
            GivenFailedMove();

            Subject.Execute(_command);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_revert_series_path_on_error()
        {
            GivenFailedMove();

            Subject.Execute(_command);

            ExceptionVerification.ExpectedErrors(1);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Game>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_use_destination_path()
        {
            Subject.Execute(_command);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_command.SourcePath, _command.DestinationPath, TransferMode.Move), Times.Once());

            Mocker.GetMock<IBuildFileNames>()
                  .Verify(v => v.GetGameFolder(It.IsAny<Game>(), null), Times.Never());
        }

        [Test]
        public void should_build_new_path_when_root_folder_is_provided()
        {
            var seriesFolder = "Game";
            var expectedPath = Path.Combine(_bulkCommand.DestinationRootFolder, seriesFolder);

            Mocker.GetMock<IBuildFileNames>()
                    .Setup(s => s.GetGameFolder(It.IsAny<Game>(), null))
                    .Returns(seriesFolder);

            Subject.Execute(_bulkCommand);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_bulkCommand.Game.First().SourcePath, expectedPath, TransferMode.Move), Times.Once());
        }

        [Test]
        public void should_skip_series_folder_if_it_does_not_exist()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);

            Subject.Execute(_command);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_command.SourcePath, _command.DestinationPath, TransferMode.Move), Times.Never());

            Mocker.GetMock<IBuildFileNames>()
                  .Verify(v => v.GetGameFolder(It.IsAny<Game>(), null), Times.Never());
        }
    }
}
