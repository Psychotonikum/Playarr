using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Indexers;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Delay;
using Playarr.Core.Qualities;

namespace Playarr.Core.DecisionEngine
{
    public class DownloadDecisionComparer : IComparer<DownloadDecision>
    {
        private readonly IConfigService _configService;
        private readonly IDelayProfileService _delayProfileService;

        public delegate int CompareDelegate(DownloadDecision x, DownloadDecision y);
        public delegate int CompareDelegate<TSubject, TValue>(DownloadDecision x, DownloadDecision y);

        public DownloadDecisionComparer(IConfigService configService, IDelayProfileService delayProfileService)
        {
            _configService = configService;
            _delayProfileService = delayProfileService;
        }

        public int Compare(DownloadDecision x, DownloadDecision y)
        {
            var comparers = new List<CompareDelegate>
            {
                CompareQuality,
                CompareCustomFormatScore,
                CompareProtocol,
                CompareEpisodeCount,
                CompareRomNumber,
                CompareIndexerPriority,
                ComparePeersIfTorrent,
                CompareAgeIfUsenet,
                CompareSize
            };

            return comparers.Select(comparer => comparer(x, y)).FirstOrDefault(result => result != 0);
        }

        private int CompareBy<TSubject, TValue>(TSubject left, TSubject right, Func<TSubject, TValue> funcValue)
            where TValue : IComparable<TValue>
        {
            var leftValue = funcValue(left);
            var rightValue = funcValue(right);

            return leftValue.CompareTo(rightValue);
        }

        private int CompareByReverse<TSubject, TValue>(TSubject left, TSubject right, Func<TSubject, TValue> funcValue)
            where TValue : IComparable<TValue>
        {
            return CompareBy(left, right, funcValue) * -1;
        }

        private int CompareAll(params int[] comparers)
        {
            return comparers.Select(comparer => comparer).FirstOrDefault(result => result != 0);
        }

        private int CompareIndexerPriority(DownloadDecision x, DownloadDecision y)
        {
            return CompareByReverse(x.RemoteRom.Release, y.RemoteRom.Release, release => release.IndexerPriority);
        }

        private int CompareQuality(DownloadDecision x, DownloadDecision y)
        {
            if (_configService.DownloadPropersAndRepacks == ProperDownloadTypes.DoNotPrefer)
            {
                return CompareBy(x.RemoteRom, y.RemoteRom, remoteRom => remoteRom.Game.QualityProfile.Value.GetIndex(remoteRom.ParsedRomInfo.Quality.Quality));
            }

            return CompareAll(
                CompareBy(x.RemoteRom, y.RemoteRom, remoteRom => remoteRom.Game.QualityProfile.Value.GetIndex(remoteRom.ParsedRomInfo.Quality.Quality)),
                CompareBy(x.RemoteRom, y.RemoteRom, remoteRom => remoteRom.ParsedRomInfo.Quality.Revision));
        }

        private int CompareCustomFormatScore(DownloadDecision x, DownloadDecision y)
        {
            return CompareBy(x.RemoteRom, y.RemoteRom, remoteMovie => remoteMovie.CustomFormatScore);
        }

        private int CompareProtocol(DownloadDecision x, DownloadDecision y)
        {
            var result = CompareBy(x.RemoteRom, y.RemoteRom, remoteRom =>
            {
                var delayProfile = _delayProfileService.BestForTags(remoteRom.Game.Tags);
                var downloadProtocol = remoteRom.Release.DownloadProtocol;
                return downloadProtocol == delayProfile.PreferredProtocol;
            });

            return result;
        }

        private int CompareEpisodeCount(DownloadDecision x, DownloadDecision y)
        {
            var seasonPackCompare = CompareBy(x.RemoteRom,
                y.RemoteRom,
                remoteRom => remoteRom.ParsedRomInfo.FullSeason);

            if (seasonPackCompare != 0)
            {
                return seasonPackCompare;
            }

            return CompareByReverse(x.RemoteRom, y.RemoteRom, remoteRom => remoteRom.Roms.Count);
        }

        private int CompareRomNumber(DownloadDecision x, DownloadDecision y)
        {
            return CompareByReverse(x.RemoteRom, y.RemoteRom, remoteRom => remoteRom.Roms.Select(e => e.EpisodeNumber).MinOrDefault());
        }

        private int ComparePeersIfTorrent(DownloadDecision x, DownloadDecision y)
        {
            // Different protocols should get caught when checking the preferred protocol,
            // since we're dealing with the same game in our comparisons
            if (x.RemoteRom.Release.DownloadProtocol != DownloadProtocol.Torrent ||
                y.RemoteRom.Release.DownloadProtocol != DownloadProtocol.Torrent)
            {
                return 0;
            }

            return CompareAll(
                CompareBy(x.RemoteRom, y.RemoteRom, remoteRom =>
                {
                    var seeders = TorrentInfo.GetSeeders(remoteRom.Release);

                    return seeders.HasValue && seeders.Value > 0 ? Math.Round(Math.Log10(seeders.Value)) : 0;
                }),
                CompareBy(x.RemoteRom, y.RemoteRom, remoteRom =>
                {
                    var peers = TorrentInfo.GetPeers(remoteRom.Release);

                    return peers.HasValue && peers.Value > 0 ? Math.Round(Math.Log10(peers.Value)) : 0;
                }));
        }

        private int CompareAgeIfUsenet(DownloadDecision x, DownloadDecision y)
        {
            if (x.RemoteRom.Release.DownloadProtocol != DownloadProtocol.Usenet ||
                y.RemoteRom.Release.DownloadProtocol != DownloadProtocol.Usenet)
            {
                return 0;
            }

            return CompareBy(x.RemoteRom, y.RemoteRom, remoteRom =>
            {
                var ageHours = remoteRom.Release.AgeHours;
                var age = remoteRom.Release.Age;

                if (ageHours < 1)
                {
                    return 1000;
                }

                if (ageHours <= 24)
                {
                    return 100;
                }

                if (age <= 7)
                {
                    return 10;
                }

                return Math.Round(Math.Log10(age)) * -1;
            });
        }

        private int CompareSize(DownloadDecision x, DownloadDecision y)
        {
            var sizeCompare =  CompareBy(x.RemoteRom, y.RemoteRom, remoteRom =>
            {
                var qualityProfile = remoteRom.Game.QualityProfile.Value;
                var qualityIndex = qualityProfile.GetIndex(remoteRom.ParsedRomInfo.Quality.Quality, true);
                var qualityOrGroup = qualityProfile.Items[qualityIndex.Index];
                var item = qualityOrGroup.Quality == null ? qualityOrGroup.Items[qualityIndex.GroupIndex] : qualityOrGroup;
                var preferredSize = item.PreferredSize;

                // If no value for preferred it means unlimited so fallback to sort largest is best
                if (preferredSize.HasValue && remoteRom.Game.Runtime > 0)
                {
                    var preferredEpisodeSize = remoteRom.Game.Runtime * preferredSize.Value.Megabytes();

                    // Calculate closest to the preferred size
                    return Math.Abs((remoteRom.Release.Size - preferredEpisodeSize).Round(200.Megabytes())) * (-1);
                }
                else
                {
                    return remoteRom.Release.Size.Round(200.Megabytes());
                }
            });

            return sizeCompare;
        }
    }
}
