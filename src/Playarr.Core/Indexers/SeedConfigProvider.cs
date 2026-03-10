using System;
using Playarr.Core.Download.Clients;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Indexers
{
    public interface ISeedConfigProvider
    {
        TorrentSeedConfiguration GetSeedConfiguration(RemoteRom release);
        TorrentSeedConfiguration GetSeedConfiguration(int indexerId, bool fullSeason);
    }

    public class SeedConfigProvider : ISeedConfigProvider
    {
        private readonly ICachedIndexerSettingsProvider _cachedIndexerSettingsProvider;

        public SeedConfigProvider(ICachedIndexerSettingsProvider cachedIndexerSettingsProvider)
        {
            _cachedIndexerSettingsProvider = cachedIndexerSettingsProvider;
        }

        public TorrentSeedConfiguration GetSeedConfiguration(RemoteRom remoteRom)
        {
            if (remoteRom.Release.DownloadProtocol != DownloadProtocol.Torrent)
            {
                return null;
            }

            if (remoteRom.Release.IndexerId == 0)
            {
                return null;
            }

            return GetSeedConfiguration(remoteRom.Release.IndexerId, remoteRom.ParsedRomInfo.FullSeason);
        }

        public TorrentSeedConfiguration GetSeedConfiguration(int indexerId, bool fullSeason)
        {
            if (indexerId == 0)
            {
                return null;
            }

            var settings = _cachedIndexerSettingsProvider.GetSettings(indexerId);
            var seedCriteria = settings?.SeedCriteriaSettings;

            if (seedCriteria == null)
            {
                return null;
            }

            var useSeasonPackSeedGoal = (SeasonPackSeedGoal)seedCriteria.SeasonPackSeedGoal == SeasonPackSeedGoal.UseSeasonPackSeedGoal;

            var seedConfig = new TorrentSeedConfiguration
            {
                Ratio = (fullSeason && useSeasonPackSeedGoal)
                    ? seedCriteria.SeasonPackSeedRatio
                    : seedCriteria.SeedRatio
            };

            var seedTime = (fullSeason && useSeasonPackSeedGoal) ? seedCriteria.SeasonPackSeedTime : seedCriteria.SeedTime;
            if (seedTime.HasValue)
            {
                seedConfig.SeedTime = TimeSpan.FromMinutes(seedTime.Value);
            }

            return seedConfig;
        }
    }
}
