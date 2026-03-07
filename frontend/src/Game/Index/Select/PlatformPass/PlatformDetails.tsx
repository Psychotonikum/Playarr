import React, { useMemo } from 'react';
import { Platform } from 'Game/Game';
import translate from 'Utilities/String/translate';
import PlatformPassPlatform from './PlatformPassPlatform';
import styles from './PlatformDetails.css';

interface PlatformDetailsProps {
  gameId: number;
  platforms: Platform[];
}

function PlatformDetails(props: PlatformDetailsProps) {
  const { gameId, platforms } = props;

  const latestSeasons = useMemo(() => {
    return platforms.slice(Math.max(platforms.length - 25, 0));
  }, [platforms]);

  return (
    <div className={styles.platforms}>
      {latestSeasons.map((platform) => {
        const { platformNumber, monitored, statistics } = platform;

        return (
          <PlatformPassPlatform
            key={platformNumber}
            gameId={gameId}
            platformNumber={platformNumber}
            monitored={monitored}
            statistics={statistics}
          />
        );
      })}

      {latestSeasons.length < platforms.length ? (
        <div className={styles.truncated}>
          {translate('PlatformPassTruncated')}
        </div>
      ) : null}
    </div>
  );
}

export default PlatformDetails;
