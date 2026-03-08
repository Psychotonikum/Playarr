import { keepPreviousData } from '@tanstack/react-query';
import { useMemo } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { useCalendarDates } from './useCalendar';

export interface UpcomingRelease {
  igdbId: number;
  title: string;
  overview?: string;
  releaseDate?: string;
  year: number;
  network?: string;
  status: string;
  genres: string[];
  platformCount: number;
  coverUrl?: string;
  source: string;
}

function getRange(dates: string[]) {
  if (!dates.length) {
    return { start: undefined, end: undefined };
  }

  return {
    start: dates[0],
    end: dates[dates.length - 1],
  };
}

const useUpcomingReleases = () => {
  const dates = useCalendarDates();

  const { start, end } = useMemo(() => {
    return getRange(dates);
  }, [dates]);

  const result = useApiQuery<UpcomingRelease[]>({
    path: '/calendar/igdb',
    queryParams: {
      start,
      end,
    },
    queryOptions: {
      enabled: !!start && !!end,
      placeholderData: keepPreviousData,
    },
  });

  return {
    ...result,
    data: result.data ?? [],
  };
};

export default useUpcomingReleases;
