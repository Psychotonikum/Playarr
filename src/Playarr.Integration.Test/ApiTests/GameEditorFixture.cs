using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Test.Common;
using Playarr.Api.V3.Game;

namespace Playarr.Integration.Test.ApiTests
{
    [TestFixture]
    public class GameEditorFixture : IntegrationTest
    {
        private void GivenExistingSeries()
        {
            WaitForCompletion(() => QualityProfiles.All().Count > 0);

            foreach (var title in new[] { "90210", "Dexter" })
            {
                var newGame = Game.Lookup(title).First();

                newGame.QualityProfileId = 1;
                newGame.Path = string.Format(@"C:\Test\{0}", title).AsOsAgnostic();

                Game.Post(newGame);
            }
        }

        [Test]
        public void should_be_able_to_update_multiple_series()
        {
            GivenExistingSeries();

            var game = Game.All();

            var seriesEditor = new GameEditorResource
            {
                QualityProfileId = 2,
                GameIds = game.Select(s => s.Id).ToList()
            };

            var result = Game.Editor(seriesEditor);

            result.Should().HaveCount(2);
            result.TrueForAll(s => s.QualityProfileId == 2).Should().BeTrue();
        }
    }
}
