using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Common.EnsureThat;
using Playarr.Test.Common;

namespace Playarr.Common.Test.EnsureTest
{
    [TestFixture]
    public class PathExtensionFixture : TestBase
    {
        [TestCase(@"p:\TV Shows\file with, comma.mkv")]
        [TestCase(@"\\server\share\file with, comma.mkv")]
        public void EnsureWindowsPath(string path)
        {
            WindowsOnly();
            Ensure.That(path, () => path).IsValidPath(PathValidationType.CurrentOs);
        }

        [TestCase(@"/var/user/file with, comma.mkv")]
        public void EnsureLinuxPath(string path)
        {
            PosixOnly();
            Ensure.That(path, () => path).IsValidPath(PathValidationType.CurrentOs);
        }
    }
}
