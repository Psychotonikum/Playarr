import React, { useCallback, useState } from 'react';
import IconButton from 'Components/Link/IconButton';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import Tooltip from 'Components/Tooltip/Tooltip';
import romEntities from 'Rom/romEntities';
import RomFormats from 'Rom/RomFormats';
import RomLanguages from 'Rom/RomLanguages';
import RomQuality from 'Rom/RomQuality';
import RomTitleLink from 'Rom/RomTitleLink';
import PlatformRomNumber from 'Rom/PlatformRomNumber';
import useRom from 'Rom/useRom';
import { icons, tooltipPositions } from 'Helpers/Props';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import GameTitleLink from 'Game/GameTitleLink';
import { useSingleGame } from 'Game/useGame';
import CustomFormat from 'typings/CustomFormat';
import { HistoryData, HistoryEventType } from 'typings/History';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import HistoryDetailsModal from './Details/HistoryDetailsModal';
import HistoryEventTypeCell from './HistoryEventTypeCell';
import styles from './HistoryRow.css';

interface HistoryRowProps {
  id: number;
  romId: number;
  gameId: number;
  languages: Language[];
  quality: QualityModel;
  customFormats?: CustomFormat[];
  customFormatScore: number;
  qualityCutoffNotMet: boolean;
  eventType: HistoryEventType;
  sourceTitle: string;
  date: string;
  data: HistoryData;
  downloadId?: string;
  isMarkingAsFailed?: boolean;
  markAsFailedError?: object;
  columns: Column[];
}

function HistoryRow(props: HistoryRowProps) {
  const {
    id,
    romId,
    gameId,
    languages,
    quality,
    customFormats = [],
    customFormatScore,
    qualityCutoffNotMet,
    eventType,
    sourceTitle,
    date,
    data,
    downloadId,
    columns,
  } = props;

  const game = useSingleGame(gameId);
  const rom = useRom(romId, 'roms');

  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);

  const handleDetailsPress = useCallback(() => {
    setIsDetailsModalOpen(true);
  }, [setIsDetailsModalOpen]);

  const handleDetailsModalClose = useCallback(() => {
    setIsDetailsModalOpen(false);
  }, [setIsDetailsModalOpen]);

  if (!game || !rom) {
    return null;
  }

  return (
    <TableRow>
      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'eventType') {
          return (
            <HistoryEventTypeCell
              key={name}
              eventType={eventType}
              data={data}
            />
          );
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
            <TableRowCell key={name}>
              <PlatformRomNumber
                platformNumber={rom.platformNumber}
                romNumber={rom.romNumber}
                absoluteRomNumber={rom.absoluteRomNumber}
                gameType={game.gameType}
                alternateTitles={game.alternateTitles}
                scenePlatformNumber={rom.scenePlatformNumber}
                sceneRomNumber={rom.sceneRomNumber}
                sceneAbsoluteRomNumber={rom.sceneAbsoluteRomNumber}
              />
            </TableRowCell>
          );
        }

        if (name === 'roms.title') {
          return (
            <TableRowCell key={name}>
              <RomTitleLink
                romId={romId}
                romEntity={romEntities.ROMS}
                gameId={game.id}
                romTitle={rom.title}
                showOpenSeriesButton={true}
              />
            </TableRowCell>
          );
        }

        if (name === 'languages') {
          return (
            <TableRowCell key={name}>
              <RomLanguages languages={languages} />
            </TableRowCell>
          );
        }

        if (name === 'quality') {
          return (
            <TableRowCell key={name}>
              <RomQuality
                quality={quality}
                isCutoffNotMet={qualityCutoffNotMet}
              />
            </TableRowCell>
          );
        }

        if (name === 'customFormats') {
          return (
            <TableRowCell key={name}>
              <RomFormats formats={customFormats} />
            </TableRowCell>
          );
        }

        if (name === 'date') {
          return <RelativeDateCell key={name} date={date} />;
        }

        if (name === 'downloadClient') {
          const downloadClientName =
            'downloadClientName' in data ? data.downloadClientName : null;
          const downloadClient =
            'downloadClient' in data ? data.downloadClient : null;

          return (
            <TableRowCell key={name} className={styles.downloadClient}>
              {downloadClientName ?? downloadClient ?? ''}
            </TableRowCell>
          );
        }

        if (name === 'indexer') {
          return (
            <TableRowCell key={name} className={styles.indexer}>
              {'indexer' in data ? data.indexer : ''}
            </TableRowCell>
          );
        }

        if (name === 'customFormatScore') {
          return (
            <TableRowCell key={name} className={styles.customFormatScore}>
              <Tooltip
                anchor={formatCustomFormatScore(
                  customFormatScore,
                  customFormats.length
                )}
                tooltip={<RomFormats formats={customFormats} />}
                position={tooltipPositions.BOTTOM}
              />
            </TableRowCell>
          );
        }

        if (name === 'releaseGroup') {
          return (
            <TableRowCell key={name} className={styles.releaseGroup}>
              {'releaseGroup' in data ? data.releaseGroup : ''}
            </TableRowCell>
          );
        }

        if (name === 'sourceTitle') {
          return <TableRowCell key={name}>{sourceTitle}</TableRowCell>;
        }

        if (name === 'details') {
          return (
            <TableRowCell key={name} className={styles.details}>
              <IconButton name={icons.INFO} onPress={handleDetailsPress} />
            </TableRowCell>
          );
        }

        return null;
      })}

      <HistoryDetailsModal
        id={id}
        isOpen={isDetailsModalOpen}
        eventType={eventType}
        sourceTitle={sourceTitle}
        data={data}
        downloadId={downloadId}
        onModalClose={handleDetailsModalClose}
      />
    </TableRow>
  );
}

export default HistoryRow;
