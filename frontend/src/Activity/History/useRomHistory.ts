import useApiQuery from 'Helpers/Hooks/useApiQuery';
import History from 'typings/History';

const DEFAULT_HISTORY: History[] = [];

const useRomHistory = (romId: number) => {
  const { data, ...result } = useApiQuery<History[]>({
    path: '/history/rom',
    queryParams: {
      romId,
    },
  });

  return {
    data: data ?? DEFAULT_HISTORY,
    ...result,
  };
};

export default useRomHistory;
