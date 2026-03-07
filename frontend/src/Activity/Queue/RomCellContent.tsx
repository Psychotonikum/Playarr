import React from 'react';
import Rom from 'Rom/Rom';
import PlatformRomNumber from 'Rom/PlatformRomNumber';
import Game from 'Game/Game';
import translate from 'Utilities/String/translate';

interface RomCellContentProps {
  roms: Rom[];
  isFullSeason: boolean;
  platformNumber?: number;
  game?: Game;
}

export default function RomCellContent({
  roms,
  isFullSeason,
  platformNumber,
  game,
}: RomCellContentProps) {
  if (roms.length === 0) {
    return '-';
  }

  if (isFullSeason && platformNumber != null) {
    return translate('PlatformNumberToken', { platformNumber });
  }

  if (roms.length === 1) {
    const rom = roms[0];

    return (
      <PlatformRomNumber
        platformNumber={rom.platformNumber}
        romNumber={rom.romNumber}
        absoluteRomNumber={rom.absoluteRomNumber}
        gameType={game?.gameType}
        alternateTitles={game?.alternateTitles}
        scenePlatformNumber={rom.scenePlatformNumber}
        sceneRomNumber={rom.sceneRomNumber}
        sceneAbsoluteRomNumber={rom.sceneAbsoluteRomNumber}
        unverifiedSceneNumbering={rom.unverifiedSceneNumbering}
      />
    );
  }

  const firstEpisode = roms[0];
  const lastEpisode = roms[roms.length - 1];

  return (
    <>
      <PlatformRomNumber
        platformNumber={firstEpisode.platformNumber}
        romNumber={firstEpisode.romNumber}
        absoluteRomNumber={firstEpisode.absoluteRomNumber}
        gameType={game?.gameType}
        alternateTitles={game?.alternateTitles}
        scenePlatformNumber={firstEpisode.scenePlatformNumber}
        sceneRomNumber={firstEpisode.sceneRomNumber}
        sceneAbsoluteRomNumber={firstEpisode.sceneAbsoluteRomNumber}
        unverifiedSceneNumbering={firstEpisode.unverifiedSceneNumbering}
      />
      {' - '}
      <PlatformRomNumber
        platformNumber={lastEpisode.platformNumber}
        romNumber={lastEpisode.romNumber}
        absoluteRomNumber={lastEpisode.absoluteRomNumber}
        gameType={game?.gameType}
        alternateTitles={game?.alternateTitles}
        scenePlatformNumber={lastEpisode.scenePlatformNumber}
        sceneRomNumber={lastEpisode.sceneRomNumber}
        sceneAbsoluteRomNumber={lastEpisode.sceneAbsoluteRomNumber}
        unverifiedSceneNumbering={lastEpisode.unverifiedSceneNumbering}
      />
    </>
  );
}
