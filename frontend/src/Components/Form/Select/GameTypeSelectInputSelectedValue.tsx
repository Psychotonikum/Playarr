import React from 'react';
import { IGameTypeOption } from './GameTypeSelectInput';
import HintedSelectInputSelectedValue from './HintedSelectInputSelectedValue';

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
