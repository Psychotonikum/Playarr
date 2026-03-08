using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Test.Common;
using Playarr.Api.V3.Game;

namespace Playarr.Integration.Test.ApiTests
{
    [TestFixture]
    public class EpisodeFixture : IntegrationTest
    {
        private GameResource _series;

        [SetUp]
        public void Setup()
        {
            _series = GivenSeriesWithEpisodes();
        }

        private GameResource GivenSeriesWithEpisodes()
        {
            var newGame = Game.Lookup("archer").Single(c => c.IgdbId == 110381);

            newGame.QualityProfileId = 1;
            newGame.Path = @"C:\Test\Archer".AsOsAgnostic();

            newGame = Game.Post(newGame);

            WaitForCompletion(() => Roms.GetEpisodesInSeries(newGame.Id).Count > 0);

            return newGame;
        }

        [Test]
        public void should_be_able_to_get_all_episodes_in_series()
        {
            Roms.GetEpisodesInSeries(_series.Id).Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_able_to_get_a_single_episode()
        {
            var roms = Roms.GetEpisodesInSeries(_series.Id);

            Roms.Get(roms.First().Id).Should().NotBeNull();
        }

        [Test]
        public void should_be_able_to_set_monitor_status()
        {
            var roms = Roms.GetEpisodesInSeries(_series.Id);
            var updatedEpisode = roms.First();
            updatedEpisode.Monitored = false;

            Roms.SetMonitored(updatedEpisode).Monitored.Should().BeFalse();
        }

        [TearDown]
        public void TearDown()
        {
            Game.Delete(_series.Id);
            Thread.Sleep(2000);
        }
    }
}
