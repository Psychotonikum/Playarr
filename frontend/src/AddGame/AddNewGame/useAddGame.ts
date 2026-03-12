import { useQueryClient } from '@tanstack/react-query';
import AddGame from 'AddGame/AddGame';
import Game from 'Game/Game';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';

interface AddGamePayload extends AddGame {
  rootFolderPath: string;
  qualityProfileId: number;
  platformFolder: boolean;
  addOptions: {
    monitor: import('Game/Game').GameMonitor;
    searchForMissingRoms: boolean;
  };
  tags: number[];
}

const DEFAULT_SERIES: AddGame[] = [];

export const useLookupSeries = (query: string, isEnabled = true) => {
  const result = useApiQuery<AddGame[]>({
    path: '/game/lookup',
    queryParams: {
      term: query,
    },
    queryOptions: {
      enabled: isEnabled && !!query,
      // Disable refetch on window focus to prevent refetching when the user switch tabs
      refetchOnWindowFocus: false,
    },
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_SERIES,
  };
};

export const useAddGame = () => {
  const queryClient = useQueryClient();

  const { isPending, error, mutate } = useApiMutation<Game, AddGamePayload>({
    path: '/game',
    method: 'POST',
    mutationOptions: {
      onSuccess: (newSeries) => {
        queryClient.setQueryData<Game[]>(['/game'], (oldSeries) => {
          if (!oldSeries) {
            return [newSeries];
          }

          return [...oldSeries, newSeries];
        });
      },
    },
  });

  return {
    isAdding: isPending,
    addError: error,
    addGame: mutate,
  };
};
