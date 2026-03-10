import React, { useCallback } from 'react';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import Game from 'Game/Game';
import { setGameOption } from 'Game/gameOptionsStore';
import useGame, { FILTER_BUILDER } from 'Game/useGame';

type GameIndexFilterModalProps = FilterModalProps<Game>;

export default function GameIndexFilterModal(props: GameIndexFilterModalProps) {
  const { data: sectionItems } = useGame();

  const dispatchSetFilter = useCallback(
    ({ selectedFilterKey }: { selectedFilterKey: string | number }) => {
      setGameOption('selectedFilterKey', selectedFilterKey);
    },
    []
  );

  return (
    <FilterModal
      {...props}
      sectionItems={sectionItems}
      filterBuilderProps={FILTER_BUILDER}
      customFilterType="game"
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
