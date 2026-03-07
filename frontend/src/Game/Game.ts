import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';

export type GameType = 'anime' | 'daily' | 'standard';
export type GameMonitor =
  | 'all'
  | 'future'
  | 'missing'
  | 'existing'
  | 'recent'
  | 'pilot'
  | 'firstPlatform'
  | 'lastPlatform'
  | 'monitorSpecials'
  | 'unmonitorSpecials'
  | 'none';

export type GameStatus = 'continuing' | 'ended' | 'upcoming' | 'deleted';

export type MonitorNewItems = 'all' | 'none';

export type CoverType = 'poster' | 'banner' | 'fanart' | 'platform';

export interface Image {
  coverType: CoverType;
  url: string;
  remoteUrl: string;
}

export interface Statistics {
  platformCount: number;
  romCount: number;
  romFileCount: number;
  percentOfRoms: number;
  previousAiring?: Date;
  releaseGroups: string[];
  sizeOnDisk: number;
  totalRomCount: number;
  monitoredRomCount: number;
  lastAired?: string;
}

export interface Platform {
  monitored: boolean;
  platformNumber: number;
  statistics: Statistics;
}

export interface Ratings {
  votes: number;
  value: number;
}

export interface AlternateTitle {
  platformNumber: number;
  scenePlatformNumber?: number;
  title: string;
  sceneOrigin: 'unknown' | 'unknown:tvdb' | 'mixed' | 'tvdb';
  comment?: string;
}

export interface GameAddOptions {
  monitor: GameMonitor;
  searchForMissingRoms: boolean;
  searchForCutoffUnmetRoms: boolean;
}

interface Game extends ModelBase {
  added: string;
  alternateTitles: AlternateTitle[];
  certification: string;
  cleanTitle: string;
  ended: boolean;
  firstAired: string;
  genres: string[];
  images: Image[];
  imdbId?: string;
  monitored: boolean;
  monitorNewItems: MonitorNewItems;
  network: string;
  originalCountry: string;
  originalLanguage: Language;
  overview: string;
  path: string;
  previousAiring?: string;
  nextAiring?: string;
  qualityProfileId: number;
  ratings: Ratings;
  rootFolderPath: string;
  runtime: number;
  platformFolder: boolean;
  platforms: Platform[];
  gameType: GameType;
  sortTitle: string;
  statistics?: Statistics;
  status: GameStatus;
  tags: number[];
  title: string;
  titleSlug: string;
  igdbId: number;
  rawgId: number;
  mobyGamesId: number;
  tmdbId: number;
  useSceneNumbering: boolean;
  year: number;
  addOptions: GameAddOptions;
}

export default Game;
