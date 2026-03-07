import React, { useEffect } from 'react';
import { useHistory, useParams } from 'react-router';
import NotFound from 'Components/NotFound';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useGame from 'Game/useGame';
import translate from 'Utilities/String/translate';
import GameDetails from './GameDetails';

function GameDetailsPage() {
  const { data: allGames } = useGame();
  const { titleSlug } = useParams<{ titleSlug: string }>();
  const history = useHistory();

  const seriesIndex = allGames.findIndex(
    (game) => game.titleSlug === titleSlug
  );

  const previousIndex = usePrevious(seriesIndex);

  useEffect(() => {
    if (
      seriesIndex === -1 &&
      previousIndex !== -1 &&
      previousIndex !== undefined
    ) {
      history.push(`${window.Playarr.urlBase}/`);
    }
  }, [seriesIndex, previousIndex, history]);

  if (seriesIndex === -1) {
    return <NotFound message={translate('SeriesCannotBeFound')} />;
  }

  return <GameDetails gameId={allGames[seriesIndex].id} />;
}

export default GameDetailsPage;
