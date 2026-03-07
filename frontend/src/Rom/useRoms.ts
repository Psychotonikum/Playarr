import { useEffect, useMemo } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import clientSideFilterAndSort from 'Utilities/Filter/clientSideFilterAndSort';
import Rom from './Rom';
import { useRomOptions } from './romOptionsStore';
import { setRomQueryKey } from './useRom';

const DEFAULT_EPISODES: Rom[] = [];

interface SeriesEpisodes {
  gameId: number;
}

interface SeasonEpisodes {
  gameId: number | undefined;
  platformNumber: number | undefined;
  isSelection: boolean;
}

interface RomIds {
  romIds: number[];
}

interface RomFileId {
  romFileId: number;
}

export type EpisodeFilter =
  | SeriesEpisodes
  | SeasonEpisodes
  | RomIds
  | RomFileId;

const useRoms = (params: EpisodeFilter) => {
  const setQueryKey = !('isSelection' in params);

  const { isPlaceholderData, queryKey, ...result } = useApiQuery<Rom[]>({
    path: '/rom',
    queryParams:
      'isSelection' in params
        ? {
            gameId: params.gameId,
            platformNumber: params.platformNumber,
          }
        : { ...params },
    queryOptions: {
      enabled:
        ('gameId' in params && params.gameId !== undefined) ||
        ('romIds' in params && params.romIds?.length > 0) ||
        ('romFileId' in params && params.romFileId !== undefined),
    },
  });

  useEffect(() => {
    if (setQueryKey && !isPlaceholderData) {
      setRomQueryKey('roms', queryKey);
    }
  }, [setQueryKey, isPlaceholderData, queryKey]);

  return {
    ...result,
    queryKey,
    data: result.data ?? DEFAULT_EPISODES,
  };
};

export default useRoms;

export const useSeasonEpisodes = (gameId: number, platformNumber: number) => {
  const { data, ...result } = useRoms({ gameId });
  const { sortKey, sortDirection } = useRomOptions();

  const seasonEpisodes = useMemo(() => {
    const { data: seasonEpisodes } = clientSideFilterAndSort(
      data.filter((rom) => rom.platformNumber === platformNumber),
      {
        sortKey,
        sortDirection,
      }
    );

    return seasonEpisodes;
  }, [data, platformNumber, sortKey, sortDirection]);

  return {
    ...result,
    data: seasonEpisodes,
  };
};
