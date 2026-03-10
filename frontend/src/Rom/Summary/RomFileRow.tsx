import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import RomFormats from 'Rom/RomFormats';
import RomLanguages from 'Rom/RomLanguages';
import RomQuality from 'Rom/RomQuality';
import { RomFile } from 'RomFile/RomFile';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import MediaInfo from './MediaInfo';
import styles from './RomFileRow.css';

interface RomFileRowProps
  extends Pick<
    RomFile,
    | 'path'
    | 'size'
    | 'languages'
    | 'quality'
    | 'customFormats'
    | 'customFormatScore'
    | 'qualityCutoffNotMet'
    | 'mediaInfo'
  > {
  columns: Column[];
  onDeleteRomFile(): void;
}

function RomFileRow(props: RomFileRowProps) {
  const {
    path,
    size,
    languages,
    quality,
    customFormats,
    customFormatScore,
    qualityCutoffNotMet,
    mediaInfo,
    columns,
    onDeleteRomFile,
  } = props;

  const [
    isRemoveRomFileModalOpen,
    setRemoveRomFileModalOpen,
    setRemoveRomFileModalClosed,
  ] = useModalOpenState(false);

  const handleRemoveRomFilePress = useCallback(() => {
    onDeleteRomFile();

    setRemoveRomFileModalClosed();
  }, [onDeleteRomFile, setRemoveRomFileModalClosed]);

  return (
    <TableRow>
      {columns.map(({ name, isVisible }) => {
        if (!isVisible) {
          return null;
        }

        if (name === 'path') {
          return <TableRowCell key={name}>{path}</TableRowCell>;
        }

        if (name === 'size') {
          return <TableRowCell key={name}>{formatBytes(size)}</TableRowCell>;
        }

        if (name === 'languages') {
          return (
            <TableRowCell key={name} className={styles.languages}>
              <RomLanguages languages={languages} />
            </TableRowCell>
          );
        }

        if (name === 'quality') {
          return (
            <TableRowCell key={name} className={styles.quality}>
              <RomQuality
                quality={quality}
                isCutoffNotMet={qualityCutoffNotMet}
              />
            </TableRowCell>
          );
        }

        if (name === 'customFormats') {
          return (
            <TableRowCell key={name} className={styles.customFormats}>
              <RomFormats formats={customFormats} />
            </TableRowCell>
          );
        }

        if (name === 'customFormatScore') {
          return (
            <TableRowCell key={name} className={styles.customFormatScore}>
              {formatCustomFormatScore(customFormatScore, customFormats.length)}
            </TableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <TableRowCell key={name} className={styles.actions}>
              {mediaInfo ? (
                <Popover
                  anchor={<Icon name={icons.MEDIA_INFO} />}
                  title={translate('MediaInfo')}
                  body={<MediaInfo {...mediaInfo} />}
                  position={tooltipPositions.LEFT}
                />
              ) : null}

              <IconButton
                title={translate('DeleteEpisodeFromDisk')}
                name={icons.REMOVE}
                onPress={setRemoveRomFileModalOpen}
              />
            </TableRowCell>
          );
        }

        return null;
      })}

      <ConfirmModal
        isOpen={isRemoveRomFileModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteRomFile')}
        message={translate('DeleteRomFileMessage', { path })}
        confirmLabel={translate('Delete')}
        onConfirm={handleRemoveRomFilePress}
        onCancel={setRemoveRomFileModalClosed}
      />
    </TableRow>
  );
}

export default RomFileRow;
