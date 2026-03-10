import React, {
  createContext,
  PropsWithChildren,
  useContext,
  useMemo,
} from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Queue from 'typings/Queue';

interface EpisodeDetails {
  romIds: number[];
}

interface GameDetails {
  gameId: number;
}

interface AllDetails {
  all: boolean;
}

type QueueDetailsFilter = AllDetails | EpisodeDetails | GameDetails;

const QueueDetailsContext = createContext<Queue[] | undefined>(undefined);

export default function QueueDetailsProvider({
  children,
  ...filter
}: PropsWithChildren<QueueDetailsFilter>) {
  const { data } = useApiQuery<Queue[]>({
    path: '/queue/details',
    queryParams: { ...filter },
    queryOptions: {
      enabled: Object.keys(filter).length > 0,
    },
  });

  return (
    <QueueDetailsContext.Provider value={data}>
      {children}
    </QueueDetailsContext.Provider>
  );
}

export function useQueueItemForEpisode(romId: number) {
  const queue = useContext(QueueDetailsContext);

  return useMemo(() => {
    return queue?.find((item) => item.romIds.includes(romId));
  }, [romId, queue]);
}

export function useIsDownloadingEpisodes(romIds: number[]) {
  const queue = useContext(QueueDetailsContext);

  return useMemo(() => {
    if (!queue) {
      return false;
    }

    return queue.some((item) => item.romIds?.some((e) => romIds.includes(e)));
  }, [romIds, queue]);
}

export interface SeriesQueueDetails {
  count: number;
  episodesWithFiles: number;
}

export function useQueueDetailsForSeries(
  gameId: number,
  platformNumber?: number
) {
  const queue = useContext(QueueDetailsContext);

  return useMemo<SeriesQueueDetails>(() => {
    if (!queue) {
      return { count: 0, episodesWithFiles: 0 };
    }

    return queue.reduce<SeriesQueueDetails>(
      (acc: SeriesQueueDetails, item) => {
        if (
          item.trackedDownloadState === 'imported' ||
          item.gameId !== gameId
        ) {
          return acc;
        }

        if (platformNumber != null && item.platformNumber !== platformNumber) {
          return acc;
        }

        acc.count++;

        if (item.episodeHasFile) {
          acc.episodesWithFiles++;
        }

        return acc;
      },
      {
        count: 0,
        episodesWithFiles: 0,
      }
    );
  }, [gameId, platformNumber, queue]);
}

export const useQueueDetails = () => {
  return useContext(QueueDetailsContext) ?? [];
};
