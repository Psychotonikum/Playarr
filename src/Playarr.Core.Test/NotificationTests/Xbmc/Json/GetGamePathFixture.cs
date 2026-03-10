using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Notifications.Xbmc;
using Playarr.Core.Notifications.Xbmc.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.NotificationTests.Xbmc.Json
{
    [TestFixture]
    public class GetGamePathFixture : CoreTest<XbmcService>
    {
        private const int IGDB_ID = 5;
        private XbmcSettings _settings;
        private Game _series;
        private List<TvShow> _xbmcSeries;

        [SetUp]
        public void Setup()
        {
            _settings = Builder<XbmcSettings>.CreateNew()
                                             .Build();

            _xbmcSeries = Builder<TvShow>.CreateListOfSize(3)
                                         .All()
                                         .With(s => s.ImdbNumber = "0")
                                         .TheFirst(1)
                                         .With(s => s.ImdbNumber = IGDB_ID.ToString())
                                         .Build()
                                         .ToList();

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetGame(_settings))
                  .Returns(_xbmcSeries);
        }

        private void GivenMatchingIgdbId()
        {
            _series = new Game
                          {
                              IgdbId = IGDB_ID,
                              Title = "TV Show"
                          };
        }

        private void GivenMatchingTitle()
        {
            _series = new Game
            {
                IgdbId = 1000,
                Title = _xbmcSeries.First().Label
            };
        }

        private void GivenMatchingSeries()
        {
            _series = new Game
            {
                IgdbId = 1000,
                Title = "Does not exist"
            };
        }

        [Test]
        public void should_return_null_when_series_is_not_found()
        {
            GivenMatchingSeries();

            Subject.GetGamePath(_settings, _series).Should().BeNull();
        }

        [Test]
        public void should_return_path_when_igdbId_matches()
        {
            GivenMatchingIgdbId();

            Subject.GetGamePath(_settings, _series).Should().Be(_xbmcSeries.First().File);
        }

        [Test]
        public void should_return_path_when_title_matches()
        {
            GivenMatchingTitle();

            Subject.GetGamePath(_settings, _series).Should().Be(_xbmcSeries.First().File);
        }

        [Test]
        public void should_not_throw_when_imdb_number_is_not_a_number()
        {
            GivenMatchingIgdbId();

            _xbmcSeries.ForEach(s => s.ImdbNumber = "tt12345");
            _xbmcSeries.Last().ImdbNumber = IGDB_ID.ToString();

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetGame(_settings))
                  .Returns(_xbmcSeries);

            Subject.GetGamePath(_settings, _series).Should().NotBeNull();
        }
    }
}
