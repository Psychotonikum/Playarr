using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Configuration;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.Languages;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Queue;
using Playarr.Core.Test.CustomFormats;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class QueueSpecificationFixture : CoreTest<QueueSpecification>
    {
        private Game _series;
        private Rom _episode;
        private RemoteEpisode _remoteRom;

        private Game _otherGame;
        private Rom _otherEpisode;

        private ReleaseInfo _releaseInfo;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

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

        private void GivenEmptyQueue()
        {
            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(new List<Queue.Queue>());
        }

        private void GivenQueueFormats(List<CustomFormat> formats)
        {
            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(x => x.ParseCustomFormat(It.IsAny<RemoteEpisode>(), It.IsAny<long>()))
                  .Returns(formats);
        }

        private void GivenQueue(IEnumerable<RemoteEpisode> remoteRoms, TrackedDownloadState trackedDownloadState = TrackedDownloadState.Downloading)
        {
            var queue = remoteRoms.Select(remoteRom => new Queue.Queue
            {
                RemoteEpisode = remoteRom,
                TrackedDownloadState = trackedDownloadState
            });

            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(queue.ToList());
        }

        [Test]
        public void should_return_true_when_queue_is_empty()
        {
            GivenEmptyQueue();
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_series_doesnt_match()
        {
            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Game = _otherGame)
                                                      .With(r => r.Roms = new List<Rom> { _episode })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_everything_is_the_same()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Game = _series)
                .With(r => r.Roms = new List<Rom> { _episode })
                .With(r => r.ParsedRomInfo = new ParsedRomInfo
                {
                    Quality = new QualityModel(Quality.DVD),
                    Languages = new List<Language> { Language.Spanish }
                })
                .With(r => r.CustomFormats = new List<CustomFormat>())
                .With(r => r.Release = _releaseInfo)
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_quality_in_queue_is_lower()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Game = _series)
                                                      .With(r => r.Roms = new List<Rom> { _episode })
                                                      .With(r => r.ParsedRomInfo = new ParsedRomInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.SDTV),
                                                                                           Languages = new List<Language> { Language.Spanish }
                                                                                       })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_quality_in_queue_is_lower_but_language_is_higher()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Game = _series)
                                                      .With(r => r.Roms = new List<Rom> { _episode })
                                                      .With(r => r.ParsedRomInfo = new ParsedRomInfo
                                                      {
                                                          Quality = new QualityModel(Quality.SDTV),
                                                          Languages = new List<Language> { Language.English }
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_episode_doesnt_match()
        {
            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Game = _series)
                                                      .With(r => r.Roms = new List<Rom> { _otherEpisode })
                                                      .With(r => r.ParsedRomInfo = new ParsedRomInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.DVD)
                                                                                       })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_qualities_are_the_same_and_languages_are_the_same_with_higher_custom_format_score()
        {
            _remoteRom.CustomFormats = new List<CustomFormat> { new CustomFormat("My Format", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 1 } };

            var lowFormat = new List<CustomFormat> { new CustomFormat("Bad Format", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 2 } };

            CustomFormatsTestHelpers.GivenCustomFormats(_remoteRom.CustomFormats.First(), lowFormat.First());

            _series.QualityProfile.Value.FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems("My Format");

            GivenQueueFormats(lowFormat);

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Game = _series)
                .With(r => r.Roms = new List<Rom> { _episode })
                .With(r => r.ParsedRomInfo = new ParsedRomInfo
                {
                    Quality = new QualityModel(Quality.DVD),
                    Languages = new List<Language> { Language.Spanish },
                })
                .With(r => r.Release = _releaseInfo)
                .With(r => r.CustomFormats = lowFormat)
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_qualities_are_the_same_and_languages_are_the_same()
        {
            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Game = _series)
                                                      .With(r => r.Roms = new List<Rom> { _episode })
                                                      .With(r => r.ParsedRomInfo = new ParsedRomInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.DVD),
                                                                                           Languages = new List<Language> { Language.Spanish },
                                                                                       })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_in_queue_is_better()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Game = _series)
                                                      .With(r => r.Roms = new List<Rom> { _episode })
                                                      .With(r => r.ParsedRomInfo = new ParsedRomInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.HDTV720p),
                                                                                           Languages = new List<Language> { Language.English }
                                                                                       })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_matching_multi_episode_is_in_queue()
        {
            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Game = _series)
                                                      .With(r => r.Roms = new List<Rom> { _episode, _otherEpisode })
                                                      .With(r => r.ParsedRomInfo = new ParsedRomInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p),
                                                          Languages = new List<Language> { Language.English }
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_episode_has_one_episode_in_queue()
        {
            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Game = _series)
                                                      .With(r => r.Roms = new List<Rom> { _episode })
                                                      .With(r => r.ParsedRomInfo = new ParsedRomInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p),
                                                          Languages = new List<Language> { Language.English }
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            _remoteRom.Roms.Add(_otherEpisode);

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_part_episode_is_already_in_queue()
        {
            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Game = _series)
                                                      .With(r => r.Roms = new List<Rom> { _episode, _otherEpisode })
                                                      .With(r => r.ParsedRomInfo = new ParsedRomInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p),
                                                          Languages = new List<Language> { Language.English }
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            _remoteRom.Roms.Add(_otherEpisode);

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_part_episode_has_two_episodes_in_queue()
        {
            var remoteRoms = Builder<RemoteEpisode>.CreateListOfSize(2)
                                                       .All()
                                                       .With(r => r.Game = _series)
                                                       .With(r => r.CustomFormats = new List<CustomFormat>())
                                                       .With(r => r.ParsedRomInfo = new ParsedRomInfo
                                                                                        {
                                                                                            Quality =
                                                                                                new QualityModel(
                                                                                                Quality.HDTV720p),
                                                                                            Languages = new List<Language> { Language.English }
                                                                                        })
                                                       .With(r => r.Release = _releaseInfo)
                                                       .TheFirst(1)
                                                       .With(r => r.Roms = new List<Rom> { _episode })
                                                       .TheNext(1)
                                                       .With(r => r.Roms = new List<Rom> { _otherEpisode })
                                                       .Build();

            _remoteRom.Roms.Add(_otherEpisode);
            GivenQueue(remoteRoms);
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_quality_in_queue_meets_cutoff()
        {
            _series.QualityProfile.Value.Cutoff = _remoteRom.ParsedRomInfo.Quality.Quality.Id;

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Game = _series)
                                                      .With(r => r.Roms = new List<Rom> { _episode })
                                                      .With(r => r.ParsedRomInfo = new ParsedRomInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p),
                                                          Languages = new List<Language> { Language.Spanish }
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_are_the_same_language_is_better_and_upgrade_allowed_is_false_for_language_profile()
        {
            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Game = _series)
                .With(r => r.Roms = new List<Rom> { _episode })
                .With(r => r.ParsedRomInfo = new ParsedRomInfo
                {
                    Quality = new QualityModel(Quality.DVD),
                    Languages = new List<Language> { Language.English }
                })
                .With(r => r.Release = _releaseInfo)
                .With(r => r.CustomFormats = new List<CustomFormat>())
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_is_better_languages_are_the_same_and_upgrade_allowed_is_false_for_quality_profile()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;
            _series.QualityProfile.Value.UpgradeAllowed = false;

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Game = _series)
                .With(r => r.Roms = new List<Rom> { _episode })
                .With(r => r.ParsedRomInfo = new ParsedRomInfo
                {
                    Quality = new QualityModel(Quality.Bluray1080p),
                    Languages = new List<Language> { Language.Spanish }
                })
                .With(r => r.Release = _releaseInfo)
                .With(r => r.CustomFormats = new List<CustomFormat>())
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_everything_is_the_same_for_failed_pending()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Game = _series)
                .With(r => r.Roms = new List<Rom> { _episode })
                .With(r => r.ParsedRomInfo = new ParsedRomInfo
                {
                    Quality = new QualityModel(Quality.DVD),
                    Languages = new List<Language> { Language.Spanish }
                })
                .With(r => r.Release = _releaseInfo)
                .With(r => r.CustomFormats = new List<CustomFormat>())
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom }, TrackedDownloadState.FailedPending);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_same_quality_non_proper_in_queue_and_download_propers_is_do_not_upgrade()
        {
            _remoteRom.ParsedRomInfo.Quality = new QualityModel(Quality.HDTV720p, new Revision(2));
            _series.QualityProfile.Value.Cutoff = _remoteRom.ParsedRomInfo.Quality.Quality.Id;

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotUpgrade);

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Game = _series)
                .With(r => r.Roms = new List<Rom> { _episode })
                .With(r => r.ParsedRomInfo = new ParsedRomInfo
                {
                    Quality = new QualityModel(Quality.HDTV720p),
                    Languages = new List<Language> { Language.English }
                })
                .With(r => r.Release = _releaseInfo)
                .With(r => r.CustomFormats = new List<CustomFormat>())
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteRom });

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }
    }
}
