using Moq;
using NUnit.Framework;
using Playarr.Common.EnvironmentInfo;
using Playarr.Core.HealthCheck.Checks;
using Playarr.Core.Localization;
using Playarr.Core.Test.Framework;
using Playarr.Test.Common;

namespace Playarr.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class AppDataLocationFixture : CoreTest<AppDataLocationCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        [Test]
        public void should_return_warning_when_app_data_is_child_of_startup_folder()
        {
            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(@"C:\Playarr".AsOsAgnostic());

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.AppDataFolder)
                  .Returns(@"C:\Playarr\AppData".AsOsAgnostic());

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_warning_when_app_data_is_same_as_startup_folder()
        {
            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(@"C:\Playarr".AsOsAgnostic());

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.AppDataFolder)
                  .Returns(@"C:\Playarr".AsOsAgnostic());

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_ok_when_no_conflict()
        {
            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(@"C:\Playarr".AsOsAgnostic());

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.AppDataFolder)
                  .Returns(@"C:\ProgramData\Playarr".AsOsAgnostic());

            Subject.Check().ShouldBeOk();
        }
    }
}
