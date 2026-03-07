using FluentAssertions;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Mono.Disk;
using Playarr.Mono.EnvironmentInfo.VersionAdapters;
using Playarr.Test.Common;

namespace Playarr.Mono.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Linux")]
    public class ReleaseFileVersionAdapterFixture : TestBase<ReleaseFileVersionAdapter>
    {
        [SetUp]
        public void Setup()
        {
            NotBsd();

            Mocker.SetConstant<IDiskProvider>(Mocker.Resolve<DiskProvider>());
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
