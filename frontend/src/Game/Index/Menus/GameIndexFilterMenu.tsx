import React from 'react';
import FilterMenu from 'Components/Menu/FilterMenu';
import { CustomFilter, Filter } from 'Filters/Filter';
import GameIndexFilterModal from 'Game/Index/GameIndexFilterModal';

interface GameIndexFilterMenuProps {
  selectedFilterKey: string | number;
  filters: Filter[];
  customFilters: CustomFilter[];
  isDisabled: boolean;
  onFilterSelect: (filter: number | string) => void;
}

function GameIndexFilterMenu(props: GameIndexFilterMenuProps) {
  const {
    selectedFilterKey,
    filters,
    customFilters,
    isDisabled,
    onFilterSelect,
  } = props;

  return (
    <FilterMenu
      alignMenu="right"
      isDisabled={isDisabled}
      selectedFilterKey={selectedFilterKey}
      filters={filters}
      customFilters={customFilters}
      filterModalConnectorComponent={GameIndexFilterModal}
      onFilterSelect={onFilterSelect}
    />
  );
}

export default GameIndexFilterMenu;
