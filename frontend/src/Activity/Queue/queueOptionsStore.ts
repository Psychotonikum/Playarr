import React from 'react';
import Icon from 'Components/Icon';
import {
  createOptionsStore,
  PageableOptions,
} from 'Helpers/Hooks/useOptionsStore';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface QueueRemovalOptions {
  removalMethod: 'changeCategory' | 'ignore' | 'removeFromClient';
  blocklistMethod: 'blocklistAndSearch' | 'blocklistOnly' | 'doNotBlocklist';
}

export interface QueueOptions extends PageableOptions {
  removalOptions: QueueRemovalOptions;
}

const { useOptions, useOption, setOptions, setOption, setSort } =
  createOptionsStore<QueueOptions>('queue_options', () => {
    return {
      pageSize: 20,
      selectedFilterKey: 'all',
      sortKey: 'time',
      sortDirection: 'descending',
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
          name: 'game.sortTitle',
          label: () => translate('Game'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'rom',
          label: () => translate('Platform'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'roms.title',
          label: () => translate('RomTitle'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'roms.airDateUtc',
          label: () => translate('EpisodeAirDate'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'languages',
          label: () => translate('Languages'),
          isSortable: true,
          isVisible: false,
        },

        {
          name: 'customFormatScore',
          columnLabel: () => translate('CustomFormatScore'),
          label: React.createElement(Icon, {
            name: icons.SCORE,
            title: () => translate('CustomFormatScore'),
          }),
          isVisible: false,
        },
        {
          name: 'protocol',
          label: () => translate('Protocol'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'indexer',
          label: () => translate('Indexer'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'downloadClient',
          label: () => translate('DownloadClient'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'title',
          label: () => translate('ReleaseTitle'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'size',
          label: () => translate('Size'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'outputPath',
          label: () => translate('OutputPath'),
          isSortable: false,
          isVisible: false,
        },
        {
          name: 'estimatedCompletionTime',
          label: () => translate('TimeLeft'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'added',
          label: () => translate('Added'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'progress',
          label: () => translate('Progress'),
          isSortable: true,
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
      removalOptions: {
        removalMethod: 'removeFromClient',
        blocklistMethod: 'doNotBlocklist',
      },
    };
  });

export const useQueueOptions = useOptions;
export const setQueueOptions = setOptions;
export const useQueueOption = useOption;
export const setQueueOption = setOption;
export const setQueueSort = setSort;
