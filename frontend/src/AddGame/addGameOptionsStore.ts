import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import { GameMonitor, GameType } from 'Game/Game';

export interface AddGameOptions {
  rootFolderPath: string;
  monitor: GameMonitor;
  qualityProfileId: number;
  gameType: GameType;
  platformFolder: boolean;
  searchForMissingRoms: boolean;
  searchForCutoffUnmetRoms: boolean;
  tags: number[];
}

const { useOptions, useOption, setOption } =
  createOptionsStore<AddGameOptions>('add_series_options', () => {
    return {
      rootFolderPath: '',
      monitor: 'all',
      qualityProfileId: 0,
      gameType: 'standard',
      platformFolder: true,
      searchForMissingRoms: false,
      searchForCutoffUnmetRoms: false,
      tags: [],
    };
  });

export const useAddGameOptions = useOptions;
export const useAddGameOption = useOption;
export const setAddGameOption = setOption;
