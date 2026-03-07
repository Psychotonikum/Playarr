using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.MediaFiles.EpisodeImport.Specifications;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class HasAudioTrackSpecificationFixture : CoreTest<HasAudioTrackSpecification>
    {
        private Game _series;
        private LocalEpisode _localRom;
        private string _rootFolder;

        [SetUp]
        public void Setup()
        {
             _rootFolder = @"C:\Test\TV".AsOsAgnostic();

             _series = Builder<Game>.CreateNew()
                                     .With(s => s.SeriesType = GameTypes.Standard)
                                     .With(s => s.Path = Path.Combine(_rootFolder, "30 Rock"))
                                     .Build();

             var roms = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .Build()
                                           .ToList();

             _localRom = new LocalEpisode
                                {
                                    Path = @"C:\Test\Unsorted\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                                    Roms = roms,
                                    Game = _series
                                };
        }

        [Test]
        public void should_accept_if_media_info_is_null()
        {
            _localRom.MediaInfo = null;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_if_audio_stream_count_is_0()
        {
            _localRom.MediaInfo = Builder<MediaInfoModel>.CreateNew()
                .With(m => m.AudioStreams = [])
                .Build();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_if_audio_stream_count_is_0()
        {
            _localRom.MediaInfo = Builder<MediaInfoModel>.CreateNew()
                .With(m => m.AudioStreams =
                [
                    new MediaInfoAudioStreamModel { Language = "eng" },
                ])
                .Build();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }
    }
}
