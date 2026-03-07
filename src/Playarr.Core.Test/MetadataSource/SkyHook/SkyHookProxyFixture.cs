using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Exceptions;
using Playarr.Core.MediaCover;
using Playarr.Core.MetadataSource.SkyHook;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common.Categories;

namespace Playarr.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxyFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [TestCase(75978, "Family Guy")]
        [TestCase(83462, "Castle (2009)")]
        [TestCase(266189, "The Blacklist")]
        public void should_be_able_to_get_series_detail(int igdbId, string title)
        {
            var details = Subject.GetSeriesInfo(igdbId);

            ValidateSeries(details.Item1);
            ValidateEpisodes(details.Item2);

            details.Item1.Title.Should().Be(title);
        }

        [Test]
        public void getting_details_of_invalid_series()
        {
            Assert.Throws<SeriesNotFoundException>(() => Subject.GetSeriesInfo(int.MaxValue));
        }

        [Test]
        public void should_not_have_period_at_start_of_title_slug()
        {
            var details = Subject.GetSeriesInfo(79099);

            details.Item1.TitleSlug.Should().Be("dothack");
        }

        private void ValidateSeries(Game game)
        {
            game.Should().NotBeNull();
            game.Title.Should().NotBeNullOrWhiteSpace();
            game.CleanTitle.Should().Be(Parser.Parser.CleanGameTitle(game.Title));
            game.SortTitle.Should().Be(GameTitleNormalizer.Normalize(game.Title, game.TvdbId));
            game.Overview.Should().NotBeNullOrWhiteSpace();
            game.AirTime.Should().NotBeNullOrWhiteSpace();
            game.FirstAired.Should().HaveValue();
            game.FirstAired.Value.Kind.Should().Be(DateTimeKind.Utc);
            game.Images.Should().NotBeEmpty();
            game.ImdbId.Should().NotBeNullOrWhiteSpace();
            game.Network.Should().NotBeNullOrWhiteSpace();
            game.Runtime.Should().BeGreaterThan(0);
            game.TitleSlug.Should().NotBeNullOrWhiteSpace();

            // game.MobyGamesId.Should().BeGreaterThan(0);
            game.TvdbId.Should().BeGreaterThan(0);
        }

        private void ValidateEpisodes(List<Rom> roms)
        {
            roms.Should().NotBeEmpty();

            var episodeGroup = roms.GroupBy(e => e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"));
            episodeGroup.Should().OnlyContain(c => c.Count() == 1);

            roms.Should().Contain(c => c.SeasonNumber > 0);
            roms.Should().Contain(c => !string.IsNullOrWhiteSpace(c.Overview));

            foreach (var rom in roms)
            {
                ValidateEpisode(rom);

                // if atleast one episdoe has title it means parse it working.
                roms.Should().Contain(c => !string.IsNullOrWhiteSpace(c.Title));
            }
        }

        private void ValidateEpisode(Rom rom)
        {
            rom.Should().NotBeNull();

            // TODO: Is there a better way to validate that rom number or platform number is greater than zero?
            (rom.EpisodeNumber + rom.SeasonNumber).Should().NotBe(0);

            rom.Should().NotBeNull();

            if (rom.AirDateUtc.HasValue)
            {
                rom.AirDateUtc.Value.Kind.Should().Be(DateTimeKind.Utc);
            }

            rom.Images.Any(i => i.CoverType == MediaCoverTypes.Screenshot && i.RemoteUrl.Contains("-940."))
                   .Should()
                   .BeFalse();
        }
    }
}
