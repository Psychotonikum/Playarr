using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Playarr.Common.Model;
using Playarr.Common.Processes;
using Playarr.Host;
using Playarr.Test.Common;

namespace Playarr.App.Test
{
    [TestFixture]
    public class PlayarrProcessServiceFixture : TestBase<SingleInstancePolicy>
    {
        private const int CURRENT_PROCESS_ID = 5;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IProcessProvider>().Setup(c => c.GetCurrentProcess())
                .Returns(new ProcessInfo() { Id = CURRENT_PROCESS_ID });

            Mocker.GetMock<IProcessProvider>()
                  .Setup(s => s.FindProcessByName(ProcessProvider.PLAYARR_CONSOLE_PROCESS_NAME))
                  .Returns(new List<ProcessInfo>());

            Mocker.GetMock<IProcessProvider>()
                  .Setup(s => s.FindProcessByName(ProcessProvider.PLAYARR_PROCESS_NAME))
                  .Returns(new List<ProcessInfo>());
        }

        [Test]
        public void should_continue_if_only_instance()
        {
            Mocker.GetMock<IProcessProvider>()
                  .Setup(c => c.FindProcessByName(It.Is<string>(f => f.Contains("Playarr"))))
                  .Returns(new List<ProcessInfo>
                           {
                               new ProcessInfo { Id = CURRENT_PROCESS_ID }
                           });

            Subject.PreventStartIfAlreadyRunning();

            Mocker.GetMock<IBrowserService>().Verify(c => c.LaunchWebUI(), Times.Never());
        }

        [Test]
        public void should_enforce_if_another_console_is_running()
        {
            Mocker.GetMock<IProcessProvider>()
                  .Setup(c => c.FindProcessByName(ProcessProvider.PLAYARR_CONSOLE_PROCESS_NAME))
                  .Returns(new List<ProcessInfo>
                           {
                               new ProcessInfo { Id = 10 },
                               new ProcessInfo { Id = CURRENT_PROCESS_ID }
                           });

            Assert.Throws<TerminateApplicationException>(() => Subject.PreventStartIfAlreadyRunning());
            Mocker.GetMock<IBrowserService>().Verify(c => c.LaunchWebUI(), Times.Once());
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_false_if_another_gui_is_running()
        {
            Mocker.GetMock<IProcessProvider>()
                  .Setup(c => c.FindProcessByName(ProcessProvider.PLAYARR_PROCESS_NAME))
                  .Returns(new List<ProcessInfo>
                           {
                               new ProcessInfo { Id = CURRENT_PROCESS_ID },
                               new ProcessInfo { Id = 10 }
                           });

            Assert.Throws<TerminateApplicationException>(() => Subject.PreventStartIfAlreadyRunning());
            Mocker.GetMock<IBrowserService>().Verify(c => c.LaunchWebUI(), Times.Once());
            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
