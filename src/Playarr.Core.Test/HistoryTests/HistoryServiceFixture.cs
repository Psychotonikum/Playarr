using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Core.Download;
using Playarr.Core.History;
using Playarr.Core.Indexers;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Test.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.Test.HistoryTests
{
    public class HistoryServiceFixture : CoreTest<HistoryService>
    {
        private QualityProfile _profile;
        private QualityProfile _profileCustom;

        [SetUp]
        public void Setup()
        {
            _profile = new QualityProfile
                {
                    Cutoff = Quality.WEBDL720p.Id,
                    Items = QualityFixture.GetDefaultQualities(),
                };

            _profileCustom = new QualityProfile
                {
                    Cutoff = Quality.WEBDL720p.Id,
                    Items = QualityFixture.GetDefaultQualities(Quality.DVD),
                };
        }

        [Test]
        public void should_use_file_name_for_source_title_if_scene_name_is_null()
        {
            var game = Builder<Game>.CreateNew().Build();
            var roms = Builder<Rom>.CreateListOfSize(1).Build().ToList();
            var romFile = Builder<RomFile>.CreateNew()
                                                  .With(f => f.SceneName = null)
                                                  .Build();

            var localRom = new LocalEpisode
                               {
                                   Game = game,
                                   Roms = roms,
                                   Path = @"C:\Test\Unsorted\Game.s01e01.mkv"
                               };

            var downloadClientItem = new DownloadClientItem
                                     {
                                         DownloadClientInfo = new DownloadClientItemClientInfo
                                         {
                                             Protocol = DownloadProtocol.Usenet,
                                             Id = 1,
                                             Name = "sab"
                                         },
                                         DownloadId = "abcd"
                                     };

            Subject.Handle(new EpisodeImportedEvent(localRom, romFile, new List<DeletedRomFile>(), true, downloadClientItem));

            Mocker.GetMock<IHistoryRepository>()
                .Verify(v => v.Insert(It.Is<EpisodeHistory>(h => h.SourceTitle == Path.GetFileNameWithoutExtension(localRom.Path))));
        }
    }
}
