import { QueryKey, useQueryClient } from '@tanstack/react-query';
import { create } from 'zustand';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import { PagedQueryResponse } from 'Helpers/Hooks/usePagedApiQuery';
import { CalendarItem } from 'typings/Calendar';
import Rom from './Rom';

export type RomEntity =
  | 'calendar'
  | 'roms'
  | 'interactiveImport.roms'
  | 'wanted.cutoffUnmet'
  | 'wanted.missing';

interface EpisodeQueryKeyStore {
  calendar: QueryKey | null;
  roms: QueryKey | null;
  cutoffUnmet: QueryKey | null;
  missing: QueryKey | null;
}

const episodeQueryKeyStore = create<EpisodeQueryKeyStore>(() => ({
  calendar: null,
  roms: null,
  cutoffUnmet: null,
  missing: null,
}));

export const getQueryKey = (romEntity: RomEntity) => {
  switch (romEntity) {
    case 'calendar':
      return episodeQueryKeyStore.getState().calendar;
    case 'roms':
      return episodeQueryKeyStore.getState().roms;
    case 'wanted.cutoffUnmet':
      return episodeQueryKeyStore.getState().cutoffUnmet;
    case 'wanted.missing':
      return episodeQueryKeyStore.getState().missing;
    default:
      return null;
  }
};

export const setRomQueryKey = (
  romEntity: RomEntity,
  queryKey: QueryKey | null
) => {
  switch (romEntity) {
    case 'calendar':
      episodeQueryKeyStore.setState({ calendar: queryKey });
      break;
    case 'roms':
      episodeQueryKeyStore.setState({ roms: queryKey });
      break;
    case 'wanted.cutoffUnmet':
      episodeQueryKeyStore.setState({ cutoffUnmet: queryKey });
      break;
    case 'wanted.missing':
      episodeQueryKeyStore.setState({ missing: queryKey });
      break;
    default:
      break;
  }
};

const useRom = (romId: number | undefined, romEntity: RomEntity) => {
  const queryClient = useQueryClient();
  const queryKey = getQueryKey(romEntity);

  if (romEntity === 'calendar') {
    return queryKey
      ? queryClient
          .getQueryData<CalendarItem[]>(queryKey)
          ?.find((e) => e.id === romId)
      : undefined;
  }

  if (romEntity === 'roms') {
    return queryKey
      ? queryClient.getQueryData<Rom[]>(queryKey)?.find((e) => e.id === romId)
      : undefined;
  }

  if (romEntity === 'wanted.cutoffUnmet' || romEntity === 'wanted.missing') {
    return queryKey
      ? queryClient
          .getQueryData<PagedQueryResponse<Rom>>(queryKey)
          ?.records?.find((e) => e.id === romId)
      : undefined;
  }

  return undefined;
};

export default useRom;

interface ToggleEpisodesMonitored {
  romIds: number[];
  monitored: boolean;
}

export const useToggleEpisodesMonitored = (queryKey: QueryKey) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, variables } = useApiMutation<
    unknown,
    ToggleEpisodesMonitored
  >({
    path: '/rom/monitor',
    method: 'PUT',
    mutationOptions: {
      onSuccess: (_data, variables) => {
        queryClient.setQueryData<Rom[] | undefined>(queryKey, (oldEpisodes) => {
          if (!oldEpisodes) {
            return oldEpisodes;
          }

          return oldEpisodes.map((oldEpisode) => {
            if (variables.romIds.includes(oldEpisode.id)) {
              return {
                ...oldEpisode,
                monitored: variables.monitored,
              };
            }

            return oldEpisode;
          });
        });
      },
    },
  });

  return {
    toggleEpisodesMonitored: mutate,
    isToggling: isPending,
    togglingRomIds: variables?.romIds ?? [],
    togglingMonitored: variables?.monitored,
  };
};

const DEFAULT_EPISODES: Rom[] = [];

export const useRomsWithIds = (romIds: number[]) => {
  const queryClient = useQueryClient();
  const queryKey = getQueryKey('roms');

  return queryKey
    ? queryClient
        .getQueryData<Rom[]>(queryKey)
        ?.filter((e) => romIds.includes(e.id)) ?? DEFAULT_EPISODES
    : DEFAULT_EPISODES;
};
