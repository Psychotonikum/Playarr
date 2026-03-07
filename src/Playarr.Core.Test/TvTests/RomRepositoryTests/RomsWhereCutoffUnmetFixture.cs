using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Datastore;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.TvTests.RomRepositoryTests
{
    [TestFixture]
    public class EpisodesWhereCutoffUnmetFixture : DbTest<RomRepository, Rom>
    {
        private Game _monitoredSeries;
        private Game _unmonitoredSeries;
        private PagingSpec<Rom> _pagingSpec;
        private List<QualitiesBelowCutoff> _qualitiesBelowCutoff;
        private List<Rom> _unairedEpisodes;

        [SetUp]
        public void Setup()
        {
            var profile = new QualityProfile
            {
                Id = 1,
                Cutoff = Quality.WEBDL480p.Id,
                Items = new List<QualityProfileQualityItem>
                {
                    new QualityProfileQualityItem { Allowed = true, Quality = Quality.SDTV },
                    new QualityProfileQualityItem { Allowed = true, Quality = Quality.WEBDL480p },
                    new QualityProfileQualityItem { Allowed = true, Quality = Quality.RAWHD }
                }
            };

            _monitoredSeries = Builder<Game>.CreateNew()
                                              .With(s => s.MobyGamesId = RandomNumber)
                                              .With(s => s.Runtime = 30)
                                              .With(s => s.Monitored = true)
                                              .With(s => s.TitleSlug = "Title3")
                                              .With(s => s.QualityProfileId = profile.Id)
                                              .BuildNew();

            _unmonitoredSeries = Builder<Game>.CreateNew()
                                                .With(s => s.TvdbId = RandomNumber)
                                                .With(s => s.Runtime = 30)
                                                .With(s => s.Monitored = false)
                                                .With(s => s.TitleSlug = "Title2")
                                                .With(s => s.QualityProfileId = profile.Id)
                                                .BuildNew();

            _monitoredSeries.Id = Db.Insert(_monitoredSeries).Id;
            _unmonitoredSeries.Id = Db.Insert(_unmonitoredSeries).Id;

            _pagingSpec = new PagingSpec<Rom>
                              {
                                  Page = 1,
                                  PageSize = 10,
                                  SortKey = "AirDate",
                                  SortDirection = SortDirection.Ascending
                              };

            _qualitiesBelowCutoff = new List<QualitiesBelowCutoff>
                                    {
                                        new QualitiesBelowCutoff(profile.Id, new[] { Quality.SDTV.Id })
                                    };

            var qualityMetLanguageUnmet = new RomFile { RelativePath = "a", Quality = new QualityModel { Quality = Quality.WEBDL480p }, Languages = new List<Language> { Language.English } };
            var qualityMetLanguageMet = new RomFile { RelativePath = "b", Quality = new QualityModel { Quality = Quality.WEBDL480p }, Languages = new List<Language> { Language.Spanish } };
            var qualityMetLanguageExceed = new RomFile { RelativePath = "c", Quality = new QualityModel { Quality = Quality.WEBDL480p }, Languages = new List<Language> { Language.French } };
            var qualityUnmetLanguageUnmet = new RomFile { RelativePath = "d", Quality = new QualityModel { Quality = Quality.SDTV }, Languages = new List<Language> { Language.English } };
            var qualityUnmetLanguageMet = new RomFile { RelativePath = "e", Quality = new QualityModel { Quality = Quality.SDTV }, Languages = new List<Language> { Language.Spanish } };
            var qualityUnmetLanguageExceed = new RomFile { RelativePath = "f", Quality = new QualityModel { Quality = Quality.SDTV }, Languages = new List<Language> { Language.French } };
            var qualityRawHDLanguageUnmet = new RomFile { RelativePath = "g", Quality = new QualityModel { Quality = Quality.RAWHD }, Languages = new List<Language> { Language.English } };
            var qualityRawHDLanguageMet = new RomFile { RelativePath = "h", Quality = new QualityModel { Quality = Quality.RAWHD }, Languages = new List<Language> { Language.Spanish } };
            var qualityRawHDLanguageExceed = new RomFile { RelativePath = "i", Quality = new QualityModel { Quality = Quality.RAWHD }, Languages = new List<Language> { Language.French } };

            var fileRepository = Mocker.Resolve<MediaFileRepository>();

            qualityMetLanguageUnmet = fileRepository.Insert(qualityMetLanguageUnmet);
            qualityMetLanguageMet = fileRepository.Insert(qualityMetLanguageMet);
            qualityMetLanguageExceed = fileRepository.Insert(qualityMetLanguageExceed);
            qualityUnmetLanguageUnmet = fileRepository.Insert(qualityUnmetLanguageUnmet);
            qualityUnmetLanguageMet = fileRepository.Insert(qualityUnmetLanguageMet);
            qualityUnmetLanguageExceed = fileRepository.Insert(qualityUnmetLanguageExceed);
            qualityRawHDLanguageUnmet = fileRepository.Insert(qualityRawHDLanguageUnmet);
            qualityRawHDLanguageMet = fileRepository.Insert(qualityRawHDLanguageMet);
            qualityRawHDLanguageExceed = fileRepository.Insert(qualityRawHDLanguageExceed);

            var monitoredSeriesEpisodes = Builder<Rom>.CreateListOfSize(4)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _monitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityUnmetLanguageUnmet.Id)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .With(e => e.EpisodeFileId = qualityMetLanguageMet.Id)
                                           .TheNext(1)
                                           .With(e => e.EpisodeFileId = qualityRawHDLanguageExceed.Id)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();

            var unmonitoredSeriesEpisodes = Builder<Rom>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _unmonitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityRawHDLanguageUnmet.Id)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .With(e => e.EpisodeFileId = qualityMetLanguageMet.Id)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();

            _unairedEpisodes             = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _monitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityUnmetLanguageUnmet.Id)
                                           .Build()
                                           .ToList();

            Db.InsertMany(monitoredSeriesEpisodes);
            Db.InsertMany(unmonitoredSeriesEpisodes);
        }

        private void GivenMonitoredFilterExpression()
        {
            _pagingSpec.FilterExpressions.Add(e => e.Monitored == true && e.Game.Monitored == true);
        }

        private void GivenUnmonitoredFilterExpression()
        {
            _pagingSpec.FilterExpressions.Add(e => e.Monitored == false || e.Game.Monitored == false);
        }

        [Test]
        public void should_include_episodes_where_cutoff_has_not_be_met()
        {
            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, false);

            spec.Records.Should().HaveCount(1);
            spec.Records.Should().OnlyContain(e => e.RomFile.Value.Quality.Quality == Quality.SDTV);
        }

        [Test]
        public void should_only_contain_monitored_episodes()
        {
            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, false);

            spec.Records.Should().HaveCount(1);
            spec.Records.Should().OnlyContain(e => e.Monitored);
        }

        [Test]
        public void should_only_contain_episode_with_monitored_series()
        {
            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, false);

            spec.Records.Should().HaveCount(1);
            spec.Records.Should().OnlyContain(e => e.Game.Monitored);
        }

        [Test]
        public void should_contain_unaired_episodes_if_file_does_not_meet_cutoff()
        {
            Db.InsertMany(_unairedEpisodes);

            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, false);

            spec.Records.Should().HaveCount(2);
            spec.Records.Should().OnlyContain(e => e.Game.Monitored);
        }
    }
}
