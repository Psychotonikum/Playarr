using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.MediaFiles.EpisodeImport.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class FullSeasonSpecificationFixture : CoreTest<FullSeasonSpecification>
    {
        private LocalEpisode _localRom;

        [SetUp]
        public void Setup()
        {
            _localRom = new LocalEpisode
            {
                Path = @"C:\Test\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                Size = 100,
                Game = Builder<Game>.CreateNew().Build(),
                FileRomInfo = new ParsedRomInfo
                                    {
                                        FullSeason = false
                                    }
            };
        }

        [Test]
        public void should_return_true_if_no_fileinfo_available()
        {
            _localRom.FileRomInfo = null;
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_file_contains_the_full_season()
        {
            _localRom.FileRomInfo.FullSeason = true;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_file_does_not_contain_the_full_season()
        {
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }
    }
}
