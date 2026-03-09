import ModelBase from 'App/ModelBase';
import ReleaseType from 'InteractiveImport/ReleaseType';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';
import MediaInfo from 'typings/MediaInfo';

export interface RomFile extends ModelBase {
  gameId: number;
  platformNumber: number;
  relativePath: string;
  path: string;
  size: number;
  dateAdded: string;
  sceneName: string;
  releaseGroup: string;
  languages: Language[];
  region?: string;
  crcHash?: string;
  quality: QualityModel;
  customFormats: CustomFormat[];
  customFormatScore: number;
  indexerFlags: number;
  releaseType: ReleaseType;
  romFileType: number;
  patchVersion?: string;
  dlcIndex?: string;
  revision?: string;
  dumpQuality: number;
  modification: number;
  modificationName?: string;
  romReleaseType: number;
  mediaInfo: MediaInfo;
  qualityCutoffNotMet: boolean;
}
