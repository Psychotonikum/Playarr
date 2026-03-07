import React, { useMemo } from 'react';
import * as gameTypes from 'Utilities/Game/gameTypes';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';
import GameTypeSelectInputOption from './GameTypeSelectInputOption';
import GameTypeSelectInputSelectedValue from './GameTypeSelectInputSelectedValue';

export interface GameTypeSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<EnhancedSelectInputValue<string>, string>,
    'values'
  > {
  includeNoChange?: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed?: boolean;
}

export interface IGameTypeOption {
  key: string;
  value: string;
  format?: string;
  isDisabled?: boolean;
}

const gameTypeOptions: IGameTypeOption[] = [
  {
    key: gameTypes.STANDARD,
    value: 'Standard',
    get format() {
      return translate('StandardEpisodeTypeFormat', { format: 'S01E05' });
    },
  },
  {
    key: gameTypes.DAILY,
    value: 'Daily / Date',
    get format() {
      return translate('DailyEpisodeTypeFormat', { format: '2020-05-25' });
    },
  },
  {
    key: gameTypes.ANIME,
    value: 'Anime / Absolute',
    get format() {
      return translate('AnimeEpisodeTypeFormat', { format: '005' });
    },
  },
];

function GameTypeSelectInput(props: GameTypeSelectInputProps) {
  const {
    includeNoChange = false,
    includeNoChangeDisabled = true,
    includeMixed = false,
  } = props;

  const values = useMemo(() => {
    const result = [...gameTypeOptions];

    if (includeNoChange) {
      result.unshift({
        key: 'noChange',
        value: translate('NoChange'),
        isDisabled: includeNoChangeDisabled,
      });
    }

    if (includeMixed) {
      result.unshift({
        key: 'mixed',
        value: `(${translate('Mixed')})`,
        isDisabled: true,
      });
    }

    return result;
  }, [includeNoChange, includeNoChangeDisabled, includeMixed]);

  return (
    <EnhancedSelectInput
      {...props}
      values={values}
      optionComponent={GameTypeSelectInputOption}
      selectedValueComponent={GameTypeSelectInputSelectedValue}
    />
  );
}

export default GameTypeSelectInput;
