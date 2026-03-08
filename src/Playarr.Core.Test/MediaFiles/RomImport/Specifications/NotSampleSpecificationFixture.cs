using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.MediaFiles.EpisodeImport.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class NotSampleSpecificationFixture : CoreTest<NotSampleSpecification>
    {
        private Game _series;
        private LocalEpisode _localRom;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.SeriesType = GameTypes.Standard)
                                     .Build();

            var roms = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.PlatformNumber = 1)
                                           .Build()
                                           .ToList();

            _localRom = new LocalEpisode
                                {
                                    Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                                    Roms = roms,
                                    Game = _series,
                                    Quality = new QualityModel(Quality.HDTV720p)
                                };
        }

        [Test]
        public void should_return_true_for_existing_file()
        {
            _localRom.ExistingFile = true;
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }
    }
}
