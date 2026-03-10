import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import GameTagList from 'Components/GameTagList';
import Label from 'Components/Label';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import { kinds } from 'Helpers/Props';
import { IndexerModel } from 'Settings/Indexers/useIndexers';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import styles from './ManageIndexersModalRow.css';

interface ManageIndexersModalRowProps {
  id: number;
  name: string;
  enableRss: boolean;
  enableAutomaticSearch: boolean;
  enableInteractiveSearch: boolean;
  priority: number;
  platformSearchMaximumSingleRomAge: number;
  implementation: string;
  tags: number[];
  columns: Column[];
}

function ManageIndexersModalRow(props: ManageIndexersModalRowProps) {
  const {
    id,
    name,
    enableRss,
    enableAutomaticSearch,
    enableInteractiveSearch,
    priority,
    platformSearchMaximumSingleRomAge,
    implementation,
    tags,
  } = props;

  const { toggleSelected, useIsSelected } = useSelect<IndexerModel>();
  const isSelected = useIsSelected(id);

  const onSelectedChangeWrapper = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [toggleSelected]
  );

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChangeWrapper}
      />

      <TableRowCell className={styles.name}>{name}</TableRowCell>

      <TableRowCell className={styles.implementation}>
        {implementation}
      </TableRowCell>

      <TableRowCell className={styles.enableRss}>
        <Label
          kind={enableRss ? kinds.SUCCESS : kinds.DISABLED}
          outline={!enableRss}
        >
          {enableRss ? translate('Yes') : translate('No')}
        </Label>
      </TableRowCell>

      <TableRowCell className={styles.enableAutomaticSearch}>
        <Label
          kind={enableAutomaticSearch ? kinds.SUCCESS : kinds.DISABLED}
          outline={!enableAutomaticSearch}
        >
          {enableAutomaticSearch ? translate('Yes') : translate('No')}
        </Label>
      </TableRowCell>

      <TableRowCell className={styles.enableInteractiveSearch}>
        <Label
          kind={enableInteractiveSearch ? kinds.SUCCESS : kinds.DISABLED}
          outline={!enableInteractiveSearch}
        >
          {enableInteractiveSearch ? translate('Yes') : translate('No')}
        </Label>
      </TableRowCell>

      <TableRowCell className={styles.priority}>{priority}</TableRowCell>

      <TableRowCell className={styles.platformSearchMaximumSingleRomAge}>
        {platformSearchMaximumSingleRomAge}
      </TableRowCell>

      <TableRowCell className={styles.tags}>
        <GameTagList tags={tags} />
      </TableRowCell>
    </TableRow>
  );
}

export default ManageIndexersModalRow;
