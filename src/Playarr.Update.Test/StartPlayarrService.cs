using System;
using Moq;
using NUnit.Framework;
using Playarr.Common;
using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Extensions;
using Playarr.Common.Processes;
using Playarr.Test.Common;
using Playarr.Update.UpdateEngine;
using IServiceProvider = Playarr.Common.IServiceProvider;

namespace Playarr.Update.Test
{
    [TestFixture]
    public class StartPlayarrServiceFixture : TestBase<StartPlayarr>
    {
        [Test]
        public void should_start_service_if_app_type_was_serivce()
        {
            var targetFolder = "c:\\Playarr\\".AsOsAgnostic();

            Subject.Start(AppType.Service, targetFolder);

            Mocker.GetMock<IServiceProvider>().Verify(c => c.Start(ServiceProvider.SERVICE_NAME), Times.Once());
        }

        [Test]
        public void should_start_console_if_app_type_was_service_but_start_failed_because_of_permissions()
        {
            var targetFolder = "c:\\Playarr\\".AsOsAgnostic();
            var targetProcess = "c:\\Playarr\\Playarr.Console".AsOsAgnostic().ProcessNameToExe();

            Mocker.GetMock<IServiceProvider>().Setup(c => c.Start(ServiceProvider.SERVICE_NAME)).Throws(new InvalidOperationException());

            Subject.Start(AppType.Service, targetFolder);

            Mocker.GetMock<IProcessProvider>().Verify(c => c.SpawnNewProcess(targetProcess, "/" + StartupContext.NO_BROWSER, null, false), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
