import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import useGame from 'Game/useGame';

function useQualityProfileInUse(id: number | undefined) {
  const { data: game = [] } = useGame();
  const importLists = useSelector(
    (state: AppState) => state.settings.importLists.items
  );

  return useMemo(() => {
    if (!id) {
      return false;
    }

    return (
      game.some((s) => s.qualityProfileId === id) ||
      importLists.some((list) => list.qualityProfileId === id)
    );
  }, [id, game, importLists]);
}

export default useQualityProfileInUse;
