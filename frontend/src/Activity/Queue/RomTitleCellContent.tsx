import React from 'react';
import Popover from 'Components/Tooltip/Popover';
import Rom from 'Rom/Rom';
import RomTitleLink from 'Rom/RomTitleLink';
import Game from 'Game/Game';
import translate from 'Utilities/String/translate';
import styles from './RomTitleCellContent.css';

interface RomTitleCellContentProps {
  roms: Rom[];
  game?: Game;
}

export default function RomTitleCellContent({
  roms,
  game,
}: RomTitleCellContentProps) {
  if (roms.length === 0 || !game) {
    return '-';
  }

  if (roms.length === 1) {
    const rom = roms[0];

    return (
      <RomTitleLink
        romId={rom.id}
        gameId={game.id}
        romTitle={rom.title}
        romEntity="roms"
        showOpenSeriesButton={true}
      />
    );
  }

  return (
    <Popover
      anchor={
        <span className={styles.multiple}>{translate('MultipleEpisodes')}</span>
      }
      title={translate('RomTitles')}
      body={
        <>
          {roms.map((rom) => {
            return (
              <div key={rom.id} className={styles.row}>
                <div className={styles.romNumber}>
                  {rom.romNumber}
                </div>

                <RomTitleLink
                  romId={rom.id}
                  gameId={game.id}
                  romTitle={rom.title}
                  romEntity="roms"
                  showOpenSeriesButton={true}
                />
              </div>
            );
          })}
        </>
      }
      position="right"
    />
  );
}
