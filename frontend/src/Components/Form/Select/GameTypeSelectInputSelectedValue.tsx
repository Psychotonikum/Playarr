import React from 'react';
import HintedSelectInputSelectedValue from './HintedSelectInputSelectedValue';
import { IGameTypeOption } from './GameTypeSelectInput';

interface GameTypeSelectInputOptionProps {
  selectedValue: string;
  values: IGameTypeOption[];
  format: string;
}
function GameTypeSelectInputSelectedValue(
  props: GameTypeSelectInputOptionProps
) {
  const { selectedValue, values, ...otherProps } = props;
  const format = values.find((v) => v.key === selectedValue)?.format;

  return (
    <HintedSelectInputSelectedValue
      {...otherProps}
      selectedValue={selectedValue}
      values={values}
      hint={format}
    />
  );
}

export default GameTypeSelectInputSelectedValue;
