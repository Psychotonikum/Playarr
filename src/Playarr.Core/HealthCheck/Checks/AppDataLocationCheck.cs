using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Extensions;
using Playarr.Core.Localization;

namespace Playarr.Core.HealthCheck.Checks
{
    public class AppDataLocationCheck : HealthCheckBase
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public AppDataLocationCheck(IAppFolderInfo appFolderInfo, ILocalizationService localizationService)
            : base(localizationService)
        {
            _appFolderInfo = appFolderInfo;
        }

        public override HealthCheck Check()
        {
            if (_appFolderInfo.StartUpFolder.IsParentPath(_appFolderInfo.AppDataFolder) ||
                _appFolderInfo.StartUpFolder.PathEquals(_appFolderInfo.AppDataFolder))
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    HealthCheckReason.AppDataLocation,
                    _localizationService.GetLocalizedString("AppDataLocationHealthCheckMessage"),
                    "#updating-will-not-be-possible-to-prevent-deleting-appdata-on-update");
            }

            return new HealthCheck(GetType());
        }
    }
}
