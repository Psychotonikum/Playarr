import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';

export type GameType = 'standard';
export type GameMonitor =
  | 'all'
  | 'future'
  | 'missing'
  | 'existing'
  | 'firstPlatform'
  | 'lastPlatform'
  | 'baseGame'
  | 'allDlcs'
  | 'latestUpdate'
  | 'allAdditional'
  | 'none';

export type GameStatus = 'released' | 'continuing' | 'ended' | 'upcoming' | 'deleted';

export type MonitorNewItems = 'all' | 'none';

export type CoverType = 'poster' | 'banner' | 'fanart' | 'platform';

export interface Image {
  coverType: CoverType;
  url: string;
  remoteUrl: string;
}

export interface Statistics {
  platformCount: number;
  fileCount: number;
  downloadedFileCount: number;
  percentOfRoms: number;
  previousAiring?: Date;
  releaseGroups: string[];
  sizeOnDisk: number;
  totalFileCount: number;
  monitoredFileCount: number;
  lastAired?: string;
}

export interface Platform {
  monitored: boolean;
  platformNumber: number;
  title?: string;
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
  sceneOrigin: 'unknown' | 'unknown:igdb' | 'mixed' | 'igdb';
  comment?: string;
}

export interface GameAddOptions {
  monitor: GameMonitor;
  searchForMissingGameFiles: boolean;
  searchForCutoffUnmetGameFiles?: boolean;
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
  gameSystemId?: number;
  preferredRegions: string[];
  preferredLanguageIds: number[];
  preferredReleaseTypes: string[];
  preferredModifications: string[];
}

export default Game;
