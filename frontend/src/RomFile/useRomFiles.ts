import { useQueryClient } from '@tanstack/react-query';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { getQueryKey, RomEntity } from 'Rom/useRom';
import { RomFile } from './RomFile';

const DEFAULT_ROM_FILES: RomFile[] = [];

interface SeriesRomFiles {
  gameId: number;
}

interface RomFileIds {
  romFileIds: number[];
}

export type RomFileFilter = SeriesRomFiles | RomFileIds;

const useRomFiles = (params: RomFileFilter) => {
  const result = useApiQuery<RomFile[]>({
    path: '/romFile',
    queryParams: { ...params },
    queryOptions: {
      enabled:
        ('gameId' in params && params.gameId !== undefined) ||
        ('romFileIds' in params && params.romFileIds?.length > 0),
    },
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_ROM_FILES,
    hasRomFiles: !!result.data?.length,
  };
};

export default useRomFiles;

export const useDeleteRomFile = (id: number, romEntity: RomEntity) => {
  const queryClient = useQueryClient();

  const { mutate, error, isPending } = useApiMutation<unknown, void>({
    path: `/romFile/${id}`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/romFile'] });
        queryClient.invalidateQueries({
          queryKey: [getQueryKey(romEntity)],
        });
      },
    },
  });

  return {
    deleteRomFile: mutate,
    isDeleting: isPending,
    deleteError: error,
  };
};

export const useDeleteRomFiles = () => {
  const queryClient = useQueryClient();

  const { mutate, error, isPending } = useApiMutation<unknown, RomFileIds>({
    path: '/romFile/bulk',
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/romFile'] });
        queryClient.invalidateQueries({ queryKey: ['/rom'] });
      },
    },
  });

  return {
    deleteRomFiles: mutate,
    isDeleting: isPending,
    deleteError: error,
  };
};

export const useUpdateRomFiles = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    unknown,
    Partial<RomFile>[]
  >({
    path: '/romFile/bulk',
    method: 'PUT',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/romFile'] });
      },
    },
  });

  return {
    updateRomFiles: mutate,
    isUpdating: isPending,
    updateError: error,
  };
};
