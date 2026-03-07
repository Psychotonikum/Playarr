import React from 'react';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons, kinds } from 'Helpers/Props';
import {
  RomFileDeletedHistory,
  GrabbedHistoryData,
  HistoryData,
  HistoryEventType,
} from 'typings/History';
import translate from 'Utilities/String/translate';
import styles from './HistoryEventTypeCell.css';

function getIconName(eventType: HistoryEventType, data: HistoryData) {
  switch (eventType) {
    case 'grabbed':
      return icons.DOWNLOADING;
    case 'seriesFolderImported':
      return icons.DRIVE;
    case 'downloadFolderImported':
      return icons.DOWNLOADED;
    case 'downloadFailed':
      return icons.DOWNLOADING;
    case 'romFileDeleted':
      return (data as RomFileDeletedHistory).reason === 'MissingFromDisk'
        ? icons.FILE_MISSING
        : icons.DELETE;
    case 'romFileRenamed':
      return icons.ORGANIZE;
    case 'downloadIgnored':
      return icons.IGNORE;
    default:
      return icons.UNKNOWN;
  }
}

function getIconKind(eventType: HistoryEventType) {
  switch (eventType) {
    case 'downloadFailed':
      return kinds.DANGER;
    default:
      return kinds.DEFAULT;
  }
}

function getTooltip(eventType: HistoryEventType, data: HistoryData) {
  switch (eventType) {
    case 'grabbed':
      return translate('EpisodeGrabbedTooltip', {
        indexer: (data as GrabbedHistoryData).indexer,
        downloadClient: (data as GrabbedHistoryData).downloadClient,
      });
    case 'seriesFolderImported':
      return translate('GameFolderImportedTooltip');
    case 'downloadFolderImported':
      return translate('EpisodeImportedTooltip');
    case 'downloadFailed':
      return translate('DownloadFailedEpisodeTooltip');
    case 'romFileDeleted':
      return (data as RomFileDeletedHistory).reason === 'MissingFromDisk'
        ? translate('RomFileMissingTooltip')
        : translate('RomFileDeletedTooltip');
    case 'romFileRenamed':
      return translate('RomFileRenamedTooltip');
    case 'downloadIgnored':
      return translate('DownloadIgnoredEpisodeTooltip');
    default:
      return translate('UnknownEventTooltip');
  }
}

interface HistoryEventTypeCellProps {
  eventType: HistoryEventType;
  data: HistoryData;
}

function HistoryEventTypeCell({ eventType, data }: HistoryEventTypeCellProps) {
  const iconName = getIconName(eventType, data);
  const iconKind = getIconKind(eventType);
  const tooltip = getTooltip(eventType, data);

  return (
    <TableRowCell className={styles.cell} title={tooltip}>
      <Icon name={iconName} kind={iconKind} />
    </TableRowCell>
  );
}

export default HistoryEventTypeCell;
