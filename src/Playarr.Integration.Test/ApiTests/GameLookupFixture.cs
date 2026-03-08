using FluentAssertions;
using NUnit.Framework;

namespace Playarr.Integration.Test.ApiTests
{
    [TestFixture]
    public class GameLookupFixture : IntegrationTest
    {
        [TestCase("archer", "Archer (2009)")]
        [TestCase("90210", "90210")]
        public void lookup_new_series_by_title(string term, string title)
        {
            var game = Game.Lookup(term);

            game.Should().NotBeEmpty();
            game.Should().Contain(c => c.Title == title);
        }

        [Test]
        public void lookup_new_series_by_igdbid()
        {
            var game = Game.Lookup("igdb:266189");

            game.Should().NotBeEmpty();
            game.Should().Contain(c => c.Title == "The Blacklist");
        }

        [Test]
        [Ignore("Unreliable")]
        public void lookup_random_series_using_asterix()
        {
            var game = Game.Lookup("*");

            game.Should().NotBeEmpty();
        }
    }
}
