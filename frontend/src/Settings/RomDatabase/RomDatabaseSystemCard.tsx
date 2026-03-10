import React from 'react';
import Icon from 'Components/Icon';
import SpinnerButton from 'Components/Link/SpinnerButton';
import { icons, kinds } from 'Helpers/Props';
import {
  RomDatabaseSystem,
  useDownloadRomDatabase,
  useRomDatabaseStatus,
} from './useRomDatabase';
import styles from './RomDatabaseSystemCard.css';

interface RomDatabaseSystemCardProps {
  system: RomDatabaseSystem;
}

function RomDatabaseSystemCard({ system }: RomDatabaseSystemCardProps) {
  const { data: status, isLoading } = useRomDatabaseStatus(system.id);
  const { download, isDownloading } = useDownloadRomDatabase(system.id);

  return (
    <div className={styles.system}>
      <div className={styles.name}>{system.name}</div>
      <div className={styles.source}>{system.source}</div>
      <div className={styles.size}>{system.estimatedSize}</div>

      <div className={styles.status}>
        {isLoading ? (
          <span>Loading...</span>
        ) : status?.isDownloaded ? (
          <span className={styles.downloaded}>
            <Icon name={icons.CHECK} /> Downloaded
            <div className={styles.entryCount}>{status.entryCount} entries</div>
          </span>
        ) : (
          <span className={styles.notDownloaded}>Not downloaded</span>
        )}
      </div>

      <div className={styles.actions}>
        <SpinnerButton
          kind={status?.isDownloaded ? kinds.DEFAULT : kinds.PRIMARY}
          isSpinning={isDownloading}
          onPress={() => download(undefined)}
        >
          {status?.isDownloaded ? 'Re-download' : 'Download'}
        </SpinnerButton>
      </div>
    </div>
  );
}

export default RomDatabaseSystemCard;
