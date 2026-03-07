import React from 'react';
import { useQueueItemForEpisode } from 'Activity/Queue/Details/QueueDetailsProvider';
import QueueDetails from 'Activity/Queue/QueueDetails';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import useRom, { RomEntity } from 'Rom/useRom';
import { useRomFile } from 'RomFile/RomFileProvider';
import { icons, kinds, sizes } from 'Helpers/Props';
import isBefore from 'Utilities/Date/isBefore';
import translate from 'Utilities/String/translate';
import RomQuality from './RomQuality';
import styles from './RomStatus.css';

interface RomStatusProps {
  romId: number;
  romEntity?: RomEntity;
  romFileId: number | undefined;
}

function RomStatus({
  romId,
  romEntity = 'roms',
  romFileId,
}: RomStatusProps) {
  const rom = useRom(romId, romEntity);
  const queueItem = useQueueItemForEpisode(romId);
  const romFile = useRomFile(romFileId);

  const { airDateUtc, grabbed, monitored } = rom || {};
  const hasRomFile = !!romFile;
  const isQueued = !!queueItem;
  const hasAired = isBefore(airDateUtc);

  if (!rom) {
    return null;
  }

  if (isQueued) {
    const { sizeLeft, size } = queueItem;

    const progress = size ? 100 - (sizeLeft / size) * 100 : 0;

    return (
      <div className={styles.center}>
        <QueueDetails
          {...queueItem}
          progressBar={
            <ProgressBar
              progress={progress}
              kind={kinds.PURPLE}
              size={sizes.MEDIUM}
            />
          }
        />
      </div>
    );
  }

  if (grabbed) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.DOWNLOADING}
          title={translate('EpisodeIsDownloading')}
        />
      </div>
    );
  }

  if (hasRomFile) {
    const quality = romFile.quality;
    const isCutoffNotMet = romFile.qualityCutoffNotMet;

    return (
      <div className={styles.center}>
        <RomQuality
          quality={quality}
          size={romFile.size}
          isCutoffNotMet={isCutoffNotMet}
          title={translate('EpisodeDownloaded')}
        />
      </div>
    );
  }

  if (!airDateUtc) {
    return (
      <div className={styles.center}>
        <Icon name={icons.TBA} title={translate('Tba')} />
      </div>
    );
  }

  if (!monitored) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.UNMONITORED}
          kind={kinds.DISABLED}
          title={translate('EpisodeIsNotMonitored')}
        />
      </div>
    );
  }

  if (hasAired) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.MISSING}
          title={translate('EpisodeMissingFromDisk')}
        />
      </div>
    );
  }

  return (
    <div className={styles.center}>
      <Icon name={icons.NOT_AIRED} title={translate('EpisodeHasNotAired')} />
    </div>
  );
}

export default RomStatus;
