import React, { useCallback, useMemo, useState } from 'react';
import HistoryDetails from 'Activity/History/Details/HistoryDetails';
import HistoryEventTypeCell from 'Activity/History/HistoryEventTypeCell';
import { useMarkAsFailed } from 'Activity/History/useHistory';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import { useSingleGame } from 'Game/useGame';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import PlatformRomNumber from 'Rom/PlatformRomNumber';
import RomFormats from 'Rom/RomFormats';
import RomLanguages from 'Rom/RomLanguages';
import RomNumber from 'Rom/RomNumber';
import RomQuality from 'Rom/RomQuality';
import useRom from 'Rom/useRom';
import CustomFormat from 'typings/CustomFormat';
import { HistoryData, HistoryEventType } from 'typings/History';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import styles from './GameHistoryRow.css';

interface GameHistoryRowProps {
  id: number;
  gameId: number;
  romId: number;
  eventType: HistoryEventType;
  sourceTitle: string;
  languages?: Language[];
  quality: QualityModel;
  qualityCutoffNotMet: boolean;
  customFormats?: CustomFormat[];
  date: string;
  data: HistoryData;
  downloadId?: string;
  fullSeries: boolean;
  customFormatScore: number;
}

function GameHistoryRow({
  id,
  gameId,
  romId,
  eventType,
  sourceTitle,
  languages = [],
  quality,
  qualityCutoffNotMet,
  customFormats = [],
  date,
  data,
  downloadId,
  fullSeries,
  customFormatScore,
}: GameHistoryRowProps) {
  const game = useSingleGame(gameId);
  const rom = useRom(romId, 'roms');
  const { markAsFailed } = useMarkAsFailed(id, 'game');
  const [isMarkAsFailedModalOpen, setIsMarkAsFailedModalOpen] = useState(false);

  const EpisodeComponent = fullSeries ? PlatformRomNumber : RomNumber;

  const title = useMemo(() => {
    switch (eventType) {
      case 'grabbed':
        return 'Grabbed';
      case 'seriesFolderImported':
        return 'Game Folder Imported';
      case 'downloadFolderImported':
        return 'Download Folder Imported';
      case 'downloadFailed':
        return 'Download Failed';
      case 'romFileDeleted':
        return 'Rom File Deleted';
      case 'romFileRenamed':
        return 'Rom File Renamed';
      default:
        return 'Unknown';
    }
  }, [eventType]);

  const handleMarkAsFailedPress = useCallback(() => {
    setIsMarkAsFailedModalOpen(true);
  }, []);

  const handleConfirmMarkAsFailed = useCallback(() => {
    markAsFailed();
  }, [markAsFailed]);

  const handleMarkAsFailedModalClose = useCallback(() => {
    setIsMarkAsFailedModalOpen(false);
  }, []);

  if (!game || !rom) {
    return null;
  }

  return (
    <TableRow>
      <HistoryEventTypeCell eventType={eventType} data={data} />

      <TableRowCell>
        <EpisodeComponent
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

      <TableRowCell className={styles.sourceTitle}>{sourceTitle}</TableRowCell>

      <TableRowCell>
        <RomLanguages languages={languages} />
      </TableRowCell>

      <TableRowCell>
        <RomQuality quality={quality} isCutoffNotMet={qualityCutoffNotMet} />
      </TableRowCell>

      <TableRowCell>
        <RomFormats formats={customFormats} />
      </TableRowCell>

      <TableRowCell>
        {formatCustomFormatScore(customFormatScore, customFormats.length)}
      </TableRowCell>

      <RelativeDateCell date={date} includeSeconds={true} includeTime={true} />

      <TableRowCell className={styles.actions}>
        <Popover
          anchor={<Icon name={icons.INFO} />}
          title={title}
          body={
            <HistoryDetails
              eventType={eventType}
              sourceTitle={sourceTitle}
              data={data}
              downloadId={downloadId}
            />
          }
          position={tooltipPositions.LEFT}
        />

        {eventType === 'grabbed' ? (
          <IconButton
            title={translate('MarkAsFailed')}
            name={icons.REMOVE}
            size={14}
            onPress={handleMarkAsFailedPress}
          />
        ) : null}
      </TableRowCell>

      <ConfirmModal
        isOpen={isMarkAsFailedModalOpen}
        kind={kinds.DANGER}
        title={translate('MarkAsFailed')}
        message={translate('MarkAsFailedConfirmation', { sourceTitle })}
        confirmLabel={translate('MarkAsFailed')}
        onConfirm={handleConfirmMarkAsFailed}
        onCancel={handleMarkAsFailedModalClose}
      />
    </TableRow>
  );
}

export default GameHistoryRow;
