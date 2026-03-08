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
    key: 'monitorSpecials',
    get value() {
      return translate('MonitorSpecialEpisodes');
    },
  },
];

export default monitorOptions;
