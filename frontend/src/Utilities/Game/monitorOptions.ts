import { GameMonitor } from 'Game/Game';
import translate from 'Utilities/String/translate';

interface MonitorOption {
  key: GameMonitor;
  value: string;
}

const monitorOptions: MonitorOption[] = [
  {
    key: 'all',
    get value() {
      return translate('MonitorAllEpisodes');
    },
  },
  {
    key: 'future',
    get value() {
      return translate('MonitorFutureEpisodes');
    },
  },
  {
    key: 'missing',
    get value() {
      return translate('MonitorMissingEpisodes');
    },
  },
  {
    key: 'existing',
    get value() {
      return translate('MonitorExistingEpisodes');
    },
  },
  {
    key: 'recent',
    get value() {
      return translate('MonitorRecentEpisodes');
    },
  },
  {
    key: 'pilot',
    get value() {
      return translate('MonitorPilotEpisode');
    },
  },
  {
    key: 'firstPlatform',
    get value() {
      return translate('MonitorFirstSeason');
    },
  },
  {
    key: 'lastPlatform',
    get value() {
      return translate('MonitorLastSeason');
    },
  },
  {
    key: 'monitorSpecials',
    get value() {
      return translate('MonitorSpecialEpisodes');
    },
  },
  {
    key: 'unmonitorSpecials',
    get value() {
      return translate('UnmonitorSpecialEpisodes');
    },
  },
  {
    key: 'none',
    get value() {
      return translate('MonitorNoEpisodes');
    },
  },
];

export default monitorOptions;
