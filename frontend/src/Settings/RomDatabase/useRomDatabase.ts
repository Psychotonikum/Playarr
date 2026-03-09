import { useQueryClient } from '@tanstack/react-query';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';

export interface RomDatabaseSystem {
  id: string;
  name: string;
  source: string;
  estimatedSize: string;
}

export interface RomDatabaseStatus {
  isDownloaded: boolean;
  entryCount: number;
  lastUpdated: string | null;
  filePath: string;
}

const DEFAULT_SYSTEMS: RomDatabaseSystem[] = [];

export const useRomDatabaseSystems = () => {
  const result = useApiQuery<RomDatabaseSystem[]>({
    path: '/romdatabase/systems',
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_SYSTEMS,
  };
};

export const useRomDatabaseStatus = (systemId: string) => {
  const result = useApiQuery<RomDatabaseStatus>({
    path: `/romdatabase/systems/${encodeURIComponent(systemId)}/status`,
    queryOptions: {
      enabled: !!systemId,
    },
  });

  return result;
};

export const useDownloadRomDatabase = (systemId: string) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation({
    path: `/romdatabase/systems/${encodeURIComponent(systemId)}/download`,
    method: 'POST',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: [`/romdatabase/systems/${systemId}/status`],
        });
      },
    },
  });

  return {
    download: mutate,
    isDownloading: isPending,
    downloadError: error,
  };
};
