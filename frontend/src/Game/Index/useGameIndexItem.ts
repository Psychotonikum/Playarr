import { maxBy } from 'lodash';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting } from 'Commands/useCommands';
import { Platform } from 'Game/Game';
import { useSingleGame } from 'Game/useGame';
import useGameQualityProfile from 'Game/useGameQualityProfile';

export function useGameIndexItem(gameId: number) {
  const game = useSingleGame(gameId);
  const qualityProfile = useGameQualityProfile(game);

  const isRefreshingSeries = useCommandExecuting(CommandNames.RefreshSeries, {
    gameIds: [gameId],
  });

  const isSearchingSeries = useCommandExecuting(CommandNames.SeriesSearch, {
    gameId,
  });

  const latestPlatform: Platform | undefined = maxBy(
    game?.platforms || [],
    (platform) => platform.platformNumber
  );

  return {
    game,
    qualityProfile,
    latestPlatform,
    isRefreshingSeries,
    isSearchingSeries,
  };
}

export default useGameIndexItem;
