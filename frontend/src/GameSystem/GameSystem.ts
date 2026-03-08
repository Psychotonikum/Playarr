import ModelBase from 'App/ModelBase';

export type GameSystemType = 'classic' | 'patchable';

interface GameSystem extends ModelBase {
  name: string;
  folderName: string;
  systemType: number;
  fileExtensions: string[];
  namingFormat: string;
  updateNamingFormat: string;
  dlcNamingFormat: string;
  baseFolderName: string;
  updateFolderName: string;
  dlcFolderName: string;
  tags: number[];
}

export default GameSystem;
