import { useQueryClient } from '@tanstack/react-query';
import moment from 'moment';
import { useCallback, useMemo } from 'react';
import { FilterBuilderTag } from 'Components/Filter/Builder/FilterBuilderRowValue';
import { Filter, FilterBuilderProp } from 'Filters/Filter';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import {
  filterBuilderTypes,
  filterBuilderValueTypes,
  sortDirections,
} from 'Helpers/Props';
import { FilterType } from 'Helpers/Props/filterTypes';
import getFilterTypePredicate from 'Helpers/Props/getFilterTypePredicate';
import { SortDirection } from 'Helpers/Props/sortDirections';
import sortByProp from 'Utilities/Array/sortByProp';
import clientSideFilterAndSort from 'Utilities/Filter/clientSideFilterAndSort';
import translate from 'Utilities/String/translate';
import Game, { Statistics } from './Game';
import { useGameOptions } from './gameOptionsStore';

// Date filter predicate helper
const dateFilterPredicate = (
  itemDate: string | undefined,
  filterValue: string | Date,
  type: FilterType
): boolean => {
  if (!itemDate) return false;
  const predicate = getFilterTypePredicate(type);
  return predicate(itemDate, filterValue);
};

export const FILTERS: Filter[] = [
  {
    key: 'all',
    label: () => translate('All'),
    filters: [],
  },
  {
    key: 'monitored',
    label: () => translate('MonitoredOnly'),
    filters: [
      {
        key: 'monitored',
        value: [true],
        type: 'equal',
      },
    ],
  },
  {
    key: 'unmonitored',
    label: () => translate('UnmonitoredOnly'),
    filters: [
      {
        key: 'monitored',
        value: [false],
        type: 'equal',
      },
    ],
  },
  {
    key: 'continuing',
    label: () => translate('ContinuingOnly'),
    filters: [
      {
        key: 'status',
        value: 'continuing',
        type: 'equal',
      },
    ],
  },
  {
    key: 'ended',
    label: () => translate('EndedOnly'),
    filters: [
      {
        key: 'status',
        value: 'ended',
        type: 'equal',
      },
    ],
  },
  {
    key: 'missing',
    label: () => translate('MissingEpisodes'),
    filters: [
      {
        key: 'missing',
        value: [true],
        type: 'equal',
      },
    ],
  },
];

const SORT_PREDICATES = {
  status: (item: Game, _direction: SortDirection) => {
    let result = 0;

    if (item.monitored) {
      result += 2;
    }

    if (item.status === 'continuing') {
      result++;
    }

    return result;
  },

  sizeOnDisk: (item: Game, _direction: SortDirection) => {
    return item.statistics?.sizeOnDisk ?? 0;
  },

  network: (item: Game, _direction: SortDirection) => {
    const network = item.network;

    return network ? network.toLowerCase() : '';
  },

  nextAiring: (item: Game, direction: SortDirection) => {
    const nextAiring = item.nextAiring;

    if (nextAiring) {
      return moment(nextAiring).unix();
    }

    if (direction === sortDirections.DESCENDING) {
      return 0;
    }

    return Number.MAX_VALUE;
  },

  previousAiring: (item: Game, direction: SortDirection) => {
    const previousAiring = item.previousAiring;

    if (previousAiring) {
      return moment(previousAiring).unix();
    }

    if (direction === sortDirections.DESCENDING) {
      return -Number.MAX_VALUE;
    }

    return Number.MAX_VALUE;
  },

  romProgress: (item: Game, _direction: SortDirection) => {
    const statistics = item.statistics;

    const romCount = statistics?.romCount ?? 0;
    const romFileCount = statistics?.romFileCount ?? 0;

    const progress = romCount
      ? (romFileCount / romCount) * 100
      : 100;

    return progress + romCount / 1000000;
  },

  romCount: (item: Game, _direction: SortDirection) => {
    return item.statistics?.totalRomCount ?? 0;
  },

  platformCount: (item: Game, _direction: SortDirection) => {
    return item.statistics?.platformCount ?? 0;
  },

  originalLanguage: (item: Game, _direction: SortDirection) => {
    const { originalLanguage } = item;

    return originalLanguage?.name ?? '';
  },

  ratings: (item: Game, _direction: SortDirection) => {
    const { ratings } = item;

    return ratings.value ?? 0;
  },

  monitorNewItems: (item: Game, _direction: SortDirection) => {
    return item.monitorNewItems === 'all' ? 1 : 0;
  },
} as const;

