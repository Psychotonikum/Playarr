using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Configuration;
using Playarr.Core.CustomFormats;
using Playarr.Core.Datastore;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.MediaFiles.EpisodeImport.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class UpgradeSpecificationFixture : CoreTest<UpgradeSpecification>
    {
        private Game _series;
        private LocalEpisode _localRom;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.SeriesType = GameTypes.Standard)
                                     .With(e => e.QualityProfile = new QualityProfile
                                        {
                                            Items = Qualities.QualityFixture.GetDefaultQualities(),
                                        })
                                     .Build();

            _localRom = new LocalEpisode
                                {
                                    Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                    Languages = new List<Language> { Language.Spanish },
                                    Game = _series
                                };
        }

        [Test]
        public void should_return_true_if_no_existing_romFile()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 0)
                                                     .With(e => e.RomFile = null)
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_no_existing_romFile_for_multi_episodes()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 0)
                                                     .With(e => e.RomFile = null)
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_romFile()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                                                new RomFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.SDTV, new Revision(version: 1)),
                                                                                    Languages = new List<Language> { Language.Spanish }
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_language_upgrade_for_existing_romFile_and_quality_is_same()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                                                new RomFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                                                                    Languages = new List<Language> { Language.English }
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_upgrade_for_existing_romFile_and_quality_is_worse()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                                                new RomFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                                                                                    Languages = new List<Language> { Language.English }
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_romFile_for_multi_episodes()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                                                new RomFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.SDTV, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_language_upgrade_for_existing_romFile_for_multi_episodes_and_quality_is_same()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                                                new RomFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                                                                    Languages = new List<Language> { Language.English }
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_upgrade_for_existing_romFile_for_multi_episodes_and_quality_is_worse()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                                                new RomFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                                                                                    Languages = new List<Language> { Language.English }
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_romFile()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                                                new RomFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_romFile_for_multi_episodes()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                                                new RomFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_one_existing_romFile_for_multi_episode()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .TheFirst(1)
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                                                new RomFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.SDTV, new Revision(version: 1))
                                                                                }))
                                                     .TheNext(1)
                                                     .With(e => e.EpisodeFileId = 2)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                                                new RomFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_a_revision_upgrade_and_prefers_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                         new RomFile
                                                         {
                                                             Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                                                             Languages = new List<Language> { Language.Spanish }
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_it_is_a_preferred_word_downgrade_and_language_downgrade_and_a_quality_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RomFile>()))
                  .Returns(new List<CustomFormat>());

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RemoteEpisode>(), It.IsAny<long>()))
                  .Returns(new List<CustomFormat>());

            _localRom.Quality = new QualityModel(Quality.Bluray2160p);

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                         new RomFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p),
                                                             Languages = new List<Language> { Language.French }
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localRom.FileRomInfo = Builder<ParsedRomInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_is_a_preferred_word_downgrade_but_a_language_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RomFile>()))
                  .Returns(new List<CustomFormat>());

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RemoteEpisode>(), It.IsAny<long>()))
                  .Returns(new List<CustomFormat>());

            _localRom.Quality = new QualityModel(Quality.Bluray1080p);

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                         new RomFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p),
                                                             Languages = new List<Language> { Language.English }
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localRom.FileRomInfo = Builder<ParsedRomInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_not_a_revision_upgrade_and_does_not_prefer_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                         new RomFile
                                                         {
                                                             Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_comparing_to_a_lower_quality_proper()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localRom.Quality = new QualityModel(Quality.Bluray1080p);

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                         new RomFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_is_a_preferred_word_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RomFile>()))
                  .Returns(new List<CustomFormat>());

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RemoteEpisode>(), It.IsAny<long>()))
                  .Returns(new List<CustomFormat>());

            _localRom.Quality = new QualityModel(Quality.Bluray1080p);

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                         new RomFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p)
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localRom.FileRomInfo = Builder<ParsedRomInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_has_an_equal_preferred_word_score()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RomFile>()))
                  .Returns(new List<CustomFormat>());

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RemoteEpisode>(), It.IsAny<long>()))
                  .Returns(new List<CustomFormat>());

            _localRom.Quality = new QualityModel(Quality.Bluray1080p);

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(
                                                         new RomFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p)
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localRom.FileRomInfo = Builder<ParsedRomInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_episode_file_is_null()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(null))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_to_custom_format_score()
        {
            var romFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var romFile = new RomFile
            {
                Quality = new QualityModel(Quality.Bluray1080p)
            };

            _series.QualityProfile.Value.FormatItems = romFileCustomFormats.Select(c => new ProfileFormatItem
            {
                Format = c,
                Score = 10
            })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(romFile))
                .Returns(romFileCustomFormats);

            _localRom.Quality = new QualityModel(Quality.Bluray1080p);
            _localRom.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localRom.CustomFormatScore = 20;

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                .All()
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.RomFile = new LazyLoaded<RomFile>(romFile))
                .Build()
                .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_not_upgrade_to_custom_format_score_but_is_upgrade_to_quality()
        {
            var romFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var romFile = new RomFile
            {
                Quality = new QualityModel(Quality.Bluray720p)
            };

            _series.QualityProfile.Value.FormatItems = romFileCustomFormats.Select(c => new ProfileFormatItem
                {
                    Format = c,
                    Score = 50
                })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(romFile))
                .Returns(romFileCustomFormats);

            _localRom.Quality = new QualityModel(Quality.Bluray1080p);
            _localRom.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localRom.CustomFormatScore = 20;

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                .All()
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.RomFile = new LazyLoaded<RomFile>(romFile))
                .Build()
                .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_not_upgrade_to_custom_format_score()
        {
            var romFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var romFile = new RomFile
            {
                Quality = new QualityModel(Quality.Bluray1080p)
            };

            _series.QualityProfile.Value.FormatItems = romFileCustomFormats.Select(c => new ProfileFormatItem
                {
                    Format = c,
                    Score = 50
                })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(romFile))
                .Returns(romFileCustomFormats);

            _localRom.Quality = new QualityModel(Quality.Bluray1080p);
            _localRom.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localRom.CustomFormatScore = 20;

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                .All()
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.RomFile = new LazyLoaded<RomFile>(romFile))
                .Build()
                .ToList();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_and_a_specific_reason_if_not_upgrade_to_custom_format_score_after_local_file_rename_but_was_before()
        {
            var romFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var romFile = new RomFile
            {
                Quality = new QualityModel(Quality.Bluray1080p)
            };

            _series.QualityProfile.Value.FormatItems = romFileCustomFormats.Select(c => new ProfileFormatItem
                {
                    Format = c,
                    Score = 50
                })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(romFile))
                .Returns(romFileCustomFormats);

            _localRom.Quality = new QualityModel(Quality.Bluray1080p);
            _localRom.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localRom.CustomFormatScore = 20;
            _localRom.OriginalFileNameCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localRom.OriginalFileNameCustomFormatScore = 60;

            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                .All()
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.RomFile = new LazyLoaded<RomFile>(romFile))
                .Build()
                .ToList();

            var result = Subject.IsSatisfiedBy(_localRom, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.NotCustomFormatUpgradeAfterRename);
        }
    }
}
