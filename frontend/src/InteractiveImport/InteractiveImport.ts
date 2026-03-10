import ModelBase from 'App/ModelBase';
import Game from 'Game/Game';
import ReleaseType from 'InteractiveImport/ReleaseType';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Rom from 'Rom/Rom';
import CustomFormat from 'typings/CustomFormat';
import Rejection from 'typings/Rejection';

export interface InteractiveImportCommandOptions {
  path: string;
  folderName: string;
  gameId: number;
  romIds: number[];
  releaseGroup?: string;
  quality: QualityModel;
  languages: Language[];
  indexerFlags: number;
  releaseType: ReleaseType;
  downloadId?: string;
  romFileId?: number;
}

interface InteractiveImport extends ModelBase {
  path: string;
  relativePath: string;
  folderName: string;
  name: string;
  size: number;
  releaseGroup: string;
  quality: QualityModel;
  languages: Language[];
  game?: Game;
  platformNumber: number;
  roms: Rom[];
  qualityWeight: number;
  customFormats: CustomFormat[];
  indexerFlags: number;
  releaseType: ReleaseType;
  rejections: Rejection[];
  romFileId?: number;
  downloadId?: string;
}

export default InteractiveImport;
