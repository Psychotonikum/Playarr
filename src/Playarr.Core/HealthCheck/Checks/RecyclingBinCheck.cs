using System.Collections.Generic;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.MediaFiles.Events;

namespace Playarr.Core.HealthCheck.Checks
{
    [CheckOn(typeof(EpisodeImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(EpisodeImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class RecyclingBinCheck : HealthCheckBase
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;

        public RecyclingBinCheck(IConfigService configService, IDiskProvider diskProvider, ILocalizationService localizationService)
            : base(localizationService)
        {
            _configService = configService;
            _diskProvider = diskProvider;
        }

        public override HealthCheck Check()
        {
            var recycleBin = _configService.RecycleBin;

            if (recycleBin.IsNullOrWhiteSpace())
            {
                return new HealthCheck(GetType());
            }

            if (!_diskProvider.FolderWritable(recycleBin))
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.RecycleBinUnableToWrite,
                    _localizationService.GetLocalizedString("RecycleBinUnableToWriteHealthCheckMessage", new Dictionary<string, object>
                    {
                        { "path", recycleBin }
                    }),
                    "#cannot-write-recycle-bin");
            }

            return new HealthCheck(GetType());
        }
    }
}