const FILTER_PREDICATES = {
  romProgress: (item: Game, filterValue: number, type: FilterType) => {
    const statistics = item.statistics;
    const romCount = statistics?.romCount ?? 0;
    const romFileCount = statistics?.romFileCount ?? 0;

    const progress = romCount
      ? (romFileCount / romCount) * 100
      : 100;

    const predicate = getFilterTypePredicate(type);
    return predicate(progress, filterValue);
  },

  missing: (item: Game, _filterValue: boolean, _type: FilterType) => {
    const statistics = item.statistics;
    const romCount = statistics?.romCount ?? 0;
    const romFileCount = statistics?.romFileCount ?? 0;
    return romCount - romFileCount > 0;
  },

  nextAiring: (item: Game, filterValue: string | Date, type: FilterType) => {
    return dateFilterPredicate(item.nextAiring, filterValue, type);
  },

  previousAiring: (
    item: Game,
    filterValue: string | Date,
    type: FilterType
  ) => {
    return dateFilterPredicate(item.previousAiring, filterValue, type);
  },

  added: (item: Game, filterValue: string | Date, type: FilterType) => {
    return dateFilterPredicate(item.added, filterValue, type);
  },

  ratings: (item: Game, filterValue: number, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const value = item.ratings.value ?? 0;
    return predicate(value * 10, filterValue);
  },

  ratingVotes: (item: Game, filterValue: number, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const votes = item.ratings.votes ?? 0;
    return predicate(votes, filterValue);
  },

  originalLanguage: (item: Game, filterValue: string, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const languageName = item.originalLanguage?.name ?? '';
    return predicate(languageName, filterValue);
  },

  releaseGroups: (item: Game, filterValue: string[], type: FilterType) => {
    const releaseGroups = item.statistics?.releaseGroups ?? [];
    const predicate = getFilterTypePredicate(type);
    return predicate(releaseGroups, filterValue);
  },

  platformCount: (item: Game, filterValue: number, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const platformCount = item.statistics?.platformCount ?? 0;
    return predicate(platformCount, filterValue);
  },

  sizeOnDisk: (item: Game, filterValue: number, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const sizeOnDisk = item.statistics?.sizeOnDisk ?? 0;
    return predicate(sizeOnDisk, filterValue);
  },

  hasMissingSeason: (item: Game, filterValue: boolean, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const platforms = item.platforms ?? [];

    const hasMissingSeason = platforms.some((platform) => {
      const { platformNumber } = platform;
      const statistics = platform.statistics;
      const romFileCount = statistics?.romFileCount ?? 0;
      const romCount = statistics?.romCount ?? 0;
      const totalRomCount = statistics?.totalRomCount ?? 0;

      return (
        platformNumber > 0 &&
        totalRomCount > 0 &&
        romCount === totalRomCount &&
        romFileCount === 0
      );
    });

    return predicate(hasMissingSeason, filterValue);
  },

  seasonsMonitoredStatus: (
    item: Game,
    filterValue: string,
    type: FilterType
  ) => {
    const predicate = getFilterTypePredicate(type);
    const platforms = item.platforms ?? [];

    const { monitoredCount, unmonitoredCount } = platforms.reduce(
      (acc, { platformNumber, monitored }) => {
        if (platformNumber <= 0) {
          return acc;
        }

        if (monitored) {
          acc.monitoredCount++;
        } else {
          acc.unmonitoredCount++;
        }

        return acc;
      },
      { monitoredCount: 0, unmonitoredCount: 0 }
    );

    let seasonsMonitoredStatus = 'partial';

    if (monitoredCount === 0) {
      seasonsMonitoredStatus = 'none';
    } else if (unmonitoredCount === 0) {
      seasonsMonitoredStatus = 'all';
    }

    return predicate(seasonsMonitoredStatus, filterValue);
  },

  episodesMonitoredStatus: (
    item: Game,
    filterValue: string,
    type: FilterType
  ) => {
    const predicate = getFilterTypePredicate(type);
    const { platforms, statistics = {} as Statistics } = item;
    const { monitoredRomCount = 0, totalRomCount = 0 } = statistics;
    const specials = platforms?.find((s) => s.platformNumber === 0);

    // The monitored count and total count include specials, but those areskipped
    // for platforms monitored status so we should to exclude them here too.

    const monitoredCount =
      monitoredRomCount -
      (specials?.statistics?.monitoredRomCount ?? 0);

    const totalCount =
      totalRomCount - (specials?.statistics?.totalRomCount ?? 0);

    let episodesMonitoredStatus = 'partial';

    if (monitoredCount === 0) {
      episodesMonitoredStatus = 'none';
    } else if (totalCount - monitoredCount === 0) {
      episodesMonitoredStatus = 'all';
    }

    return predicate(episodesMonitoredStatus, filterValue);
  },
} as const;

