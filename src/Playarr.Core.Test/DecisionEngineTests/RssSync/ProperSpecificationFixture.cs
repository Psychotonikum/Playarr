using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Configuration;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.DecisionEngine.Specifications.RssSync;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]

    public class ProperSpecificationFixture : CoreTest<ProperSpecification>
    {
        private RemoteRom _parseResultMulti;
        private RemoteRom _parseResultSingle;
        private RomFile _firstFile;
        private RomFile _secondFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _firstFile = new RomFile { Quality = new QualityModel(Quality.Verified, new Revision(version: 1)), DateAdded = DateTime.Now };
            _secondFile = new RomFile { Quality = new QualityModel(Quality.Verified, new Revision(version: 1)), DateAdded = DateTime.Now };

            var singleEpisodeList = new List<Rom> { new Rom { RomFile = _firstFile, EpisodeFileId = 1 }, new Rom { RomFile = null } };
            var doubleEpisodeList = new List<Rom> { new Rom { RomFile = _firstFile, EpisodeFileId = 1 }, new Rom { RomFile = _secondFile, EpisodeFileId = 1 }, new Rom { RomFile = null } };

            var fakeSeries = Builder<Game>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Verified.Id })
                         .Build();

            _parseResultMulti = new RemoteRom
            {
                Game = fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.Bad, new Revision(version: 2)) },
                Roms = doubleEpisodeList
            };

            _parseResultSingle = new RemoteRom
            {
                Game = fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.Bad, new Revision(version: 2)) },
                Roms = singleEpisodeList
            };
        }

        private void WithFirstFileUpgradable()
        {
            _firstFile.Quality = new QualityModel(Quality.Unknown);
        }

        [Test]
        public void should_return_false_when_romFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.Bad;

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_first_romFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.Bad;
            _secondFile.Quality.Quality = Quality.Bad;

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_second_romFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.Bad;
            _secondFile.Quality.Quality = Quality.Bad;

            _secondFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_romFile_was_added_more_than_7_days_ago_but_proper_is_for_better_quality()
        {
            WithFirstFileUpgradable();

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_romFile_was_added_more_than_7_days_ago_but_is_for_search()
        {
            WithFirstFileUpgradable();

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_proper_but_auto_download_propers_is_false()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotUpgrade);

            _firstFile.Quality.Quality = Quality.Bad;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_romFile_was_added_today()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _firstFile.Quality.Quality = Quality.Bad;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_propers_are_not_preferred()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _firstFile.Quality.Quality = Quality.Bad;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }
    }
}
