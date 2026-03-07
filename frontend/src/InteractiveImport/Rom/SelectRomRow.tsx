import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRowButton from 'Components/Table/TableRowButton';
import Rom from 'Rom/Rom';
import { icons, kinds } from 'Helpers/Props';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import Icon from '../../Components/Icon';
import styles from './SelectRomRow.css';

function getWarningMessage(
  unverifiedSceneNumbering: boolean,
  isAnime: boolean,
  absoluteRomNumber: number | undefined
) {
  const messages = [];

  if (unverifiedSceneNumbering) {
    messages.push(translate('SceneNumberNotVerified'));
  }

  if (isAnime && !absoluteRomNumber) {
    messages.push(translate('EpisodeMissingAbsoluteNumber'));
  }

  return messages.join('\n');
}

interface SelectRomRowProps {
  id: number;
  romNumber: number;
  absoluteRomNumber: number | undefined;
  title: string;
  airDate: string;
  isAnime: boolean;
  isSelected?: boolean;
  unverifiedSceneNumbering?: boolean;
}

function SelectRomRow({
  id,
  romNumber,
  absoluteRomNumber,
  title,
  airDate,
  isAnime,
  unverifiedSceneNumbering = false,
}: SelectRomRowProps) {
  const { toggleSelected, useIsSelected } = useSelect<Rom>();
  const isSelected = useIsSelected(id);

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey = false }: SelectStateInputProps) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [toggleSelected]
  );

  const handlePress = useCallback(() => {
    handleSelectedChange({ id, value: !isSelected, shiftKey: false });
  }, [id, isSelected, handleSelectedChange]);

  const warningMessage = getWarningMessage(
    unverifiedSceneNumbering,
    isAnime,
    absoluteRomNumber
  );

  return (
    <TableRowButton onPress={handlePress}>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={handleSelectedChange}
      />

      <TableRowCell>
        {romNumber}
        {isAnime && !!absoluteRomNumber
          ? ` (${absoluteRomNumber})`
          : ''}
        {warningMessage ? (
          <Icon
            className={styles.warning}
            name={icons.WARNING}
            kind={kinds.WARNING}
            title={warningMessage}
          />
        ) : null}
      </TableRowCell>

      <TableRowCell>{title}</TableRowCell>

      <TableRowCell>{airDate}</TableRowCell>
    </TableRowButton>
  );
}

export default SelectRomRow;
