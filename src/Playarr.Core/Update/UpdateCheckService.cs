using Playarr.Common.EnvironmentInfo;
using Playarr.Core.Configuration;

namespace Playarr.Core.Update
{
    public interface ICheckUpdateService
    {
        UpdatePackage AvailableUpdate();
    }

    public class CheckUpdateService : ICheckUpdateService
    {
        private readonly IUpdatePackageProvider _updatePackageProvider;
        private readonly IConfigFileProvider _configFileProvider;

        public CheckUpdateService(IUpdatePackageProvider updatePackageProvider,
                                  IConfigFileProvider configFileProvider)
        {
            _updatePackageProvider = updatePackageProvider;
            _configFileProvider = configFileProvider;
        }

        public UpdatePackage AvailableUpdate()
        {
            return _updatePackageProvider.GetLatestUpdate(_configFileProvider.Branch, BuildInfo.Version);
        }
    }
}
