using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Core.Download;
using Playarr.Core.Housekeeping.Housekeepers;
using Playarr.Core.Test.Framework;
using Playarr.Core.ThingiProvider.Status;

namespace Playarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class FixFutureDownloadClientStatusTimesFixture : CoreTest<FixFutureDownloadClientStatusTimes>
    {
        [Test]
        public void should_set_disabled_till_when_its_too_far_in_the_future()
        {
            var disabledTillTime = EscalationBackOff.Periods[1];
            var downloadClientStatuses = Builder<DownloadClientStatus>.CreateListOfSize(5)
                                                        .All()
                                                        .With(t => t.DisabledTill = DateTime.UtcNow.AddDays(5))
                                                        .With(t => t.InitialFailure = DateTime.UtcNow.AddDays(-5))
                                                        .With(t => t.MostRecentFailure = DateTime.UtcNow.AddDays(-5))
                                                        .With(t => t.EscalationLevel = 1)
                                                        .BuildListOfNew();

            Mocker.GetMock<IDownloadClientStatusRepository>()
                  .Setup(s => s.All())
                  .Returns(downloadClientStatuses);

            Subject.Clean();

            Mocker.GetMock<IDownloadClientStatusRepository>()
                  .Verify(v => v.UpdateMany(
                          It.Is<List<DownloadClientStatus>>(i => i.All(
                              s => s.DisabledTill.Value <= DateTime.UtcNow.AddMinutes(disabledTillTime)))));
        }

        [Test]
        public void should_set_initial_failure_when_its_in_the_future()
        {
            var downloadClientStatuses = Builder<DownloadClientStatus>.CreateListOfSize(5)
                                                        .All()
                                                        .With(t => t.DisabledTill = DateTime.UtcNow.AddDays(-5))
                                                        .With(t => t.InitialFailure = DateTime.UtcNow.AddDays(5))
                                                        .With(t => t.MostRecentFailure = DateTime.UtcNow.AddDays(-5))
                                                        .With(t => t.EscalationLevel = 1)
                                                        .BuildListOfNew();

            Mocker.GetMock<IDownloadClientStatusRepository>()
                  .Setup(s => s.All())
                  .Returns(downloadClientStatuses);

            Subject.Clean();

            Mocker.GetMock<IDownloadClientStatusRepository>()
                  .Verify(v => v.UpdateMany(
                          It.Is<List<DownloadClientStatus>>(i => i.All(
                              s => s.InitialFailure.Value <= DateTime.UtcNow))));
        }

        [Test]
        public void should_set_most_recent_failure_when_its_in_the_future()
        {
            var downloadClientStatuses = Builder<DownloadClientStatus>.CreateListOfSize(5)
                                                        .All()
                                                        .With(t => t.DisabledTill = DateTime.UtcNow.AddDays(-5))
                                                        .With(t => t.InitialFailure = DateTime.UtcNow.AddDays(-5))
                                                        .With(t => t.MostRecentFailure = DateTime.UtcNow.AddDays(5))
                                                        .With(t => t.EscalationLevel = 1)
                                                        .BuildListOfNew();

            Mocker.GetMock<IDownloadClientStatusRepository>()
                  .Setup(s => s.All())
                  .Returns(downloadClientStatuses);

            Subject.Clean();

            Mocker.GetMock<IDownloadClientStatusRepository>()
                  .Verify(v => v.UpdateMany(
                          It.Is<List<DownloadClientStatus>>(i => i.All(
                              s => s.MostRecentFailure.Value <= DateTime.UtcNow))));
        }

        [Test]
        public void should_not_change_statuses_when_times_are_in_the_past()
        {
            var downloadClientStatuses = Builder<DownloadClientStatus>.CreateListOfSize(5)
                                                        .All()
                                                        .With(t => t.DisabledTill = DateTime.UtcNow.AddDays(-5))
                                                        .With(t => t.InitialFailure = DateTime.UtcNow.AddDays(-5))
                                                        .With(t => t.MostRecentFailure = DateTime.UtcNow.AddDays(-5))
                                                        .With(t => t.EscalationLevel = 0)
                                                        .BuildListOfNew();

            Mocker.GetMock<IDownloadClientStatusRepository>()
                  .Setup(s => s.All())
                  .Returns(downloadClientStatuses);

            Subject.Clean();

            Mocker.GetMock<IDownloadClientStatusRepository>()
                  .Verify(v => v.UpdateMany(
                          It.Is<List<DownloadClientStatus>>(i => i.Count == 0)));
        }
    }
}
