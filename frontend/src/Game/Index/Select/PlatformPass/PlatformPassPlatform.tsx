import classNames from 'classnames';
import React, { useCallback } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import { Statistics } from 'Game/Game';
import { useToggleSeasonMonitored } from 'Game/useGame';
import formatPlatform from 'Platform/formatPlatform';
import translate from 'Utilities/String/translate';
import styles from './PlatformPassPlatform.css';

interface PlatformPassPlatformProps {
  gameId: number;
  platformNumber: number;
  monitored: boolean;
  statistics: Statistics;
}

function PlatformPassPlatform(props: PlatformPassPlatformProps) {
  const {
    gameId,
    platformNumber,
    monitored,
    statistics = {
      romFileCount: 0,
      totalRomCount: 0,
      percentOfRoms: 0,
    },
  } = props;

  const { romFileCount, totalRomCount, percentOfRoms } = statistics;

  const { toggleSeasonMonitored, isTogglingSeasonMonitored } =
    useToggleSeasonMonitored(gameId);
  const onSeasonMonitoredPress = useCallback(() => {
    toggleSeasonMonitored({ platformNumber, monitored: !monitored });
  }, [platformNumber, monitored, toggleSeasonMonitored]);

  return (
    <div className={styles.platform}>
      <div className={styles.info}>
        <MonitorToggleButton
          monitored={monitored}
          isSaving={isTogglingSeasonMonitored}
          onPress={onSeasonMonitoredPress}
        />

        <span>{formatPlatform(platformNumber, true)}</span>
      </div>

      <div
        className={classNames(
          styles.roms,
          percentOfRoms === 100 && styles.allRoms
        )}
        title={translate('PlatformPassEpisodesDownloaded', {
          romFileCount,
          totalRomCount,
        })}
      >
        {totalRomCount === 0 ? '0/0' : `${romFileCount}/${totalRomCount}`}
      </div>
    </div>
  );
}

export default PlatformPassPlatform;
