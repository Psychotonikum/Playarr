import ModelBase from 'App/ModelBase';
import Game from 'Game/Game';

interface Rom extends ModelBase {
  gameId: number;
  igdbId: number;
  romFileId: number;
  platformNumber: number;
  romNumber: number;
  airDate: string;
  airDateUtc?: string;
  lastSearchTime?: string;
  runtime: number;
  absoluteRomNumber?: number;
  scenePlatformNumber?: number;
  sceneRomNumber?: number;
  sceneAbsoluteRomNumber?: number;
  overview: string;
  title: string;
  romFile?: object;
  hasFile: boolean;
  monitored: boolean;
  grabbed?: boolean;
  unverifiedSceneNumbering: boolean;
  game?: Game;
  finaleType?: string;
}

export default Rom;
