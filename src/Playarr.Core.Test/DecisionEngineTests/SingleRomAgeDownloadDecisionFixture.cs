using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class SingleEpisodeAgeDownloadDecisionFixture : CoreTest<SeasonPackOnlySpecification>
    {
        private RemoteRom _parseResultMulti;
        private RemoteRom _parseResultSingle;
        private Game _series;
        private List<Rom> _episodes;
        private SeasonSearchCriteria _multiSearch;
        private ReleaseDecisionInformation _multiInfo;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                    .With(s => s.Platforms = Builder<Platform>.CreateListOfSize(1).Build().ToList())
                                    .With(s => s.SeriesType = GameTypes.Standard)
                                    .Build();

            _episodes = new List<Rom>();
            _episodes.Add(CreateEpisodeStub(1, 400));
            _episodes.Add(CreateEpisodeStub(2, 370));
            _episodes.Add(CreateEpisodeStub(3, 340));
            _episodes.Add(CreateEpisodeStub(4, 310));

            _multiSearch = new SeasonSearchCriteria();
            _multiSearch.Roms = _episodes.ToList();
            _multiSearch.PlatformNumber = 1;
            _multiInfo = new ReleaseDecisionInformation(false, _multiSearch);

            _parseResultMulti = new RemoteRom
            {
                Game = _series,
                Release = new ReleaseInfo(),
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)), FullSeason = true },
                Roms = _episodes.ToList()
            };

            _parseResultSingle = new RemoteRom
            {
                Game = _series,
                Release = new ReleaseInfo(),
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                Roms = new List<Rom>()
            };
        }

        private Rom CreateEpisodeStub(int number, int age)
        {
            return new Rom()
                   {
                        PlatformNumber = 1,
                        EpisodeNumber = number,
                        AirDateUtc = DateTime.UtcNow.AddDays(-age)
                   };
        }

        [TestCase(1, 200, false)]
        [TestCase(4, 200, false)]
        [TestCase(1, 600, true)]
        [TestCase(1, 365, true)]
        [TestCase(4, 365, true)]
        [TestCase(1, 0, true)]
        public void single_episode_release(int rom, int seasonSearchMaximumSingleEpisodeAge, bool expectedResult)
        {
            _parseResultSingle.Release.SeasonSearchMaximumSingleEpisodeAge = seasonSearchMaximumSingleEpisodeAge;
            _parseResultSingle.Roms.Clear();
            _parseResultSingle.Roms.Add(_episodes.Find(e => e.EpisodeNumber == rom));

            Subject.IsSatisfiedBy(_parseResultSingle, _multiInfo).Accepted.Should().Be(expectedResult);
        }

        // should always accept all platform packs
        [TestCase(200, true)]
        [TestCase(600, true)]
        [TestCase(365, true)]
        [TestCase(0, true)]
        public void multi_episode_release(int seasonSearchMaximumSingleEpisodeAge, bool expectedResult)
        {
            _parseResultMulti.Release.SeasonSearchMaximumSingleEpisodeAge = seasonSearchMaximumSingleEpisodeAge;

            Subject.IsSatisfiedBy(_parseResultMulti, _multiInfo).Accepted.Should().BeTrue();
        }
    }
}
