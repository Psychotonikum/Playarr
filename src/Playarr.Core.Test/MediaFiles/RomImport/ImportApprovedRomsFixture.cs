using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.History;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class ImportApprovedEpisodesFixture : CoreTest<ImportApprovedEpisodes>
    {
        private List<ImportDecision> _rejectedDecisions;
        private List<ImportDecision> _approvedDecisions;

        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _rejectedDecisions = new List<ImportDecision>();
            _approvedDecisions = new List<ImportDecision>();

            var outputPath = @"C:\Test\Unsorted\TV\30.Rock.S01E01".AsOsAgnostic();

            var game = Builder<Game>.CreateNew()
                                        .With(e => e.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                        .With(s => s.Path = @"C:\Test\TV\30 Rock".AsOsAgnostic())
                                        .Build();

            var roms = Builder<Rom>.CreateListOfSize(5)
                                           .Build();

            _rejectedDecisions.Add(new ImportDecision(new LocalEpisode(), new ImportRejection(ImportRejectionReason.Unknown, "Rejected!")));
            _rejectedDecisions.Add(new ImportDecision(new LocalEpisode(), new ImportRejection(ImportRejectionReason.Unknown, "Rejected!")));
            _rejectedDecisions.Add(new ImportDecision(new LocalEpisode(), new ImportRejection(ImportRejectionReason.Unknown, "Rejected!")));
            _rejectedDecisions.ForEach(r => r.LocalEpisode.FileRomInfo = new ParsedRomInfo());

            foreach (var rom in roms)
            {
                _approvedDecisions.Add(new ImportDecision(
                                           new LocalEpisode
                                               {
                                                   Game = game,
                                                   Roms = new List<Rom> { rom },
                                                   Path = Path.Combine(game.Path, "30 Rock - S01E01 - Pilot.avi"),
                                                   Quality = new QualityModel(Quality.Bluray720p),
                                                   ReleaseGroup = "DRONE",
                                                   FileRomInfo = new ParsedRomInfo()
                                               }));
            }

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Setup(s => s.UpgradeRomFile(It.IsAny<RomFile>(), It.IsAny<LocalEpisode>(), It.IsAny<bool>()))
                  .Returns(new RomFileMoveResult());

            Mocker.GetMock<IHistoryService>()
                .Setup(x => x.FindByDownloadId(It.IsAny<string>()))
                .Returns(new List<EpisodeHistory>());

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                .With(d => d.OutputPath = new OsPath(outputPath))
                .Build();
        }

        private void GivenNewDownload()
        {
            _approvedDecisions.ForEach(a => a.LocalEpisode.Path = Path.Combine(_downloadClientItem.OutputPath.ToString(), Path.GetFileName(a.LocalEpisode.Path)));
        }

        private void GivenExistingFileOnDisk()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(new List<RomFile>());
        }

        [Test]
        public void should_not_import_any_if_there_are_no_approved_decisions()
        {
            Subject.Import(_rejectedDecisions, false).Where(i => i.Result == ImportResultType.Imported).Should().BeEmpty();

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.IsAny<RomFile>()), Times.Never());
        }

        [Test]
        public void should_import_each_approved()
        {
            GivenExistingFileOnDisk();

            Subject.Import(_approvedDecisions, false).Should().HaveCount(5);
        }

        [Test]
        public void should_only_import_approved()
        {
            GivenExistingFileOnDisk();

            var all = new List<ImportDecision>();
            all.AddRange(_rejectedDecisions);
            all.AddRange(_approvedDecisions);

            var result = Subject.Import(all, false);

            result.Should().HaveCount(all.Count);
            result.Where(i => i.Result == ImportResultType.Imported).Should().HaveCount(_approvedDecisions.Count);
        }

        [Test]
        public void should_only_import_each_episode_once()
        {
            GivenExistingFileOnDisk();

            var all = new List<ImportDecision>();
            all.AddRange(_approvedDecisions);
            all.Add(new ImportDecision(_approvedDecisions.First().LocalEpisode));

            var result = Subject.Import(all, false);

            result.Where(i => i.Result == ImportResultType.Imported).Should().HaveCount(_approvedDecisions.Count);
        }

        [Test]
        public void should_move_new_downloads()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeRomFile(It.IsAny<RomFile>(), _approvedDecisions.First().LocalEpisode, false),
                          Times.Once());
        }

        [Test]
        public void should_publish_EpisodeImportedEvent_for_new_downloads()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.IsAny<EpisodeImportedEvent>()), Times.Once());
        }

        [Test]
        public void should_not_move_existing_files()
        {
            GivenExistingFileOnDisk();

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, false);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeRomFile(It.IsAny<RomFile>(), _approvedDecisions.First().LocalEpisode, false),
                          Times.Never());
        }

        [Test]
        public void should_import_larger_files_first()
        {
            GivenExistingFileOnDisk();

            var fileDecision = _approvedDecisions.First();
            fileDecision.LocalEpisode.Size = 1.Gigabytes();

            var sampleDecision = new ImportDecision(
                new LocalEpisode
                 {
                     Game = fileDecision.LocalEpisode.Game,
                     Roms = new List<Rom> { fileDecision.LocalEpisode.Roms.First() },
                     Path = @"C:\Test\TV\30 Rock\30 Rock - S01E01 - Pilot.avi".AsOsAgnostic(),
                     Quality = new QualityModel(Quality.Bluray720p),
                     Size = 80.Megabytes()
                 });

            var all = new List<ImportDecision>();
            all.Add(fileDecision);
            all.Add(sampleDecision);

            var results = Subject.Import(all, false);

            results.Should().HaveCount(all.Count);
            results.Should().ContainSingle(d => d.Result == ImportResultType.Imported);
            results.Should().ContainSingle(d => d.Result == ImportResultType.Imported && d.ImportDecision.LocalEpisode.Size == fileDecision.LocalEpisode.Size);
        }

        [Test]
        public void should_copy_when_cannot_move_files_downloads()
        {
            GivenNewDownload();
            _downloadClientItem.Title = "30.Rock.S01E01";
            _downloadClientItem.CanMoveFiles = false;

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeRomFile(It.IsAny<RomFile>(), _approvedDecisions.First().LocalEpisode, true), Times.Once());
        }

        [Test]
        public void should_use_override_importmode()
        {
            GivenNewDownload();
            _downloadClientItem.Title = "30.Rock.S01E01";
            _downloadClientItem.CanMoveFiles = false;

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem, ImportMode.Move);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeRomFile(It.IsAny<RomFile>(), _approvedDecisions.First().LocalEpisode, false), Times.Once());
        }

        [Test]
        public void should_use_file_name_only_for_download_client_item_without_a_job_folder()
        {
            var fileName = "Game.Title.S01E01.720p.HDTV.x264-Playarr.mkv";
            var path = Path.Combine(@"C:\Test\Unsorted\TV\".AsOsAgnostic(), fileName);

            _downloadClientItem.OutputPath = new OsPath(path);
            _approvedDecisions.First().LocalEpisode.Path = path;

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<RomFile>(c => c.OriginalFilePath == fileName)));
        }

        [Test]
        public void should_use_folder_and_file_name_only_for_download_client_item_with_a_job_folder()
        {
            var name = "Game.Title.S01E01.720p.HDTV.x264-Playarr";
            var outputPath = Path.Combine(@"C:\Test\Unsorted\TV\".AsOsAgnostic(), name);

            _downloadClientItem.OutputPath = new OsPath(outputPath);
            _approvedDecisions.First().LocalEpisode.Path = Path.Combine(outputPath, name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<RomFile>(c => c.OriginalFilePath == $"{name}\\{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_include_intermediate_folders_for_download_client_item_with_a_job_folder()
        {
            var name = "Game.Title.S01E01.720p.HDTV.x264-Playarr";
            var outputPath = Path.Combine(@"C:\Test\Unsorted\TV\".AsOsAgnostic(), name);

            _downloadClientItem.OutputPath = new OsPath(outputPath);
            _approvedDecisions.First().LocalEpisode.Path = Path.Combine(outputPath, "subfolder", name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<RomFile>(c => c.OriginalFilePath == $"{name}\\subfolder\\{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_use_folder_info_release_title_to_find_relative_path()
        {
            var name = "Game.Title.S01E01.720p.HDTV.x264-Playarr";
            var outputPath = Path.Combine(@"C:\Test\Unsorted\TV\".AsOsAgnostic(), name);
            var localRom = _approvedDecisions.First().LocalEpisode;

            localRom.FolderRomInfo = new ParsedRomInfo { ReleaseTitle = name };
            localRom.Path = Path.Combine(outputPath, "subfolder", name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, null);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<RomFile>(c => c.OriginalFilePath == $"{name}\\subfolder\\{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_get_relative_path_when_there_is_no_grandparent_windows()
        {
            WindowsOnly();

            var name = "Game.Title.S01E01.720p.HDTV.x264-Playarr";
            var outputPath = @"C:\";
            var localRom = _approvedDecisions.First().LocalEpisode;

            localRom.FolderRomInfo = new ParsedRomInfo { ReleaseTitle = name };
            localRom.Path = Path.Combine(outputPath, name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, null);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<RomFile>(c => c.OriginalFilePath == $"{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_get_relative_path_when_there_is_no_grandparent_mono()
        {
            PosixOnly();

            var name = "Game.Title.S01E01.720p.HDTV.x264-Playarr";
            var outputPath = "/";
            var localRom = _approvedDecisions.First().LocalEpisode;

            localRom.FolderRomInfo = new ParsedRomInfo { ReleaseTitle = name };
            localRom.Path = Path.Combine(outputPath, name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, null);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<RomFile>(c => c.OriginalFilePath == $"{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_get_relative_path_when_there_is_no_grandparent_for_UNC_path()
        {
            WindowsOnly();

            var name = "Game.Title.S01E01.720p.HDTV.x264-Playarr";
            var outputPath = @"\\server\share";
            var localRom = _approvedDecisions.First().LocalEpisode;

            localRom.FolderRomInfo = new ParsedRomInfo { ReleaseTitle = name };
            localRom.Path = Path.Combine(outputPath, name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, null);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<RomFile>(c => c.OriginalFilePath == $"{name}.mkv")));
        }

        [Test]
        public void should_use_folder_info_release_title_to_find_relative_path_when_file_is_not_in_download_client_item_output_directory()
        {
            var name = "Game.Title.S01E01.720p.HDTV.x264-Playarr";
            var outputPath = Path.Combine(@"C:\Test\Unsorted\TV\".AsOsAgnostic(), name);
            var localRom = _approvedDecisions.First().LocalEpisode;

            _downloadClientItem.OutputPath = new OsPath(Path.Combine(@"C:\Test\Unsorted\TV-Other\".AsOsAgnostic(), name));
            localRom.FolderRomInfo = new ParsedRomInfo { ReleaseTitle = name };
            localRom.Path = Path.Combine(outputPath, "subfolder", name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<RomFile>(c => c.OriginalFilePath == $"{name}\\subfolder\\{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_delete_existing_metadata_files_with_the_same_path()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(Builder<RomFile>.CreateListOfSize(1).BuildList());

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, false);

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Delete(It.IsAny<RomFile>(), DeleteMediaFileReason.ManualOverride), Times.Once());
        }

        [Test]
        public void should_use_folder_info_release_title_to_find_relative_path_when_download_client_item_has_an_empty_output_path()
        {
            var name = "Game.Title.S01E01.720p.HDTV.x264-Playarr";
            var outputPath = Path.Combine(@"C:\Test\Unsorted\TV\".AsOsAgnostic(), name);
            var localRom = _approvedDecisions.First().LocalEpisode;

            _downloadClientItem.OutputPath = default(OsPath);
            localRom.FolderRomInfo = new ParsedRomInfo { ReleaseTitle = name };
            localRom.Path = Path.Combine(outputPath, "subfolder", name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<RomFile>(c => c.OriginalFilePath == $"{name}\\subfolder\\{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_include_scene_name_with_new_downloads()
        {
            var firstDecision = _approvedDecisions.First();
            firstDecision.LocalEpisode.SceneName = "Game.Title.S01E01.dvdrip-DRONE";

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeRomFile(It.Is<RomFile>(e => e.SceneName == firstDecision.LocalEpisode.SceneName), _approvedDecisions.First().LocalEpisode, false),
                      Times.Once());
        }
    }
}
