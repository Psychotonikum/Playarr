import { GameMonitor } from 'Game/Game';
import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';

export interface AddGameOptions {
  rootFolderPath: string;
  monitor: GameMonitor;
  qualityProfileId: number;
  languageProfileId: number;
  platformFolder: boolean;
  searchForMissingRoms: boolean;
  tags: number[];
  preferredRegions: string[];
  preferredLanguageIds: number[];
  preferredReleaseTypes: string[];
  preferredModifications: string[];
}

const { useOptions, useOption, setOption } = createOptionsStore<AddGameOptions>(
  'add_series_options',
  () => {
    return {
      rootFolderPath: '',
      monitor: 'all',
      qualityProfileId: 0,
      languageProfileId: 0,
      platformFolder: true,
      searchForMissingRoms: false,
      tags: [],
      preferredRegions: [],
      preferredLanguageIds: [],
      preferredReleaseTypes: [],
      preferredModifications: [],
    };
  }
);

export const useAddGameOptions = useOptions;
export const useAddGameOption = useOption;
export const setAddGameOption = setOption;
