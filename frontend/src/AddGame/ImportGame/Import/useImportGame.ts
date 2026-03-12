import { useQueryClient } from '@tanstack/react-query';
import { useCallback } from 'react';
import Game from 'Game/Game';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import {
  getImportGameItems,
  removeImportGameItemByPath,
} from './importGameStore';

export const useImportGame = () => {
  const queryClient = useQueryClient();

  const { isPending, error, mutate } = useApiMutation<Game[], Game[]>({
    path: '/game/import',
    method: 'POST',
    mutationOptions: {
      onSuccess: (data, newSeries) => {
        queryClient.invalidateQueries({ queryKey: ['/rootFolder'] });
        queryClient.setQueryData<Game[]>(['/game'], (oldSeries) => {
          if (!oldSeries) {
            return data;
          }

          return [...oldSeries, ...data];
        });

        newSeries.forEach((game) => {
          removeImportGameItemByPath(game.path);
        });
      },
    },
  });

  const importSeries = useCallback(
    (ids: string[]) => {
      const items = getImportGameItems(ids);
      const addedIds: string[] = [];

      const allNewSeries = ids.reduce<Game[]>((acc, id) => {
        const item = items.find((i) => i.id === id);
        const selectedSeries = item?.selectedSeries;

        // Make sure we have a selected game and the same game hasn't been added yet.
        if (
          selectedSeries &&
          !acc.some((a) => a.igdbId === selectedSeries.igdbId)
        ) {
          const newSeries: Game = {
            ...selectedSeries,
            monitored: true,
            monitorNewItems: 'all',
            platformFolder: true,
            path: item.path,
            addOptions: {
              monitor: item.monitor,
              searchForMissingRoms: false,
            },
            tags: [],
          };

          newSeries.path = item.path;

          addedIds.push(id);
          acc.push(newSeries);
        }

        return acc;
      }, []);

      if (allNewSeries.length > 0) {
        mutate(allNewSeries);
      }
    },
    [mutate]
  );

  return {
    isImporting: isPending,
    importError: error,
    importSeries,
  };
};