export const FILTER_BUILDER: FilterBuilderProp<Game>[] = [
  {
    name: 'monitored',
    label: () => translate('Monitored'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.BOOL,
  },
  {
    name: 'status',
    label: () => translate('Status'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.SERIES_STATUS,
  },
  {
    name: 'gameType',
    label: () => translate('Type'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.SERIES_TYPES,
  },
  {
    name: 'title',
    label: () => translate('Title'),
    type: filterBuilderTypes.STRING,
  },
  {
    name: 'network',
    label: () => translate('Network'),
    type: filterBuilderTypes.ARRAY,
    optionsSelector: function (items: Game[]) {
      const tagList = items.reduce<FilterBuilderTag<string, string>[]>(
        (acc, game) => {
          if (game.network) {
            acc.push({
              id: game.network,
              name: game.network,
            });
          }

          return acc;
        },
        []
      );

      return tagList.sort(sortByProp('name'));
    },
  },
  {
    name: 'qualityProfileId',
    label: () => translate('QualityProfile'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.QUALITY_PROFILE,
  },
  {
    name: 'nextAiring',
    label: () => translate('NextAiring'),
    type: filterBuilderTypes.DATE,
    valueType: filterBuilderValueTypes.DATE,
  },
  {
    name: 'previousAiring',
    label: () => translate('PreviousAiring'),
    type: filterBuilderTypes.DATE,
    valueType: filterBuilderValueTypes.DATE,
  },
  {
    name: 'added',
    label: () => translate('Added'),
    type: filterBuilderTypes.DATE,
    valueType: filterBuilderValueTypes.DATE,
  },
  {
    name: 'platformCount',
    label: () => translate('PlatformCount'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'romProgress',
    label: () => translate('RomProgress'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'path',
    label: () => translate('Path'),
    type: filterBuilderTypes.STRING,
  },
  {
    name: 'rootFolderPath',
    label: () => translate('RootFolderPath'),
    type: filterBuilderTypes.EXACT,
  },
  {
    name: 'sizeOnDisk',
    label: () => translate('SizeOnDisk'),
    type: filterBuilderTypes.NUMBER,
    valueType: filterBuilderValueTypes.BYTES,
  },
  {
    name: 'genres',
    label: () => translate('Genres'),
    type: filterBuilderTypes.ARRAY,
    optionsSelector: function (items: Game[]) {
      const tagList = items.reduce<FilterBuilderTag<string, string>[]>(
        (acc, game) => {
          game.genres.forEach((genre) => {
            acc.push({
              id: genre,
              name: genre,
            });
          });

          return acc;
        },
        []
      );

      return tagList.sort(sortByProp('name'));
    },
  },
  {
    name: 'originalLanguage',
    label: () => translate('OriginalLanguage'),
    type: filterBuilderTypes.EXACT,
    optionsSelector: function (items: Game[]) {
      const languageList = items.reduce<FilterBuilderTag<string, string>[]>(
        (acc, game) => {
          if (game.originalLanguage) {
            acc.push({
              id: game.originalLanguage.name,
              name: game.originalLanguage.name,
            });
          }

          return acc;
        },
        []
      );

      return languageList.sort(sortByProp('name'));
    },
  },
  {
    name: 'releaseGroups',
    label: () => translate('ReleaseGroups'),
    type: filterBuilderTypes.ARRAY,
  },
  {
    name: 'ratings',
    label: () => translate('Rating'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'ratingVotes',
    label: () => translate('RatingVotes'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'certification',
    label: () => translate('Certification'),
    type: filterBuilderTypes.EXACT,
  },
  {
    name: 'tags',
    label: () => translate('Tags'),
    type: filterBuilderTypes.ARRAY,
    valueType: filterBuilderValueTypes.TAG,
  },
  {
    name: 'hasMissingSeason',
    label: () => translate('HasMissingSeason'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.BOOL,
  },
  {
    name: 'seasonsMonitoredStatus',
    label: () => translate('SeasonsMonitoredStatus'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.MONITORED_STATUS,
  },
  {
    name: 'episodesMonitoredStatus',
    label: () => translate('EpisodesMonitoredStatus'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.MONITORED_STATUS,
  },
  {
    name: 'year',
    label: () => translate('Year'),
    type: filterBuilderTypes.NUMBER,
  },
];

const DEFAULT_SERIES: Game[] = [];

const useGame = () => {
  const { data, ...result } = useApiQuery<Game[]>({
    path: '/game',
    queryOptions: {
      staleTime: 5 * 60 * 1000,
      gcTime: Infinity,
    },
  });

  const seriesMap = useMemo(() => {
    if (!data) {
      return new Map<number, Game>();
    }

    return new Map<number, Game>(data.map((game) => [game.id, game]));
  }, [data]);

  return {
    ...result,
    data: data ?? DEFAULT_SERIES,
    seriesMap,
  };
};

export default useGame;

export const useGameIndex = () => {
  const { selectedFilterKey, sortKey, sortDirection } = useGameOptions();
  const { data: seriesData = [], ...queryResult } = useGame();
  const customFilters = useCustomFiltersList('game');

  const data = useMemo(() => {
    return clientSideFilterAndSort<
      Game,
      typeof FILTER_PREDICATES,
      typeof SORT_PREDICATES
    >(seriesData, {
      selectedFilterKey,
      filters: FILTERS,
      filterPredicates: FILTER_PREDICATES,
      customFilters,
      sortKey: sortKey as keyof Game,
      sortDirection,
      secondarySortKey: 'sortTitle',
      secondarySortDirection: 'ascending',
      sortPredicates: SORT_PREDICATES,
    });
  }, [customFilters, seriesData, selectedFilterKey, sortKey, sortDirection]);

  return {
    ...queryResult,
    data: data.data,
    totalItems: data.totalItems,
  };
};

export const useHasSeries = () => {
  const { data: seriesData = [] } = useGame();

  return useMemo(() => {
    return seriesData.length > 0;
  }, [seriesData]);
};

export const useSingleGame = (gameId?: number) => {
  const { seriesMap } = useGame();

  return useMemo(() => {
    if (!gameId) {
      return undefined;
    }

    return seriesMap.get(gameId);
  }, [seriesMap, gameId]);
};

export const useMultipleSeries = (gameIds: number[]) => {
  const { seriesMap } = useGame();

  return useMemo(() => {
    if (gameIds.length === 0) {
      return DEFAULT_SERIES;
    }

    return gameIds.reduce((acc: Game[], gameId) => {
      const game = seriesMap.get(gameId);

      if (game) {
        acc.push(game);
      }

      return acc;
    }, []);
  }, [seriesMap, gameIds]);
};

interface SaveSeriesPayload extends Partial<Game> {
  id: number;
}

interface DeleteGamePayload {
  deleteFiles?: boolean;
  addImportListExclusion?: boolean;
}

interface ToggleGameMonitoredPayload {
  monitored: boolean;
}

interface ToggleSeasonMonitoredPayload {
  platformNumber: number;
  monitored: boolean;
}

interface UpdateGameMonitorPayload {
  game: {
    id: number;
    monitored?: boolean;
    platforms?: {
      platformNumber: number;
      monitored: boolean;
    }[];
  }[];
  monitoringOptions?: {
    monitor: string;
  };
}

interface BulkDeleteGamePayload {
  gameIds: number[];
  deleteFiles?: boolean;
  addImportListExclusion?: boolean;
}

interface SaveGameEditorPayload {
  gameIds: number[];
  monitored?: boolean;
  qualityProfileId?: number;
  gameType?: string;
  platformFolder?: boolean;
  rootFolderPath?: string;
  tags?: number[];
}

export const useSaveSeries = (moveFiles?: boolean) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    Game,
    SaveSeriesPayload
  >({
    path: '/game',
    queryParams: {
      moveFiles,
    },
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedSeries) => {
        queryClient.setQueryData<Game[]>(['/game'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.map((game) => {
            if (game.id === updatedSeries.id) {
              return {
                ...game,
                ...updatedSeries,
              };
            }

            return game;
          });
        });
      },
    },
  });

  return {
    saveSeries: mutate,
    isSaving: isPending,
    saveError: error,
  };
};

export const useDeleteGame = (
  gameId: number,
  options: DeleteGamePayload
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<unknown, void>({
    path: `/game/${gameId}`,
    queryParams: {
      ...options,
    },
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.setQueryData<Game[]>(['/game'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.filter((game) => game.id !== gameId);
        });
      },
    },
  });

  return {
    deleteGame: mutate,
    isDeleting: isPending,
    deleteError: error,
  };
};

export const useToggleGameMonitored = (gameId: number) => {
  const queryClient = useQueryClient();
  const game = useSingleGame(gameId);

  const { mutate, isPending, error } = useApiMutation<
    Game,
    ToggleGameMonitoredPayload
  >({
    path: '/game',
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedSeries) => {
        queryClient.setQueryData<Game[]>(['/game'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.map((game) =>
            game.id === updatedSeries.id ? updatedSeries : game
          );
        });
      },
    },
  });
  const toggleGameMonitored = useCallback(
    (payload: ToggleGameMonitoredPayload) => {
      return mutate({ ...game, ...payload });
    },
    [game, mutate]
  );

  return {
    toggleGameMonitored,
    isTogglingGameMonitored: isPending,
    toggleGameMonitoredError: error,
  };
};

export const useToggleSeasonMonitored = (gameId: number) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    Game,
    ToggleSeasonMonitoredPayload
  >({
    path: `/game/${gameId}/platform`,
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedSeries) => {
        queryClient.setQueryData<Game[]>(['/game'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.map((game) => {
            if (game.id === updatedSeries.id) {
              return {
                ...game,
                platforms: game.platforms.map((platform) => {
                  const updatedSeason = updatedSeries.platforms.find(
                    (s) => s.platformNumber === platform.platformNumber
                  );

                  if (updatedSeason) {
                    return {
                      ...platform,
                      ...updatedSeason,
                    };
                  }

                  return platform;
                }),
              };
            }

            return game;
          });
        });
      },
    },
  });

  return {
    toggleSeasonMonitored: mutate,
    isTogglingSeasonMonitored: isPending,
    toggleSeasonMonitoredError: error,
  };
};

export const useUpdateGameMonitor = (
  shouldFetchEpisodesAfterUpdate = false
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    void,
    UpdateGameMonitorPayload
  >({
    path: '/platformPass',
    method: 'POST',
    mutationOptions: {
      onSuccess: (_, variables) => {
        if (shouldFetchEpisodesAfterUpdate) {
          queryClient.invalidateQueries({ queryKey: ['/rom'] });
        }

        queryClient.setQueryData<Game[]>(['/game'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.map((game) => {
            const updatedSeries = variables.game.find(
              (s) => s.id === game.id
            );

            if (!updatedSeries) {
              return game;
            }

            return {
              ...game,
              monitored: updatedSeries.monitored ?? game.monitored,
              platforms: game.platforms.map((platform) => {
                const updatedSeason = updatedSeries.platforms?.find(
                  (s) => s.platformNumber === platform.platformNumber
                );

                if (updatedSeason) {
                  return {
                    ...platform,
                    monitored: updatedSeason.monitored,
                  };
                }

                return platform;
              }),
            };
          });
        });
      },
    },
  });

  return {
    updateGameMonitor: mutate,
    isUpdatingGameMonitor: isPending,
    updateGameMonitorError: error,
  };
};

export const useSaveGameEditor = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    Game[],
    SaveGameEditorPayload
  >({
    path: '/game/editor',
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedSeries) => {
        queryClient.setQueryData<Game[]>(['/game'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.map((game) => {
            const updatedSeriesData = updatedSeries.find(
              (updated) => updated.id === game.id
            );

            if (updatedSeriesData) {
              const {
                alternateTitles,
                images,
                rootFolderPath,
                statistics,
                ...propsToUpdate
              } = updatedSeriesData;

              return { ...game, ...propsToUpdate };
            }

            return game;
          });
        });
      },
    },
  });

  return {
    saveGameEditor: mutate,
    isSavingGameEditor: isPending,
    saveGameEditorError: error,
  };
};

export const useBulkDeleteGame = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    void,
    BulkDeleteGamePayload
  >({
    path: '/game/editor',
    method: 'DELETE',
    mutationOptions: {
      onSuccess: (_, variables) => {
        const gameIds = new Set(variables.gameIds);

        queryClient.setQueryData<Game[]>(['/game'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.filter((game) => !gameIds.has(game.id));
        });
      },
    },
  });

  return {
    bulkDeleteGame: mutate,
    isBulkDeleting: isPending,
    bulkDeleteError: error,
  };
};
