using Playarr.Common.Disk;
using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Extensions;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class CleanupTemporaryUpdateFiles : IHousekeepingTask
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAppFolderInfo _appFolderInfo;

        public CleanupTemporaryUpdateFiles(IDiskProvider diskProvider, IAppFolderInfo appFolderInfo)
        {
            _diskProvider = diskProvider;
            _appFolderInfo = appFolderInfo;
        }

        public void Clean()
        {
            var updateSandboxFolder = _appFolderInfo.GetUpdateSandboxFolder();

            if (_diskProvider.FolderExists(updateSandboxFolder))
            {
                _diskProvider.DeleteFolder(updateSandboxFolder, true);
            }
        }
    }
}
