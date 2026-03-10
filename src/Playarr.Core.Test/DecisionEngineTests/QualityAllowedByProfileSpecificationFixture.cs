using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Datastore;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class QualityAllowedByProfileSpecificationFixture : CoreTest<QualityAllowedByProfileSpecification>
    {
        private RemoteRom _remoteRom;

        public static object[] AllowedTestCases =
        {
            new object[] { Quality.DVD },
            new object[] { Quality.HDTV720p },
            new object[] { Quality.Bluray1080p }
        };

        public static object[] DeniedTestCases =
        {
            new object[] { Quality.SDTV },
            new object[] { Quality.WEBDL720p },
            new object[] { Quality.Bluray720p }
        };

        [SetUp]
        public void Setup()
        {
            var fakeSeries = Builder<Game>.CreateNew()
                         .With(c => c.QualityProfile = (LazyLoaded<QualityProfile>)new QualityProfile { Cutoff = Quality.Bluray1080p.Id })
                         .Build();

            _remoteRom = new RemoteRom
            {
                Game = fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
            };
        }

        [Test]
        [TestCaseSource(nameof(AllowedTestCases))]
        public void should_allow_if_quality_is_defined_in_profile(Quality qualityType)
        {
            _remoteRom.ParsedRomInfo.Quality.Quality = qualityType;
            _remoteRom.Game.QualityProfile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        [TestCaseSource(nameof(DeniedTestCases))]
        public void should_not_allow_if_quality_is_not_defined_in_profile(Quality qualityType)
        {
            _remoteRom.ParsedRomInfo.Quality.Quality = qualityType;
            _remoteRom.Game.QualityProfile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }
    }
}
