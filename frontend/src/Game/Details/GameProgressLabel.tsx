import React from 'react';
import { useQueueDetailsForSeries } from 'Activity/Queue/Details/QueueDetailsProvider';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';

function getRomCountKind(
  monitored: boolean,
  romFileCount: number,
  romCount: number,
  isDownloading: boolean
) {
  if (isDownloading) {
    return kinds.PURPLE;
  }

  if (romFileCount === romCount && romCount > 0) {
    return kinds.SUCCESS;
  }

  if (!monitored) {
    return kinds.WARNING;
  }

  return kinds.DANGER;
}

interface GameProgressLabelProps {
  className: string;
  gameId: number;
  monitored: boolean;
  romCount: number;
  romFileCount: number;
}

function GameProgressLabel({
  className,
  gameId,
  monitored,
  romCount,
  romFileCount,
}: GameProgressLabelProps) {
  const queueDetails = useQueueDetailsForSeries(gameId);

  const newDownloads = queueDetails.count - queueDetails.episodesWithFiles;
  const text = newDownloads
    ? `${romFileCount} + ${newDownloads} / ${romCount}`
    : `${romFileCount} / ${romCount}`;

  return (
    <Label
      className={className}
      kind={getRomCountKind(
        monitored,
        romFileCount,
        romCount,
        queueDetails.count > 0
      )}
      size={sizes.LARGE}
    >
      <span>{text}</span>
    </Label>
  );
}

export default GameProgressLabel;
