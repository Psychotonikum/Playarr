import React from 'react';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import Popover from 'Components/Tooltip/Popover';
import PlatformDetails from 'Game/Index/Select/PlatformPass/PlatformDetails';
import { Platform } from 'Game/Game';
import translate from 'Utilities/String/translate';
import styles from './PlatformsCell.css';

interface GameStatusCellProps {
  className: string;
  gameId: number;
  platformCount: number;
  platforms: Platform[];
  isSelectMode: boolean;
}

function PlatformsCell(props: GameStatusCellProps) {
  const {
    className,
    gameId,
    platformCount,
    platforms,
    isSelectMode,
    ...otherProps
  } = props;

  return (
    <VirtualTableRowCell className={className} {...otherProps}>
      {isSelectMode ? (
        <Popover
          className={styles.platformCount}
          anchor={platformCount}
          title={translate('PlatformDetails')}
          body={<PlatformDetails gameId={gameId} platforms={platforms} />}
          position="left"
        />
      ) : (
        platformCount
      )}
    </VirtualTableRowCell>
  );
}

export default PlatformsCell;
