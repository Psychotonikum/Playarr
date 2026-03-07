import React from 'react';
import GameMonitoringOptionsPopoverContent from 'AddGame/GameMonitoringOptionsPopoverContent';
import GameTypePopoverContent from 'AddGame/GameTypePopoverContent';
import Icon from 'Components/Icon';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import VirtualTableSelectAllHeaderCell from 'Components/Table/VirtualTableSelectAllHeaderCell';
import Popover from 'Components/Tooltip/Popover';
import { icons, tooltipPositions } from 'Helpers/Props';
import { CheckInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './ImportGameHeader.css';

interface ImportGameHeaderProps {
  allSelected: boolean;
  allUnselected: boolean;
  onSelectAllChange: (change: CheckInputChanged) => void;
}

function ImportGameHeader({
  allSelected,
  allUnselected,
  onSelectAllChange,
}: ImportGameHeaderProps) {
  return (
    <VirtualTableHeader>
      <VirtualTableSelectAllHeaderCell
        allSelected={allSelected}
        allUnselected={allUnselected}
        onSelectAllChange={onSelectAllChange}
      />

      <VirtualTableHeaderCell className={styles.folder} name="folder">
        {translate('Folder')}
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell className={styles.monitor} name="monitor">
        {translate('Monitor')}

        <Popover
          anchor={<Icon className={styles.detailsIcon} name={icons.INFO} />}
          title={translate('MonitoringOptions')}
          body={<GameMonitoringOptionsPopoverContent />}
          position={tooltipPositions.RIGHT}
        />
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.qualityProfile}
        name="qualityProfileId"
      >
        {translate('QualityProfile')}
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell className={styles.gameType} name="gameType">
        {translate('GameType')}

        <Popover
          anchor={<Icon className={styles.detailsIcon} name={icons.INFO} />}
          title={translate('GameType')}
          body={<GameTypePopoverContent />}
          position={tooltipPositions.RIGHT}
        />
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.platformFolder}
        name="platformFolder"
      >
        {translate('PlatformFolder')}
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell className={styles.game} name="game">
        {translate('Game')}
      </VirtualTableHeaderCell>
    </VirtualTableHeader>
  );
}

export default ImportGameHeader;
