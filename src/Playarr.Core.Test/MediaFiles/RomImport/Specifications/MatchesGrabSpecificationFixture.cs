using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.History;
using Playarr.Core.MediaFiles.EpisodeImport.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class MatchesGrabSpecificationFixture : CoreTest<MatchesGrabSpecification>
    {
        private Rom _episode1;
        private Rom _episode2;
        private Rom _episode3;
        private LocalEpisode _localRom;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _episode1 = Builder<Rom>.CreateNew()
                .With(e => e.Id = 1)
                .Build();

            _episode2 = Builder<Rom>.CreateNew()
                .With(e => e.Id = 2)
                .Build();

            _episode3 = Builder<Rom>.CreateNew()
                .With(e => e.Id = 3)
                .Build();

            _localRom = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Path = @"C:\Test\Unsorted\Game.Title.S01E01.720p.HDTV-Playarr\S01E05.mkv".AsOsAgnostic())
                                                 .With(l => l.Roms = new List<Rom> { _episode1 })
                                                 .With(l => l.Release = null)
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();
        }

        private void GivenHistoryForEpisodes(params Rom[] roms)
        {
            if (roms.Empty())
            {
                return;
            }

            var grabbedHistories = Builder<EpisodeHistory>.CreateListOfSize(roms.Length)
                .All()
                .With(h => h.EventType == EpisodeHistoryEventType.Grabbed)
                .BuildList();

            for (var i = 0; i < grabbedHistories.Count; i++)
            {
                grabbedHistories[i].EpisodeId = roms[i].Id;
            }

            _localRom.Release = new GrabbedReleaseInfo(grabbedHistories);
        }

        [Test]
        public void should_be_accepted_for_existing_file()
        {
            _localRom.ExistingFile = true;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_download_client_item()
        {
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_grabbed_release_info()
        {
            GivenHistoryForEpisodes();

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_episode_matches_single_grabbed_release_info()
        {
            GivenHistoryForEpisodes(_episode1);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_episode_is_in_multi_episode_grabbed_release_info()
        {
            GivenHistoryForEpisodes(_episode1, _episode2);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_file_episode_does_not_match_single_grabbed_release_info()
        {
            GivenHistoryForEpisodes(_episode2);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_rejected_if_file_episode_is_not_in_multi_episode_grabbed_release_info()
        {
            GivenHistoryForEpisodes(_episode2, _episode3);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}
