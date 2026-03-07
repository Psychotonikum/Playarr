import ModelBase from 'App/ModelBase';
import Rom from 'Rom/Rom';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Game from 'Game/Game';
import CustomFormat from 'typings/CustomFormat';

export interface GameTitleInfo {
  title: string;
  titleWithoutYear: string;
  year: number;
  allTitles: string[];
}

export interface ParsedRomInfo {
  releaseTitle: string;
  gameTitle: string;
  gameTitleInfo: GameTitleInfo;
  quality: QualityModel;
  platformNumber: number;
  romNumbers: number[];
  absoluteRomNumbers: number[];
  specialAbsoluteRomNumbers: number[];
  languages: Language[];
  fullSeason: boolean;
  isPartialSeason: boolean;
  isMultiSeason: boolean;
  isSeasonExtra: boolean;
  special: boolean;
  releaseHash: string;
  seasonPart: number;
  releaseGroup?: string;
  releaseTokens: string;
  airDate?: string;
  isDaily: boolean;
  isAbsoluteNumbering: boolean;
  isPossibleSpecialEpisode: boolean;
  isPossibleSceneSeasonSpecial: boolean;
}

export interface ParseModel extends ModelBase {
  title: string;
  parsedRomInfo: ParsedRomInfo;
  game?: Game;
  roms: Rom[];
  languages?: Language[];
  customFormats?: CustomFormat[];
  customFormatScore?: number;
}
