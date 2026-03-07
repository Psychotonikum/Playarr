import { MonitorNewItems, GameMonitor, GameType } from 'Game/Game';
import Provider from './Provider';

interface ImportList extends Provider {
  enable: boolean;
  enableAutomaticAdd: boolean;
  searchForMissingRoms: boolean;
  qualityProfileId: number;
  rootFolderPath: string;
  shouldMonitor: GameMonitor;
  monitorNewItems: MonitorNewItems;
  gameType: GameType;
  platformFolder: boolean;
  listType: string;
  listOrder: number;
  minRefreshInterval: string;
  name: string;
  tags: number[];
}

export default ImportList;
