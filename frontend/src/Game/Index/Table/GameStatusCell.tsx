import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import { GameStatus } from 'Game/Game';
import { getGameStatusDetails } from 'Game/GameStatus';
import { useToggleGameMonitored } from 'Game/useGame';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './GameStatusCell.css';

interface GameStatusCellProps {
  className: string;
  gameId: number;
  monitored: boolean;
  status: GameStatus;
  isSelectMode: boolean;
  component?: React.ElementType;
}

function GameStatusCell({
  className,
  gameId,
  monitored,
  status,
  isSelectMode,
  component: Component = VirtualTableRowCell,
  ...otherProps
}: GameStatusCellProps) {
  const statusDetails = getGameStatusDetails(status);
  const { toggleGameMonitored, isTogglingGameMonitored } =
    useToggleGameMonitored(gameId);

  const onMonitoredPress = useCallback(() => {
    toggleGameMonitored({ monitored: !monitored });
  }, [monitored, toggleGameMonitored]);

  return (
    <Component className={className} {...otherProps}>
      {isSelectMode ? (
        <MonitorToggleButton
          className={styles.statusIcon}
          monitored={monitored}
          isSaving={isTogglingGameMonitored}
          onPress={onMonitoredPress}
        />
      ) : (
        <Icon
          className={styles.statusIcon}
          name={monitored ? icons.MONITORED : icons.UNMONITORED}
          title={
            monitored
              ? translate('SeriesIsMonitored')
              : translate('SeriesIsUnmonitored')
          }
        />
      )}

      <Icon
        className={styles.statusIcon}
        name={statusDetails.icon}
        title={`${statusDetails.title}: ${statusDetails.message}`}
      />
    </Component>
  );
}

export default GameStatusCell;
