import useApiQuery from 'Helpers/Hooks/useApiQuery';
import History from 'typings/History';

const DEFAULT_HISTORY: History[] = [];

const useGameHistory = (
  gameId: number,
  platformNumber: number | undefined
) => {
  const { data, ...result } = useApiQuery<History[]>({
    path: '/history/game',
    queryParams: {
      gameId,
      platformNumber,
    },
  });

  return {
    data: data ?? DEFAULT_HISTORY,
    ...result,
  };
};

export default useGameHistory;
