import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import Rom from 'Rom/Rom';
import RomSearchCell from 'Rom/RomSearchCell';
import RomStatus from 'Rom/RomStatus';
import RomTitleLink from 'Rom/RomTitleLink';
import PlatformRomNumber from 'Rom/PlatformRomNumber';
import RomFileLanguages from 'RomFile/RomFileLanguages';
import GameTitleLink from 'Game/GameTitleLink';
import { useSingleGame } from 'Game/useGame';
import { SelectStateInputProps } from 'typings/props';
import styles from './CutoffUnmetRow.css';

interface CutoffUnmetRowProps {
  id: number;
  gameId: number;
  romFileId?: number;
  platformNumber: number;
  romNumber: number;
  absoluteRomNumber?: number;
  scenePlatformNumber?: number;
  sceneRomNumber?: number;
  sceneAbsoluteRomNumber?: number;
  unverifiedSceneNumbering: boolean;
  airDateUtc?: string;
  lastSearchTime?: string;
  title: string;
  columns: Column[];
}

function CutoffUnmetRow({
  id,
  gameId,
  romFileId,
  platformNumber,
  romNumber,
  absoluteRomNumber,
  scenePlatformNumber,
  sceneRomNumber,
  sceneAbsoluteRomNumber,
  unverifiedSceneNumbering,
  airDateUtc,
  lastSearchTime,
  title,
  columns,
}: CutoffUnmetRowProps) {
  const game = useSingleGame(gameId);
  const { toggleSelected, useIsSelected } = useSelect<Rom>();
  const isSelected = useIsSelected(id);

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey = false }: SelectStateInputProps) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [toggleSelected]
  );

  if (!game || !romFileId) {
    return null;
  }

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={handleSelectedChange}
      />

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'game.sortTitle') {
          return (
            <TableRowCell key={name}>
              <GameTitleLink
                titleSlug={game.titleSlug}
                title={game.title}
              />
            </TableRowCell>
          );
        }

        if (name === 'rom') {
          return (
            <TableRowCell key={name} className={styles.rom}>
              <PlatformRomNumber
                platformNumber={platformNumber}
                romNumber={romNumber}
                absoluteRomNumber={absoluteRomNumber}
                gameType={game.gameType}
                alternateTitles={game.alternateTitles}
                scenePlatformNumber={scenePlatformNumber}
                sceneRomNumber={sceneRomNumber}
                sceneAbsoluteRomNumber={sceneAbsoluteRomNumber}
                unverifiedSceneNumbering={unverifiedSceneNumbering}
              />
            </TableRowCell>
          );
        }

        if (name === 'roms.title') {
          return (
            <TableRowCell key={name}>
              <RomTitleLink
                romId={id}
                gameId={game.id}
                romEntity="wanted.cutoffUnmet"
                romTitle={title}
                showOpenSeriesButton={true}
              />
            </TableRowCell>
          );
        }

        if (name === 'roms.airDateUtc') {
          return <RelativeDateCell key={name} date={airDateUtc} />;
        }

        if (name === 'roms.lastSearchTime') {
          return (
            <RelativeDateCell
              key={name}
              date={lastSearchTime}
              includeSeconds={true}
            />
          );
        }

        if (name === 'languages') {
          return (
            <TableRowCell key={name} className={styles.languages}>
              <RomFileLanguages romFileId={romFileId} />
            </TableRowCell>
          );
        }

        if (name === 'status') {
          return (
            <TableRowCell key={name} className={styles.status}>
              <RomStatus
                romId={id}
                romFileId={romFileId}
                romEntity="wanted.cutoffUnmet"
              />
            </TableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <RomSearchCell
              key={name}
              romId={id}
              gameId={game.id}
              romTitle={title}
              romEntity="wanted.cutoffUnmet"
              showOpenSeriesButton={true}
            />
          );
        }

        return null;
      })}
    </TableRow>
  );
}

export default CutoffUnmetRow;
