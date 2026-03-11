using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class GetSceneNameFixture : CoreTest
    {
        private LocalEpisode _localRom;
        private string _seasonName = "game.title.s02.dvdrip.x264-ingot";
        private string _episodeName = "game.title.s02e23.dvdrip.x264-ingot";

        [SetUp]
        public void Setup()
        {
            var game = Builder<Game>.CreateNew()
                                        .With(e => e.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                        .With(s => s.Path = @"C:\Test\TV\Game Title".AsOsAgnostic())
                                        .Build();

            var rom = Builder<Rom>.CreateNew()
                                          .Build();

            _localRom = new LocalEpisode
                            {
                                Game = game,
                                Roms = new List<Rom> { rom },
                                Path = Path.Combine(game.Path, "Game Title - S02E23 - Rom Title.mkv"),
                                Quality = new QualityModel(Quality.Bluray720p),
                                ReleaseGroup = "DRONE"
                            };
        }

        private void GivenExistingFileOnDisk()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(new List<RomFile>());
        }

        [Test]
        public void should_use_download_client_item_title_as_scene_name()
        {
            _localRom.DownloadClientRomInfo = new ParsedRomInfo
                                                      {
                                                          ReleaseTitle = _episodeName
                                                      };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .Be(_episodeName);
        }

        [Test]
        public void should_not_use_download_client_item_title_as_scene_name_if_full_season()
        {
            _localRom.DownloadClientRomInfo = new ParsedRomInfo
                                                      {
                                                          ReleaseTitle = _seasonName,
                                                          FullSeason = true
                                                      };

            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _seasonName, _episodeName)
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_download_client_item_title_as_scene_name_if_there_are_other_video_files()
        {
            _localRom.OtherVideoFiles = true;
            _localRom.DownloadClientRomInfo = new ParsedRomInfo
                                                      {
                                                          ReleaseTitle = _seasonName,
                                                          FullSeason = false
                                                      };

            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _seasonName, _episodeName)
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_use_file_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _seasonName, _episodeName + ".mkv")
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .Be(_episodeName);
        }

        [Test]
        public void should_not_use_file_name_as_scenename_if_it_doesnt_look_like_scenename()
        {
            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _episodeName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_use_folder_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _localRom.FolderRomInfo = new ParsedRomInfo
                                              {
                                                  ReleaseTitle = _episodeName
                                              };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .Be(_episodeName);
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_doesnt_look_like_scenename()
        {
            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _episodeName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localRom.FolderRomInfo = new ParsedRomInfo
                                              {
                                                  ReleaseTitle = "aaaaa"
                                              };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_is_for_a_full_season()
        {
            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _episodeName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localRom.FolderRomInfo = new ParsedRomInfo
                                              {
                                                  ReleaseTitle = _seasonName,
                                                  FullSeason = true
                                              };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_is_for_batch()
        {
            var batchName = "[HorribleSubs] Game Title (01-62) [1080p] (Batch)";

            _localRom.DownloadClientRomInfo = new ParsedRomInfo
                                                      {
                                                          FullSeason = false,
                                                          ReleaseTitle = batchName
                                                      };

            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", batchName, "[HorribleSubs] Game Title - 14 [1080p].mkv")
                                     .AsOsAgnostic();

            _localRom.OtherVideoFiles = true;

            _localRom.FolderRomInfo = new ParsedRomInfo
                                              {
                                                  ReleaseTitle = _seasonName,
                                                  FullSeason = false
                                              };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_there_are_other_video_files()
        {
            _localRom.OtherVideoFiles = true;
            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _episodeName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localRom.FolderRomInfo = new ParsedRomInfo
                                              {
                                                  ReleaseTitle = _seasonName,
                                                  FullSeason = false
                                              };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [TestCase(".nsp")]
        [TestCase(".par2")]
        [TestCase(".nzb")]
        public void should_remove_extension_from_nzb_title_for_scene_name(string extension)
        {
            _localRom.DownloadClientRomInfo = new ParsedRomInfo
                                                      {
                                                          ReleaseTitle = _episodeName + extension
                                                      };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .Be(_episodeName);
        }
    }
}
