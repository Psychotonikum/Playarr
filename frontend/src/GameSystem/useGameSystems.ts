import { useQueryClient } from '@tanstack/react-query';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import GameSystem from './GameSystem';

const DEFAULT_GAME_SYSTEMS: GameSystem[] = [];

const useGameSystems = () => {
  const result = useApiQuery<GameSystem[]>({
    path: '/gamesystem',
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_GAME_SYSTEMS,
  };
};

export default useGameSystems;

export const useGameSystem = (id: number) => {
  const result = useApiQuery<GameSystem>({
    path: `/gamesystem/${id}`,
  });

  return {
    ...result,
    data: result.data,
  };
};

export const useAddGameSystem = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error, data } = useApiMutation<
    GameSystem,
    GameSystem
  >({
    path: '/gamesystem',
    method: 'POST',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/gamesystem'] });
      },
    },
  });

  return {
    addGameSystem: mutate,
    isAdding: isPending,
    addError: error,
    newGameSystem: data,
  };
};

export const useUpdateGameSystem = (id: number) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<GameSystem, GameSystem>({
    path: `/gamesystem/${id}`,
    method: 'PUT',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/gamesystem'] });
      },
    },
  });

  return {
    updateGameSystem: mutate,
    isUpdating: isPending,
    updateError: error,
  };
};

export const useDeleteGameSystem = (id: number) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<unknown, void>({
    path: `/gamesystem/${id}`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/gamesystem'] });
      },
    },
  });

  return {
    deleteGameSystem: mutate,
    isDeleting: isPending,
    deleteError: error,
  };
};
