import React from 'react';
import useGame from 'Game/useGame';
import sortByProp from 'Utilities/Array/sortByProp';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type GameFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number, string>,
  'tagList'
>;

function GameFilterBuilderRowValue<T>(
  props: GameFilterBuilderRowValueProps<T>
) {
  const { data: allGames = [] } = useGame();

  const tagList = allGames
    .map((game) => ({ id: game.id, name: game.title }))
    .sort(sortByProp('name'));

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default GameFilterBuilderRowValue;
