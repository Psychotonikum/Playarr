import React from 'react';
import { GameType } from 'Game/Game';
import RomNumber, { RomNumberProps } from './RomNumber';

interface PlatformRomNumberProps extends RomNumberProps {
  airDate?: string;
  gameType?: GameType;
}

function PlatformRomNumber(props: PlatformRomNumberProps) {
  const { airDate, gameType, ...otherProps } = props;

  if (gameType === 'daily' && airDate) {
    return <span>{airDate}</span>;
  }

  return (
    <RomNumber
      gameType={gameType}
      showPlatformNumber={true}
      {...otherProps}
    />
  );
}

export default PlatformRomNumber;
