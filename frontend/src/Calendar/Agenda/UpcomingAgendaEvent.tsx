import moment from 'moment';
import React from 'react';
import { UpcomingRelease } from 'Calendar/useUpcomingReleases';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import translate from 'Utilities/String/translate';
import styles from './UpcomingAgendaEvent.css';

interface UpcomingAgendaEventProps {
  release: UpcomingRelease;
  showDate: boolean;
}

function UpcomingAgendaEvent({ release, showDate }: UpcomingAgendaEventProps) {
  const { longDateFormat } = useUiSettingsValues();

  const releaseTime = release.releaseDate
    ? moment(release.releaseDate)
    : undefined;

  return (
    <div className={styles.event}>
      <div className={styles.overlay}>
        <div className={styles.date}>
          {showDate && releaseTime ? releaseTime.format(longDateFormat) : null}
        </div>

        <div className={styles.eventWrapper}>
          <div className={styles.gameTitle}>{release.title}</div>
          <div className={styles.source}>{release.source}</div>
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
    </div>
  );
}

export default UpcomingAgendaEvent;
