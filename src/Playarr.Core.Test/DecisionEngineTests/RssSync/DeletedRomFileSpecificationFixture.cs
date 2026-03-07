using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Core.Configuration;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications.RssSync;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class DeletedRomFileSpecificationFixture : CoreTest<DeletedRomFileSpecification>
    {
        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private RomFile _firstFile;
        private RomFile _secondFile;

        [SetUp]
        public void Setup()
        {
            _firstFile = new RomFile
            {
                Id = 1,
                RelativePath = "My.Game.S01E01.mkv",
                Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                DateAdded = DateTime.Now
            };
            _secondFile = new RomFile
            {
                Id = 2,
                RelativePath = "My.Game.S01E02.mkv",
                Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                DateAdded = DateTime.Now
            };

            var singleEpisodeList = new List<Rom> { new Rom { RomFile = _firstFile, EpisodeFileId = 1 } };
            var doubleEpisodeList = new List<Rom>
            {
                new Rom { RomFile = _firstFile, EpisodeFileId = 1 },
                new Rom { RomFile = _secondFile, EpisodeFileId = 2 }
            };

            var fakeSeries = Builder<Game>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p.Id })
                         .With(c => c.Path = @"C:\Game\My.Game".AsOsAgnostic())
                         .Build();

            _parseResultMulti = new RemoteEpisode
            {
                Game = fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                Roms = doubleEpisodeList
            };

            _parseResultSingle = new RemoteEpisode
            {
                Game = fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                Roms = singleEpisodeList
            };

            GivenUnmonitorDeletedEpisodes(true);
        }

        private void GivenUnmonitorDeletedEpisodes(bool enabled)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(v => v.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(enabled);
        }

        private void WithExistingFile(RomFile romFile)
        {
            var path = Path.Combine(@"C:\Game\My.Game".AsOsAgnostic(), romFile.RelativePath);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(path))
                  .Returns(true);
        }

        [Test]
        public void should_return_true_when_unmonitor_deleted_episdes_is_off()
        {
            GivenUnmonitorDeletedEpisodes(false);

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_searching()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new SeasonSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_file_exists()
        {
            WithExistingFile(_firstFile);

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_file_is_missing()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_both_of_multiple_episode_exist()
        {
            WithExistingFile(_firstFile);
            WithExistingFile(_secondFile);

            Subject.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_one_of_multiple_episode_is_missing()
        {
            WithExistingFile(_firstFile);

            Subject.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }
    }
}
