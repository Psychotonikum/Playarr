import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

const gameTypeList = [
  {
    id: 'anime',
    get name() {
      return translate('Anime');
    },
  },
  {
    id: 'daily',
    get name() {
      return translate('Daily');
    },
  },
  {
    id: 'standard',
    get name() {
      return translate('Standard');
    },
  },
];

type GameTypeFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, string, string>,
  'tagList'
>;

function GameTypeFilterBuilderRowValue<T>(
  props: GameTypeFilterBuilderRowValueProps<T>
) {
  return <FilterBuilderRowValue tagList={gameTypeList} {...props} />;
}

export default GameTypeFilterBuilderRowValue;
