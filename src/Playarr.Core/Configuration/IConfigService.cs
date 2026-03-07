using System.Collections.Generic;
using Playarr.Common.Http.Proxy;
using Playarr.Core.ImportLists;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.Qualities;
using Playarr.Core.Security;

namespace Playarr.Core.Configuration
{
    public interface IConfigService
    {
        void SaveConfigDictionary(Dictionary<string, object> configValues);

        bool IsDefined(string key);

        // Download Client
        string DownloadClientWorkingFolders { get; set; }
        int DownloadClientHistoryLimit { get; set; }

        // Completed/Failed Download Handling (Download client)
        bool EnableCompletedDownloadHandling { get; set; }
        bool AutoRedownloadFailed { get; set; }
        bool AutoRedownloadFailedFromInteractiveSearch { get; set; }

        // Media Management
        bool AutoUnmonitorPreviouslyDownloadedEpisodes { get; set; }
        string RecycleBin { get; set; }
        int RecycleBinCleanupDays { get; set; }
        ProperDownloadTypes DownloadPropersAndRepacks { get; set; }
        bool CreateEmptyGameFolders { get; set; }
        bool DeleteEmptyFolders { get; set; }
        FileDateType FileDate { get; set; }
        bool SkipFreeSpaceCheckWhenImporting { get; set; }
        int MinimumFreeSpaceWhenImporting { get; set; }
        bool CopyUsingHardlinks { get; set; }
        bool EnableMediaInfo { get; set; }
        bool UseScriptImport { get; set; }
        string ScriptImportPath { get; set; }
        bool ImportExtraFiles { get; set; }
        string ExtraFileExtensions { get; set; }
        RescanAfterRefreshType RescanAfterRefresh { get; set; }
        RomTitleRequiredType RomTitleRequired { get; set; }
        string UserRejectedExtensions { get; set; }

        // Platform Pack Upgrade (Media Management)
        SeasonPackUpgradeType SeasonPackUpgrade { get; set; }
        double SeasonPackUpgradeThreshold { get; set; }

        // Permissions (Media Management)
        bool SetPermissionsLinux { get; set; }
        string ChmodFolder { get; set; }
        string ChownGroup { get; set; }

        // Indexers
        int Retention { get; set; }
        int RssSyncInterval { get; set; }
        int MaximumSize { get; set; }
        int MinimumAge { get; set; }

        ListSyncLevelType ListSyncLevel { get; set; }
        int ListSyncTag { get; set; }

        // UI
        int FirstDayOfWeek { get; set; }
        string CalendarWeekColumnHeader { get; set; }

        string ShortDateFormat { get; set; }
        string LongDateFormat { get; set; }
        string TimeFormat { get; set; }
        string TimeZone { get; set; }
        bool ShowRelativeDates { get; set; }
        bool EnableColorImpairedMode { get; set; }
        int UILanguage { get; set; }

        // Internal
        bool CleanupMetadataImages { get; set; }
        string PlexClientIdentifier { get; }

        // Forms Auth
        string RijndaelPassphrase { get; }
        string HmacPassphrase { get; }
        string RijndaelSalt { get; }
        string HmacSalt { get; }

        // Proxy
        bool ProxyEnabled { get; }
        ProxyType ProxyType { get; }
        string ProxyHostname { get; }
        int ProxyPort { get; }
        string ProxyUsername { get; }
        string ProxyPassword { get; }
        string ProxyBypassFilter { get; }
        bool ProxyBypassLocalAddresses { get; }

        // Backups
        string BackupFolder { get; }
        int BackupInterval { get; }
        int BackupRetention { get; }

        CertificateValidationType CertificateValidation { get; }
        string ApplicationUrl { get; }
    }
}
