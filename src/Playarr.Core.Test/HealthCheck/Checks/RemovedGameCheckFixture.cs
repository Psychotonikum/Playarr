using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Core.HealthCheck.Checks;
using Playarr.Core.Localization;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class RemovedSeriesCheckFixture : CoreTest<RemovedSeriesCheck>
    {
        private void GivenSeries(int amount, int deleted)
        {
            List<Game> game;

            if (amount == 0)
            {
                game = new List<Game>();
            }
            else if (deleted == 0)
            {
                game = Builder<Game>.CreateListOfSize(amount)
                    .All()
                    .With(v => v.Status = GameStatusType.Continuing)
                    .BuildList();
            }
            else
            {
                game = Builder<Game>.CreateListOfSize(amount)
                    .All()
                    .With(v => v.Status = GameStatusType.Continuing)
                    .Random(deleted)
                    .With(v => v.Status = GameStatusType.Deleted)
                    .BuildList();
            }

            Mocker.GetMock<IGameService>()
                .Setup(v => v.GetAllSeries())
                .Returns(game);

            Mocker.GetMock<ILocalizationService>()
                .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                .Returns("Some Warning Message");
        }

        [Test]
        public void should_return_error_if_series_no_longer_on_tvdb()
        {
            GivenSeries(4, 1);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_error_if_multiple_series_no_longer_on_tvdb()
        {
            GivenSeries(4, 2);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_ok_if_all_series_still_on_tvdb()
        {
            GivenSeries(4, 0);

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_if_no_series_exist()
        {
            GivenSeries(0, 0);

            Subject.Check().ShouldBeOk();
        }
    }
}
