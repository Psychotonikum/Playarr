using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Playarr.Integration.Test.ApiTests
{
    [TestFixture]
    public class SeriesFixture : IntegrationTest
    {
        // IGDB IDs for real games
        private const int WitcherIgdbId = 1942;
        private const string WitcherTitle = "The Witcher 3: Wild Hunt";
        private const int PortalIgdbId = 732;
        private const string PortalTitle = "Portal 2";

        [Test]
        [Order(0)]
        public void add_series_with_tags_should_store_them()
        {
            EnsureNoGame(WitcherIgdbId, WitcherTitle);
            var tag = EnsureTag("abc");

            var game = Game.Lookup("igdb:" + WitcherIgdbId).Single();

            game.QualityProfileId = 1;
            game.Path = Path.Combine(SeriesRootFolder, game.Title);
            game.Tags = new HashSet<int>();
            game.Tags.Add(tag.Id);

            var result = Game.Post(game);

            result.Should().NotBeNull();
            result.Tags.Should().Equal(tag.Id);
        }

        [Test]
        [Order(0)]
        public void add_series_without_profileid_should_return_badrequest()
        {
            EnsureNoGame(WitcherIgdbId, WitcherTitle);

            var game = Game.Lookup("igdb:" + WitcherIgdbId).Single();

            game.Path = Path.Combine(SeriesRootFolder, game.Title);

            Game.InvalidPost(game);
        }

        [Test]
        [Order(0)]
        public void add_series_without_path_should_return_badrequest()
        {
            EnsureNoGame(WitcherIgdbId, WitcherTitle);

            var game = Game.Lookup("igdb:" + WitcherIgdbId).Single();

            game.QualityProfileId = 1;

            Game.InvalidPost(game);
        }

        [Test]
        [Order(1)]
        public void add_series()
        {
            EnsureNoGame(WitcherIgdbId, WitcherTitle);

            var game = Game.Lookup("igdb:" + WitcherIgdbId).Single();

            game.QualityProfileId = 1;
            game.Path = Path.Combine(SeriesRootFolder, game.Title);

            var result = Game.Post(game);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);
            result.QualityProfileId.Should().Be(1);
            result.Path.Should().Be(Path.Combine(SeriesRootFolder, game.Title));
        }

        [Test]
        [Order(2)]
        public void get_all_series()
        {
            EnsureSeries(WitcherIgdbId, WitcherTitle);
            EnsureSeries(PortalIgdbId, PortalTitle);

            Game.All().Should().NotBeNullOrEmpty();
            Game.All().Should().Contain(v => v.IgdbId == PortalIgdbId);
            Game.All().Should().Contain(v => v.IgdbId == WitcherIgdbId);
        }

        [Test]
        [Order(2)]
        public void get_series_by_id()
        {
            var game = EnsureSeries(WitcherIgdbId, WitcherTitle);

            var result = Game.Get(game.Id);

            result.IgdbId.Should().Be(WitcherIgdbId);
        }

        [Test]
        public void get_series_by_unknown_id_should_return_404()
        {
            var result = Game.InvalidGet(1000000);
        }

        [Test]
        [Order(2)]
        public void update_series_profile_id()
        {
            var game = EnsureSeries(WitcherIgdbId, WitcherTitle);

            var profileId = 1;
            if (game.QualityProfileId == profileId)
            {
                profileId = 2;
            }

            game.QualityProfileId = profileId;

            var result = Game.Put(game);

            Game.Get(game.Id).QualityProfileId.Should().Be(profileId);
        }

        [Test]
        [Order(3)]
        public void update_series_monitored()
        {
            var game = EnsureSeries(WitcherIgdbId, WitcherTitle, false);

            game.Monitored.Should().BeFalse();

            game.Monitored = true;
            game.Platforms.ForEach(platform =>
            {
                platform.Monitored = true;
            });

            var result = Game.Put(game);

            result.Monitored.Should().BeTrue();
        }

        [Test]
        [Order(3)]
        public void update_series_tags()
        {
            var game = EnsureSeries(WitcherIgdbId, WitcherTitle);
            var tag = EnsureTag("abc");

            if (game.Tags.Contains(tag.Id))
            {
                game.Tags.Remove(tag.Id);

                var result = Game.Put(game);
                Game.Get(game.Id).Tags.Should().NotContain(tag.Id);
            }
            else
            {
                game.Tags.Add(tag.Id);

                var result = Game.Put(game);
                Game.Get(game.Id).Tags.Should().Contain(tag.Id);
            }
        }

        [Test]
        [Order(4)]
        public void delete_series()
        {
            var game = EnsureSeries(WitcherIgdbId, WitcherTitle);

            Game.Get(game.Id).Should().NotBeNull();

            Game.Delete(game.Id);

            Game.All().Should().NotContain(v => v.IgdbId == WitcherIgdbId);
        }
    }
}
