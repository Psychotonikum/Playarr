import {
  createOptionsStore,
  PageableOptions,
} from 'Helpers/Hooks/useOptionsStore';
import translate from 'Utilities/String/translate';

const { useOptions, useOption, setOptions, setOption, setSort } =
  createOptionsStore<PageableOptions>('cutoffUnmet_options', () => {
    return {
      pageSize: 20,
      selectedFilterKey: 'monitored',
      sortKey: 'roms.airDateUtc',
      sortDirection: 'descending',
      columns: [
        {
          name: 'game.sortTitle',
          label: () => translate('GameTitle'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'rom',
          label: () => translate('Rom'),
          isVisible: true,
        },
        {
          name: 'roms.title',
          label: () => translate('RomTitle'),
          isVisible: true,
        },
        {
          name: 'roms.airDateUtc',
          label: () => translate('AirDate'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'roms.lastSearchTime',
          label: () => translate('LastSearched'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'languages',
          label: () => translate('Languages'),
          isVisible: false,
        },
        {
          name: 'status',
          label: () => translate('Status'),
          isVisible: true,
        },
        {
          name: 'actions',
          label: '',
          columnLabel: () => translate('Actions'),
          isVisible: true,
          isModifiable: false,
        },
      ],
    };
  });

export const useCutoffUnmetOptions = useOptions;
export const setCutoffUnmetOptions = setOptions;
export const useCutoffUnmetOption = useOption;
export const setCutoffUnmetOption = setOption;
export const setCutoffUnmetSort = setSort;
