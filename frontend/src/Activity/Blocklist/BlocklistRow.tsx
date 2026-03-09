import React, { useCallback, useState } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import IconButton from 'Components/Link/IconButton';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import RomLanguages from 'Rom/RomLanguages';
import { icons, kinds } from 'Helpers/Props';
import GameTitleLink from 'Game/GameTitleLink';
import { useSingleGame } from 'Game/useGame';
import Blocklist from 'typings/Blocklist';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import BlocklistDetailsModal from './BlocklistDetailsModal';
import { useRemoveBlocklistItem } from './useBlocklist';
import styles from './BlocklistRow.css';

interface BlocklistRowProps extends Blocklist {
  columns: Column[];
}

function BlocklistRow({
  id,
  gameId,
  sourceTitle,
  languages,
  date,
  protocol,
  indexer,
  message,
  source,
  columns,
}: BlocklistRowProps) {
  const game = useSingleGame(gameId);
  const { isRemoving, removeBlocklistItem } = useRemoveBlocklistItem(id);
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const { toggleSelected, useIsSelected } = useSelect<Blocklist>();
  const isSelected = useIsSelected(id);

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey = false }: SelectStateInputProps) => {
      toggleSelected({ id, isSelected: value, shiftKey });
    },
    [toggleSelected]
  );

  const handleDetailsPress = useCallback(() => {
    setIsDetailsModalOpen(true);
  }, [setIsDetailsModalOpen]);

  const handleDetailsModalClose = useCallback(() => {
    setIsDetailsModalOpen(false);
  }, [setIsDetailsModalOpen]);

  const handleRemovePress = useCallback(() => {
    removeBlocklistItem();
  }, [removeBlocklistItem]);

  if (!game) {
    return null;
  }

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={handleSelectedChange}
      />

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'game.sortTitle') {
          return (
            <TableRowCell key={name}>
              <GameTitleLink
                titleSlug={game.titleSlug}
                title={game.title}
              />
            </TableRowCell>
          );
        }

        if (name === 'sourceTitle') {
          return <TableRowCell key={name}>{sourceTitle}</TableRowCell>;
        }

        if (name === 'languages') {
          return (
            <TableRowCell key={name} className={styles.languages}>
              <RomLanguages languages={languages} />
            </TableRowCell>
          );
        }



        if (name === 'date') {
          // eslint-disable-next-line @typescript-eslint/ban-ts-comment
          // @ts-ignore ts(2739)
          return <RelativeDateCell key={name} date={date} />;
        }

        if (name === 'indexer') {
          return (
            <TableRowCell key={name} className={styles.indexer}>
              {indexer}
            </TableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <TableRowCell key={name} className={styles.actions}>
              <IconButton name={icons.INFO} onPress={handleDetailsPress} />

              <IconButton
                title={translate('RemoveFromBlocklist')}
                name={icons.REMOVE}
                kind={kinds.DANGER}
                isSpinning={isRemoving}
                onPress={handleRemovePress}
              />
            </TableRowCell>
          );
        }

        return null;
      })}

      <BlocklistDetailsModal
        isOpen={isDetailsModalOpen}
        sourceTitle={sourceTitle}
        protocol={protocol}
        indexer={indexer}
        message={message}
        source={source}
        onModalClose={handleDetailsModalClose}
      />
    </TableRow>
  );
}

export default BlocklistRow;
