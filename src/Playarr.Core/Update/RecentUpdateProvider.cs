using System.Collections.Generic;
using Playarr.Common.EnvironmentInfo;
using Playarr.Core.Configuration;
using Playarr.Core.Update.History;

namespace Playarr.Core.Update
{
    public interface IRecentUpdateProvider
    {
        List<UpdatePackage> GetRecentUpdatePackages();
    }

    public class RecentUpdateProvider : IRecentUpdateProvider
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IUpdatePackageProvider _updatePackageProvider;
        private readonly IUpdateHistoryService _updateHistoryService;

        public RecentUpdateProvider(IConfigFileProvider configFileProvider,
                                    IUpdatePackageProvider updatePackageProvider,
                                    IUpdateHistoryService updateHistoryService)
        {
            _configFileProvider = configFileProvider;
            _updatePackageProvider = updatePackageProvider;
            _updateHistoryService = updateHistoryService;
        }

        public List<UpdatePackage> GetRecentUpdatePackages()
        {
            var branch = _configFileProvider.Branch;
            var version = BuildInfo.Version;
            var prevVersion = _configFileProvider.LogDbEnabled ? _updateHistoryService.PreviouslyInstalled() : null;
            return _updatePackageProvider.GetRecentUpdates(branch, version, prevVersion);
        }
    }
}
