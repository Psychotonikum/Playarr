using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Datastore.Converters;
using Playarr.Core.Datastore.Migration;
using Playarr.Core.Download.Pending;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class fix_pending_releasesFixture : MigrationTest<remove_plex_hometheatre>
    {
        [Test]
        public void should_fix_quality_for_pending_releases()
        {
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedRomInfo162>());

            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("PendingReleases").Row(new
                {
                    SeriesId = 1,
                    Title = "Test Game",
                    Added = DateTime.UtcNow,
                    ParsedRomInfo = @"{
  ""releaseTitle"": ""Nurses.2020.S02E10.720p.HDTV.x264 - SYNCOPY"",
  ""gameTitle"": ""Nurses 2020"",
  ""gameTitleInfo"": {
    ""title"": ""Nurses 2020"",
    ""titleWithoutYear"": ""Nurses"",
    ""year"": 2020
  },
  ""quality"": {
    ""quality"": {
      ""id"": 4,
      ""name"": ""HDTV-720p"",
      ""source"": ""television"",
      ""resolution"": 720
    },
    ""revision"": {
      ""version"": 1,
      ""real"": 0,
      ""isRepack"": false
    }
  },
  ""platformNumber"": 2,
  ""romNumbers"": [
    10
  ],
  ""absoluteRomNumbers"": [],
  ""specialAbsoluteRomNumbers"": [],
  ""language"": {
    ""id"": 1,
    ""name"": ""English""
  },
  ""fullSeason"": false,
  ""isPartialSeason"": false,
  ""isMultiSeason"": false,
  ""isSeasonExtra"": false,
  ""special"": false,
  ""releaseGroup"": ""SYNCOPY"",
  ""releaseHash"": """",
  ""seasonPart"": 0,
  ""releaseTokens"": "".720p.HDTV.x264-SYNCOPY"",
  ""isDaily"": false,
  ""isAbsoluteNumbering"": false,
  ""isPossibleSpecialEpisode"": false,
  ""isPossibleSceneSeasonSpecial"": false
}",
                    Release = "{}",
                    Reason = (int)PendingReleaseReason.Delay
                });
            });

            var json = db.Query<string>("SELECT \"ParsedRomInfo\" FROM \"PendingReleases\"").First();

            var pending = db.Query<ParsedRomInfo162>("SELECT \"ParsedRomInfo\" FROM \"PendingReleases\"").First();
            pending.Quality.Quality.Should().Be(Quality.HDTV720p.Id);
            pending.Language.Should().Be(Language.English.Id);
        }

        private class GameTitleInfo161
        {
            public string Title { get; set; }
            public string TitleWithoutYear { get; set; }
            public int Year { get; set; }
        }

        private class ParsedRomInfo162
        {
            public string GameTitle { get; set; }
            public GameTitleInfo161 GameTitleInfo { get; set; }
            public QualityModel162 Quality { get; set; }
            public int SeasonNumber { get; set; }
            public List<int> RomNumbers { get; set; }
            public List<int> AbsoluteRomNumbers { get; set; }
            public List<int> SpecialAbsoluteRomNumbers { get; set; }
            public int Language { get; set; }
            public bool FullSeason { get; set; }
            public bool IsPartialSeason { get; set; }
            public bool IsMultiSeason { get; set; }
            public bool IsSeasonExtra { get; set; }
            public bool Speacial { get; set; }
            public string ReleaseGroup { get; set; }
            public string ReleaseHash { get; set; }
            public int SeasonPart { get; set; }
            public string ReleaseTokens { get; set; }
            public bool IsDaily { get; set; }
            public bool IsAbsoluteNumbering { get; set; }
            public bool IsPossibleSpecialEpisode { get; set; }
            public bool IsPossibleSceneSeasonSpecial { get; set; }
        }

        private class QualityModel162
        {
            public int Quality { get; set; }
            public Revision162 Revision { get; set; }
        }

        private class Revision162
        {
            public int Version { get; set; }
            public int Real { get; set; }
            public bool IsRepack { get; set; }
        }
    }
}
