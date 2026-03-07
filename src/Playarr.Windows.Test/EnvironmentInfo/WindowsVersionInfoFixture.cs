using FluentAssertions;
using NUnit.Framework;
using Playarr.Test.Common;
using Playarr.Windows.EnvironmentInfo;

namespace Playarr.Windows.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Win")]
    public class WindowsVersionInfoFixture : TestBase<WindowsVersionInfo>
    {
        [Test]
        public void should_get_windows_version()
        {
            var info = Subject.Read();
            info.Version.Should().NotBeNullOrWhiteSpace();
            info.Name.Should().Contain("Windows");
            info.FullName.Should().Contain("Windows");
            info.FullName.Should().Contain("NT");
            info.FullName.Should().Contain(info.Version);
        }
    }
}
