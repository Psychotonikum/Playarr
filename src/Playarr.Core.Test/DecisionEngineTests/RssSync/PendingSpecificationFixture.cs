using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications.RssSync;
using Playarr.Core.Download.Pending;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.CustomFormats;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class PendingSpecificationFixture : CoreTest<PendingSpecification>
    {
        private Game _series;
        private Rom _episode;
        private RemoteEpisode _remoteRom;

        private Game _otherGame;
        private Rom _otherEpisode;

        private ReleaseInfo _releaseInfo;
        private ReleaseDecisionInformation _information = new(false, null);

        [SetUp]
        public void Setup()
        {
            CustomFormatsTestHelpers.GivenCustomFormats();

            _series = Builder<Game>.CreateNew()
                                     .With(e => e.QualityProfile = new QualityProfile
                                                                {
                                                                    UpgradeAllowed = true,
                                                                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                                                                    FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems(),
                                                                    MinFormatScore = 0
                                                                })
                                     .Build();

            _episode = Builder<Rom>.CreateNew()
                                       .With(e => e.GameId = _series.Id)
                                       .Build();

            _otherGame = Builder<Game>.CreateNew()
                                          .With(s => s.Id = 2)
                                          .Build();

            _otherEpisode = Builder<Rom>.CreateNew()
                                            .With(e => e.GameId = _otherGame.Id)
                                            .With(e => e.Id = 2)
                                            .With(e => e.PlatformNumber = 2)
                                            .With(e => e.EpisodeNumber = 2)
                                            .Build();

            _releaseInfo = Builder<ReleaseInfo>.CreateNew()
                                               .Build();

            _remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                   .With(r => r.Game = _series)
                                                   .With(r => r.Roms = new List<Rom> { _episode })
                                                   .With(r => r.ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD), Languages = new List<Language> { Language.Spanish } })
                                                   .With(r => r.CustomFormats = new List<CustomFormat>())
                                                   .Build();

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(x => x.ParseCustomFormat(It.IsAny<RemoteEpisode>(), It.IsAny<long>()))
                  .Returns(new List<CustomFormat>());
        }

        private void GivenEmptyPendingQueue()
        {
            Mocker.GetMock<IPendingReleaseService>()
                .Setup(s => s.GetPendingQueue())
                .Returns(new List<Queue.Queue>());
        }

        private void GivenPendingQueue(IEnumerable<RemoteEpisode> remoteRoms)
        {
            var queue = remoteRoms.Select(remoteRom => new Queue.Queue
            {
                RemoteEpisode = remoteRom
            });

            Mocker.GetMock<IPendingReleaseService>()
                .Setup(s => s.GetPendingQueue())
                .Returns(queue.ToList());
        }

        [Test]
        public void should_return_true_when_pending_queue_is_empty()
        {
            GivenEmptyPendingQueue();

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();

            Mocker.GetMock<IPendingReleaseService>()
                .Verify(s => s.GetPendingQueue(), Times.Once);
        }

        [Test]
        public void should_return_true_when_not_pushed_release()
        {
            _remoteRom.ReleaseSource = ReleaseSourceType.Rss;

            GivenEmptyPendingQueue();

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();

            Mocker.GetMock<IPendingReleaseService>()
                .Verify(s => s.GetPendingQueue(), Times.Never);
        }

        [Test]
        public void should_return_true_when_series_and_episode_is_not_pending()
        {
            GivenEmptyPendingQueue();

            Mocker.GetMock<IPendingReleaseService>()
                .Setup(s => s.GetPendingQueue())
                .Returns(new List<Queue.Queue>
                {
                    new()
                    {
                        RemoteEpisode = new RemoteEpisode
                        {
                            Game = _otherGame,
                            Roms = new List<Rom> { _otherEpisode }
                        }
                    }
                });

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();

            Mocker.GetMock<IPendingReleaseService>()
                .Verify(s => s.GetPendingQueue(), Times.Once);
        }

        [Test]
        public void should_return_true_when_episode_is_not_pending()
        {
            GivenEmptyPendingQueue();

            _otherEpisode.GameId = _series.Id;

            Mocker.GetMock<IPendingReleaseService>()
                .Setup(s => s.GetPendingQueue())
                .Returns(new List<Queue.Queue>
                {
                    new()
                    {
                        RemoteEpisode = new RemoteEpisode
                        {
                            Game = _series,
                            Roms = new List<Rom> { _otherEpisode }
                        }
                    }
                });

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();

            Mocker.GetMock<IPendingReleaseService>()
                .Verify(s => s.GetPendingQueue(), Times.Once);
        }

        [Test]
        public void should_return_false_when_episode_is_pending()
        {
            GivenEmptyPendingQueue();

            Mocker.GetMock<IPendingReleaseService>()
                .Setup(s => s.GetPendingQueue())
                .Returns(new List<Queue.Queue>
                {
                    new()
                    {
                        RemoteEpisode = new RemoteEpisode
                        {
                            Game = _series,
                            Roms = new List<Rom> { _episode }
                        }
                    }
                });

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();

            Mocker.GetMock<IPendingReleaseService>()
                .Verify(s => s.GetPendingQueue(), Times.Once);
        }
    }
}
