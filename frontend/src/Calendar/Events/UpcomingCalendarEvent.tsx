import React from 'react';
import { UpcomingRelease } from 'Calendar/useUpcomingReleases';
import translate from 'Utilities/String/translate';
import styles from './UpcomingCalendarEvent.css';

interface UpcomingCalendarEventProps {
  release: UpcomingRelease;
}

function UpcomingCalendarEvent({ release }: UpcomingCalendarEventProps) {
  return (
    <div className={styles.event}>
      <div className={styles.overlay}>
        <div className={styles.info}>
          <div className={styles.gameTitle}>{release.title}</div>
          <div className={styles.source}>{release.source}</div>
        </div>

        {release.platformCount > 0 ? (
          <div className={styles.platformCount}>
            {release.platformCount}{' '}
            {release.platformCount === 1
              ? translate('Platform')
              : translate('Platforms')}
          </div>
        ) : null}
      </div>
    </div>
  );
}

export default UpcomingCalendarEvent;
