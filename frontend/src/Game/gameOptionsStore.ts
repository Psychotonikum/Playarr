import Column from 'Components/Table/Column';
import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import translate from 'Utilities/String/translate';

export interface SeriesOptions {
  selectedFilterKey: string | number;
  sortKey: string;
  sortDirection: 'ascending' | 'descending';
  view: string;
  columns: Column[];
  posterOptions: {
    detailedProgressBar: boolean;
    size: 'small' | 'medium' | 'large';
    showTitle: boolean;
    showMonitored: boolean;
    showQualityProfile: boolean;
    showTags: boolean;
    showSearchAction: boolean;
  };
  overviewOptions: {
    detailedProgressBar: boolean;
    size: 'small' | 'medium' | 'large';
    showMonitored: boolean;
    showQualityProfile: boolean;
    showAdded: boolean;
    showPlatformCount: boolean;
    showPath: boolean;
    showSizeOnDisk: boolean;
    showTags: boolean;
    showSearchAction: boolean;
  };
  tableOptions: {
    showBanners: boolean;
    showSearchAction: boolean;
  };
  deleteOptions: {
    addImportListExclusion: boolean;
  };
}

const { useOptions, useOption, setOptions, setOption, setSort, getOptions } =
  createOptionsStore<SeriesOptions>('series_options', () => {
    return {
      selectedFilterKey: 'all',
      sortKey: 'sortTitle',
      sortDirection: 'ascending',
      secondarySortKey: 'sortTitle',
      secondarySortDirection: 'ascending',
      view: 'posters',
      posterOptions: {
        detailedProgressBar: false,
        size: 'large',
        showTitle: false,
        showMonitored: true,
        showQualityProfile: true,
        showTags: false,
        showSearchAction: false,
      },
      overviewOptions: {
        detailedProgressBar: false,
        size: 'medium',
        showMonitored: true,
        showQualityProfile: true,
        showAdded: false,
        showPlatformCount: true,
        showPath: false,
        showSizeOnDisk: false,
        showTags: false,
        showSearchAction: false,
      },
      tableOptions: {
        showBanners: false,
        showSearchAction: false,
      },
      deleteOptions: {
        addImportListExclusion: false,
      },
      columns: [
        {
          name: 'status',
          label: '',
          columnLabel: () => translate('Status'),
          isSortable: true,
          isVisible: true,
          isModifiable: false,
        },
        {
          name: 'sortTitle',
          label: () => translate('GameTitle'),
          isSortable: true,
          isVisible: true,
          isModifiable: false,
        },
        {
          name: 'qualityProfileId',
          label: () => translate('QualityProfile'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'originalLanguage',
          label: () => translate('OriginalLanguage'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'added',
          label: () => translate('Added'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'platformCount',
          label: () => translate('Platforms'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'platformFolder',
          label: () => translate('PlatformFolder'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'romProgress',
          label: () => translate('Roms'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'romCount',
          label: () => translate('RomCount'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'year',
          label: () => translate('Year'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'path',
          label: () => translate('Path'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'sizeOnDisk',
          label: () => translate('SizeOnDisk'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'genres',
          label: () => translate('Genres'),
          isSortable: false,
          isVisible: false,
        },
        {
          name: 'ratings',
          label: () => translate('Rating'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'releaseGroups',
          label: () => translate('ReleaseGroups'),
          isSortable: false,
          isVisible: false,
        },
        {
          name: 'tags',
          label: () => translate('Tags'),
          isSortable: true,
          isVisible: false,
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

export const useGameOptions = useOptions;
export const setGameOptions = setOptions;
export const useGameOption = useOption;
export const setGameOption = setOption;
export const setGameSort = setSort;

export const useGamePosterOptions = () => useOption('posterOptions');
export const setGamePosterOptions = (
  options: Partial<SeriesOptions['posterOptions']>
) => {
  const currentOptions = getOptions().posterOptions;
  setGameOption('posterOptions', { ...currentOptions, ...options });
};

export const useGameOverviewOptions = () => useOption('overviewOptions');
export const setGameOverviewOptions = (
  options: Partial<SeriesOptions['overviewOptions']>
) => {
  const currentOptions = getOptions().overviewOptions;
  setGameOption('overviewOptions', { ...currentOptions, ...options });
};

export const useGameTableOptions = () => useOption('tableOptions');
export const setGameTableOptions = (
  options: Partial<SeriesOptions['tableOptions']>
) => {
  const currentOptions = getOptions().tableOptions;
  setGameOption('tableOptions', { ...currentOptions, ...options });
};

export const useGameDeleteOptions = () => useOption('deleteOptions');
export const setGameDeleteOptions = (
  options: Partial<SeriesOptions['deleteOptions']>
) => {
  const currentOptions = getOptions().deleteOptions;
  setGameOption('deleteOptions', { ...currentOptions, ...options });
};
