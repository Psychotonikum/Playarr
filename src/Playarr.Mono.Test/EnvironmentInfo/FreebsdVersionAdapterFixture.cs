using FluentAssertions;
using NUnit.Framework;
using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Processes;
using Playarr.Mono.EnvironmentInfo.VersionAdapters;
using Playarr.Test.Common;

namespace Playarr.Mono.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Linux")]
    public class FreebsdVersionAdapterFixture : TestBase<FreebsdVersionAdapter>
    {
        [SetUp]
        public void Setup()
        {
            if (OsInfo.Os != Os.Bsd)
            {
                throw new IgnoreException("BSD Only");
            }

            Mocker.SetConstant<IProcessProvider>(Mocker.Resolve<ProcessProvider>());
        }

        [Test]
        public void should_get_version_info()
        {
            var info = Subject.Read();
            info.FullName.Should().NotBeNullOrWhiteSpace();
            info.Name.Should().NotBeNullOrWhiteSpace();
            info.Version.Should().NotBeNullOrWhiteSpace();
        }
    }
}
