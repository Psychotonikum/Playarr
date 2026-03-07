import { useMemo } from 'react';
import useGame from 'Game/useGame';

function useExistingGame(igdbId: number | undefined) {
  const { data: game = [] } = useGame();

  return useMemo(() => {
    if (igdbId == null) {
      return false;
    }

    return game.some((s) => s.igdbId === igdbId);
  }, [igdbId, game]);
}

export default useExistingGame;
