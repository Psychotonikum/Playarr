import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import { SortDirection } from 'Helpers/Props/sortDirections';

interface EpisodeSelectOptions {
  sortKey: string;
  sortDirection: SortDirection;
}

const { useOptions, useOption, setOptions, setOption, setSort } =
  createOptionsStore<EpisodeSelectOptions>('episode_selection_options', () => {
    return {
      sortKey: 'romNumber',
      sortDirection: 'ascending',
    };
  });

export const useRomSelectionOptions = useOptions;
export const setEpisodeSelectionOptions = setOptions;
export const useRomSelectionOption = useOption;
export const setEpisodeSelectionOption = setOption;
export const setEpisodeSelectionSort = setSort;
