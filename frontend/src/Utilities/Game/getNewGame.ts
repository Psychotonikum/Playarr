import Game, {
  MonitorNewItems,
  GameMonitor,
  GameType,
} from 'Game/Game';

interface NewSeriesPayload {
  rootFolderPath: string;
  monitor: GameMonitor;
  monitorNewItems: MonitorNewItems;
  qualityProfileId: number;
  gameType: GameType;
  platformFolder: boolean;
  tags: number[];
  searchForMissingRoms?: boolean;
  searchForCutoffUnmetRoms?: boolean;
}

function getNewSeries(game: Game, payload: NewSeriesPayload) {
  const {
    rootFolderPath,
    monitor,
    monitorNewItems,
    qualityProfileId,
    gameType,
    platformFolder,
    tags,
    searchForMissingRoms = false,
    searchForCutoffUnmetRoms = false,
  } = payload;

  const addOptions = {
    monitor,
    searchForMissingRoms,
    searchForCutoffUnmetRoms,
  };

  game.addOptions = addOptions;
  game.monitored = true;
  game.monitorNewItems = monitorNewItems;
  game.qualityProfileId = qualityProfileId;
  game.rootFolderPath = rootFolderPath;
  game.gameType = gameType;
  game.platformFolder = platformFolder;
  game.tags = tags;

  return game;
}

export default getNewSeries;
