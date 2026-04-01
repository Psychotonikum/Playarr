import { GameMonitor } from 'Game/Game';
import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';

export interface AddGameOptions {
  rootFolderPath: string;
  monitor: GameMonitor;
  qualityProfileId: number;
  languageProfileId: number;
  searchForMissingRoms: boolean;
  tags: number[];
  preferredRegions: string[];
  preferredLanguageIds: number[];
  preferredReleaseTypes: string[];
  preferredModifications: string[];
}

const { useOptions, useOption, setOption } = createOptionsStore<AddGameOptions>(
  'add_game_options',
  () => {
    return {
      rootFolderPath: '',
      monitor: 'all',
      qualityProfileId: 0,
      languageProfileId: 0,
      searchForMissingRoms: false,
      tags: [],
      preferredRegions: ['Europe'],
      preferredLanguageIds: [],
      preferredReleaseTypes: ['Retail'],
      preferredModifications: ['Original'],
    };
  },
  {
    version: 2,
    migrate: (persistedState: unknown, version: number) => {
      const state = persistedState as Record<string, unknown>;

      if (version < 2) {
        const releaseTypes = state.preferredReleaseTypes as string[] | undefined;
        const modifications = state.preferredModifications as string[] | undefined;

        if (!releaseTypes || releaseTypes.length === 0) {
          state.preferredReleaseTypes = ['Retail'];
        }

        if (!modifications || modifications.length === 0) {
          state.preferredModifications = ['Original'];
        }

        delete state.platformFolder;
      }

      return state as unknown as AddGameOptions;
    },
  }
);

export const useAddGameOptions = useOptions;
export const useAddGameOption = useOption;
export const setAddGameOption = setOption;
