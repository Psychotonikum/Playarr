using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.RootFolders;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.DiskScanServiceTests
{
    [TestFixture]
    public class ScanFixture : CoreTest<DiskScanService>
    {
        private Game _series;
        private string _rootFolder;
        private string _otherGameFolder;

        [SetUp]
        public void Setup()
        {
            _rootFolder = @"C:\Test\TV".AsOsAgnostic();
            _otherGameFolder = @"C:\Test\TV\OtherSeries".AsOsAgnostic();
            var seriesFolder = @"C:\Test\TV\Game".AsOsAgnostic();

            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Path = seriesFolder)
                                     .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(It.IsAny<string>()))
                  .Returns((string path) => Directory.GetParent(path).FullName);

            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                  .Returns(_rootFolder);

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesBySeries(It.IsAny<int>()))
                  .Returns(new List<RomFile>());
        }

        private void GivenRootFolder(params string[] subfolders)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_rootFolder))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(_rootFolder))
                  .Returns(subfolders);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderEmpty(_rootFolder))
                  .Returns(subfolders.Empty());

            foreach (var folder in subfolders)
            {
                Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(folder))
                  .Returns(true);
            }
        }

        private void GivenGameFolder()
        {
            GivenRootFolder(_series.Path);
        }

        private void GivenFiles(IEnumerable<string> files)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(It.IsAny<string>(), true))
                  .Returns(files.ToArray());
        }

        [Test]
        public void should_not_scan_if_root_folder_does_not_exist()
        {
            Subject.Scan(_series);

            ExceptionVerification.ExpectedWarns(1);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetFiles(_series.Path, true), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.CreateFolder(_series.Path), Times.Never());

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Game>(), It.IsAny<List<string>>()), Times.Never());
        }

        [Test]
        public void should_not_scan_if_series_root_folder_is_empty()
        {
            GivenRootFolder();

            Subject.Scan(_series);

            ExceptionVerification.ExpectedWarns(1);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetFiles(_series.Path, true), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.CreateFolder(_series.Path), Times.Never());

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Game>(), It.IsAny<List<string>>()), Times.Never());

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.IsAny<List<string>>(), _series, false), Times.Never());
        }

        [Test]
        public void should_create_if_series_folder_does_not_exist_but_create_folder_enabled()
        {
            GivenRootFolder(_otherGameFolder);

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.CreateEmptyGameFolders)
                  .Returns(true);

            Subject.Scan(_series);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.CreateFolder(_series.Path), Times.Once());
        }

        [Test]
        public void should_not_create_if_series_folder_does_not_exist_and_create_folder_disabled()
        {
            GivenRootFolder(_otherGameFolder);

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.CreateEmptyGameFolders)
                  .Returns(false);

            Subject.Scan(_series);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.CreateFolder(_series.Path), Times.Never());
        }

        [Test]
        public void should_clean_but_not_import_if_series_folder_does_not_exist()
        {
            GivenRootFolder(_otherGameFolder);

            Subject.Scan(_series);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.FolderExists(_series.Path), Times.Once());

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Game>(), It.IsAny<List<string>>()), Times.Once());

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.IsAny<List<string>>(), _series, false), Times.Never());
        }

        [Test]
        public void should_not_scan_various_extras_subfolders()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "Behind the Scenes", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Deleted Scenes", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Featurettes", "file3.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Interviews", "file4.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Samples", "file5.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Scenes", "file6.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Shorts", "file7.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Trailers", "file8.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Other", "file9.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Game Title S01E01 (1080p BluRay x265 10bit Tigole).mkv").AsOsAgnostic(),
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_not_scan_featurettes_subfolders()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "Featurettes", "An Epic Reborn.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Featurettes", "Deleted & Alternate Scenes.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Featurettes", "En Garde - Multi-Angle Dailies.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Featurettes", "Layer-By-Layer - Sound Design - Multiple Audio.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Game Title S01E01 (1080p BluRay x265 10bit Tigole).mkv").AsOsAgnostic(),
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_clean_but_not_import_if_series_folder_does_not_exist_and_create_folder_enabled()
        {
            GivenRootFolder(_otherGameFolder);

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.CreateEmptyGameFolders)
                  .Returns(true);

            Subject.Scan(_series);

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Game>(), It.IsAny<List<string>>()), Times.Once());

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.IsAny<List<string>>(), _series, false), Times.Never());
        }

        [Test]
        public void should_find_files_at_root_of_series_folder()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 2), _series, false), Times.Once());
        }

        [Test]
        public void should_not_scan_extras_subfolder()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "EXTRAS", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Extras", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "EXTRAs", "file3.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "ExTrAs", "file4.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetFiles(It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(2));

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_not_scan_AppleDouble_subfolder()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, ".AppleDouble", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, ".appledouble", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_scan_extras_series_and_subfolders()
        {
            _series.Path = @"C:\Test\TV\Extras".AsOsAgnostic();

            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "Extras", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, ".AppleDouble", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", "s01e01.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", "s01e02.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 2", "s02e01.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 2", "s02e02.mkv").AsOsAgnostic(),
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 4), _series, false), Times.Once());
        }

        [Test]
        public void should_not_scan_subfolders_that_start_with_period()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, ".@__THUMB", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, ".hidden", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_scan_files_that_start_with_period()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "Platform 1", ".s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_not_scan_subfolder_of_season_folder_that_starts_with_a_period()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "Platform 1", ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", ".@__THUMB", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", ".hidden", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", ".AppleDouble", "s01e01.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_not_scan_Synology_eaDir()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "@eaDir", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_not_scan_thumb_folder()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_scan_dotHack_folder()
        {
            _series.Path = @"C:\Test\TV\.hack".AsOsAgnostic();

            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "Platform 1", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Platform 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 2), _series, false), Times.Once());
        }

        [Test]
        public void should_exclude_inline_extra_files()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "Game Title S01E01.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Deleted Scenes-deleted.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "The World of Pandora-other.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_exclude_osx_metadata_files()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "._24 The Status Quo Combustion.mp4").AsOsAgnostic(),
                           Path.Combine(_series.Path, "24 The Status Quo Combustion.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());
        }

        [Test]
        public void should_not_scan_excluded_files()
        {
            GivenGameFolder();

            GivenFiles(new List<string>
            {
                Path.Combine(_series.Path, ".DS_Store").AsOsAgnostic(),
                Path.Combine(_series.Path, ".unmanic").AsOsAgnostic(),
                Path.Combine(_series.Path, ".unmanic.part").AsOsAgnostic(),
                Path.Combine(_series.Path, "24 The Status Quo Combustion.mkv").AsOsAgnostic()
            });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.Is<SeriesScannedEvent>(c => c.Game != null && c.PossibleExtraFiles.Count == 0)), Times.Once());
        }
    }
}
