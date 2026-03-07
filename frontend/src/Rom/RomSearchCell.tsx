import React, { useCallback } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { RomEntity } from 'Rom/useRom';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import RomDetailsModal from './RomDetailsModal';
import styles from './RomSearchCell.css';

interface RomSearchCellProps {
  romId: number;
  romEntity: RomEntity;
  gameId: number;
  romTitle: string;
  showOpenSeriesButton: boolean;
}

function RomSearchCell({
  romId,
  romEntity,
  gameId,
  romTitle,
  showOpenSeriesButton,
}: RomSearchCellProps) {
  const isSearching = useCommandExecuting(CommandNames.EpisodeSearch, {
    romIds: [romId],
  });

  const executeCommand = useExecuteCommand();

  const [isDetailsModalOpen, setDetailsModalOpen, setDetailsModalClosed] =
    useModalOpenState(false);

  const handleSearchPress = useCallback(() => {
    executeCommand({
      name: CommandNames.EpisodeSearch,
      romIds: [romId],
    });
  }, [romId, executeCommand]);

  return (
    <TableRowCell className={styles.episodeSearchCell}>
      <SpinnerIconButton
        name={icons.SEARCH}
        isSpinning={isSearching}
        title={translate('AutomaticSearch')}
        onPress={handleSearchPress}
      />

      <IconButton
        name={icons.INTERACTIVE}
        title={translate('InteractiveSearch')}
        onPress={setDetailsModalOpen}
      />

      <RomDetailsModal
        isOpen={isDetailsModalOpen}
        romId={romId}
        romEntity={romEntity}
        gameId={gameId}
        romTitle={romTitle}
        selectedTab="search"
        startInteractiveSearch={true}
        showOpenSeriesButton={showOpenSeriesButton}
        onModalClose={setDetailsModalClosed}
      />
    </TableRowCell>
  );
}

export default RomSearchCell;
