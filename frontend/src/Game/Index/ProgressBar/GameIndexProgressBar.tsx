import React from 'react';
import { useQueueDetailsForSeries } from 'Activity/Queue/Details/QueueDetailsProvider';
import ProgressBar from 'Components/ProgressBar';
import { GameStatus } from 'Game/Game';
import { sizes } from 'Helpers/Props';
import getProgressBarKind from 'Utilities/Game/getProgressBarKind';
import translate from 'Utilities/String/translate';
import styles from './GameIndexProgressBar.css';

interface GameIndexProgressBarProps {
  gameId: number;
  platformNumber?: number;
  monitored: boolean;
  status: GameStatus;
  romCount: number;
  romFileCount: number;
  totalRomCount: number;
  width: number;
  detailedProgressBar: boolean;
  isStandalone: boolean;
}

function GameIndexProgressBar(props: GameIndexProgressBarProps) {
  const {
    gameId,
    platformNumber,
    monitored,
    status,
    romCount,
    romFileCount,
    totalRomCount,
    width,
    detailedProgressBar,
    isStandalone,
  } = props;

  const queueDetails = useQueueDetailsForSeries(gameId, platformNumber);

  const newDownloads = queueDetails.count - queueDetails.episodesWithFiles;
  const progress = romCount ? (romFileCount / romCount) * 100 : 100;
  const text = newDownloads
    ? `${romFileCount} + ${newDownloads} / ${romCount}`
    : `${romFileCount} / ${romCount}`;

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={isStandalone ? undefined : styles.progress}
      progress={progress}
      kind={getProgressBarKind(
        status,
        monitored,
        progress,
        queueDetails.count > 0
      )}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      text={text}
      title={translate('SeriesProgressBarText', {
        romFileCount,
        romCount,
        totalRomCount,
        downloadingCount: queueDetails.count,
      })}
      width={width}
    />
  );
}

export default GameIndexProgressBar;
