import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowCellButton from 'Components/Table/Cells/TableRowCellButton';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import Game from 'Game/Game';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import SelectGameModal from 'InteractiveImport/Game/SelectGameModal';
import SelectIndexerFlagsModal from 'InteractiveImport/IndexerFlags/SelectIndexerFlagsModal';
import InteractiveImport from 'InteractiveImport/InteractiveImport';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectPlatformModal from 'InteractiveImport/Platform/SelectPlatformModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectReleaseGroupModal from 'InteractiveImport/ReleaseGroup/SelectReleaseGroupModal';
import ReleaseType from 'InteractiveImport/ReleaseType';
import SelectReleaseTypeModal from 'InteractiveImport/ReleaseType/SelectReleaseTypeModal';
import SelectRomModal from 'InteractiveImport/Rom/SelectRomModal';
import { SelectedEpisode } from 'InteractiveImport/Rom/SelectRomModalContent';
import { useUpdateInteractiveImportItem } from 'InteractiveImport/useInteractiveImport';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import getReleaseTypeName from 'Rom/getReleaseTypeName';
import IndexerFlags from 'Rom/IndexerFlags';
import Rom from 'Rom/Rom';
import RomFormats from 'Rom/RomFormats';
import RomLanguages from 'Rom/RomLanguages';
import RomQuality from 'Rom/RomQuality';
import CustomFormat from 'typings/CustomFormat';
import { SelectStateInputProps } from 'typings/props';
import Rejection from 'typings/Rejection';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import InteractiveImportRowCellPlaceholder from './InteractiveImportRowCellPlaceholder';
import styles from './InteractiveImportRow.css';

type SelectType =
  | 'game'
  | 'platform'
  | 'rom'
  | 'releaseGroup'
  | 'quality'
  | 'language'
  | 'indexerFlags'
  | 'releaseType';

type SelectedChangeProps = SelectStateInputProps & {
  hasRomFileId: boolean;
};

interface InteractiveImportRowProps {
  id: number;
  allowSeriesChange: boolean;
  relativePath: string;
  game?: Game;
  platformNumber?: number;
  roms?: Rom[];
  releaseGroup?: string;
  quality?: QualityModel;
  languages?: Language[];
  size: number;
  releaseType: ReleaseType;
  customFormats?: CustomFormat[];
  customFormatScore?: number;
  indexerFlags: number;
  rejections: Rejection[];
  columns: Column[];
  romFileId?: number;
  isReprocessing?: boolean;
  modalTitle: string;
  onReprocessItems: (ids: number[]) => void;
  onSelectedChange(result: SelectedChangeProps): void;
  onValidRowChange(id: number, isValid: boolean): void;
}

function InteractiveImportRow(props: InteractiveImportRowProps) {
  const {
    id,
    allowSeriesChange,
    relativePath,
    game,
    platformNumber,
    roms = [],
    quality,
    languages,
    releaseGroup,
    size,
    releaseType,
    customFormats = [],
    customFormatScore,
    indexerFlags,
    rejections,
    isReprocessing,
    modalTitle,
    romFileId,
    columns,
    onReprocessItems,
    onSelectedChange,
    onValidRowChange,
  } = props;

  const { useIsSelected } = useSelect<InteractiveImport>();
  const isSelected = useIsSelected(id);
  const { updateInteractiveImportItem } = useUpdateInteractiveImportItem();

  const isSeriesColumnVisible = useMemo(
    () => columns.find((c) => c.name === 'game')?.isVisible ?? false,
    [columns]
  );
  const isIndexerFlagsColumnVisible = useMemo(
    () => columns.find((c) => c.name === 'indexerFlags')?.isVisible ?? false,
    [columns]
  );

  const [selectModalOpen, setSelectModalOpen] = useState<SelectType | null>(
    null
  );

  useEffect(
    () => {
      if (
        allowSeriesChange &&
        game &&
        platformNumber != null &&
        roms.length &&
        quality &&
        languages &&
        size > 0
      ) {
        onSelectedChange({
          id,
          hasRomFileId: !!romFileId,
          value: true,
          shiftKey: false,
        });
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  useEffect(() => {
    const isValid = !!(
      game &&
      platformNumber != null &&
      roms.length &&
      quality &&
      languages
    );

    if (isSelected && !isValid) {
      onValidRowChange(id, false);
    } else {
      onValidRowChange(id, true);
    }
  }, [
    id,
    game,
    platformNumber,
    roms,
    quality,
    languages,
    isSelected,
    onValidRowChange,
  ]);

  const handleSelectedChange = useCallback(
    (result: SelectStateInputProps) => {
      onSelectedChange({
        ...result,
        hasRomFileId: !!romFileId,
      });
    },
    [romFileId, onSelectedChange]
  );

  const selectRowAfterChange = useCallback(() => {
    if (!isSelected) {
      onSelectedChange({
        id,
        hasRomFileId: !!romFileId,
        value: true,
        shiftKey: false,
      });
    }
  }, [id, romFileId, isSelected, onSelectedChange]);

  const onSelectModalClose = useCallback(() => {
    setSelectModalOpen(null);
  }, [setSelectModalOpen]);

  const onSelectSeriesPress = useCallback(() => {
    setSelectModalOpen('game');
  }, [setSelectModalOpen]);

  const onSeriesSelect = useCallback(
    (game: Game) => {
      updateInteractiveImportItem(id, {
        game,
        platformNumber: undefined,
        roms: [],
      });

      onReprocessItems([id]);
      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectSeasonPress = useCallback(() => {
    setSelectModalOpen('platform');
  }, [setSelectModalOpen]);

  const onSeasonSelect = useCallback(
    (platformNumber: number) => {
      updateInteractiveImportItem(id, {
        platformNumber,
        roms: [],
      });

      onReprocessItems([id]);
      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectEpisodePress = useCallback(() => {
    setSelectModalOpen('rom');
  }, [setSelectModalOpen]);

  const onEpisodesSelect = useCallback(
    (selectedEpisodes: SelectedEpisode[]) => {
      const roms = selectedEpisodes[0].roms;
      updateInteractiveImportItem(id, { roms });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectReleaseGroupPress = useCallback(() => {
    setSelectModalOpen('releaseGroup');
  }, [setSelectModalOpen]);

  const onReleaseGroupSelect = useCallback(
    (releaseGroup: string) => {
      updateInteractiveImportItem(id, { releaseGroup });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectQualityPress = useCallback(() => {
    setSelectModalOpen('quality');
  }, [setSelectModalOpen]);

  const onQualitySelect = useCallback(
    (quality: QualityModel) => {
      updateInteractiveImportItem(id, { quality });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectLanguagePress = useCallback(() => {
    setSelectModalOpen('language');
  }, [setSelectModalOpen]);

  const onLanguagesSelect = useCallback(
    (languages: Language[]) => {
      updateInteractiveImportItem(id, { languages });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectReleaseTypePress = useCallback(() => {
    setSelectModalOpen('releaseType');
  }, [setSelectModalOpen]);

  const onReleaseTypeSelect = useCallback(
    (releaseType: ReleaseType) => {
      updateInteractiveImportItem(id, { releaseType });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const onSelectIndexerFlagsPress = useCallback(() => {
    setSelectModalOpen('indexerFlags');
  }, [setSelectModalOpen]);

  const onIndexerFlagsSelect = useCallback(
    (indexerFlags: number) => {
      updateInteractiveImportItem(id, { indexerFlags });
      onReprocessItems([id]);

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [
      id,
      updateInteractiveImportItem,
      onReprocessItems,
      setSelectModalOpen,
      selectRowAfterChange,
    ]
  );

  const gameTitle = game ? game.title : '';
  const isAnime = game?.gameType === 'anime';

  const romInfo = roms.map((rom) => {
    return (
      <div key={rom.id}>
        {rom.romNumber}

        {isAnime && rom.absoluteRomNumber != null
          ? ` (${rom.absoluteRomNumber})`
          : ''}

        {` - ${rom.title}`}
      </div>
    );
  });

  const requiresPlatformNumber = isNaN(Number(platformNumber));
  const showSeriesPlaceholder = isSelected && !game;
  const showPlatformNumberPlaceholder =
    isSelected && !!game && requiresPlatformNumber && !isReprocessing;
  const showRomNumbersPlaceholder =
    isSelected && Number.isInteger(platformNumber) && !roms.length;
  const showReleaseGroupPlaceholder = isSelected && !releaseGroup;
  const showQualityPlaceholder = isSelected && !quality;
  const showLanguagePlaceholder = isSelected && !languages;
  const showIndexerFlagsPlaceholder = isSelected && !indexerFlags;

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={handleSelectedChange}
      />

      <TableRowCell className={styles.relativePath} title={relativePath}>
        {relativePath}
      </TableRowCell>

      {isSeriesColumnVisible ? (
        <TableRowCellButton
          isDisabled={!allowSeriesChange}
          title={
            allowSeriesChange ? translate('ClickToChangeSeries') : undefined
          }
          onPress={onSelectSeriesPress}
        >
          {showSeriesPlaceholder ? (
            <InteractiveImportRowCellPlaceholder />
          ) : (
            gameTitle
          )}
        </TableRowCellButton>
      ) : null}

      <TableRowCellButton
        isDisabled={!game}
        title={game ? translate('ClickToChangeSeason') : undefined}
        onPress={onSelectSeasonPress}
      >
        {showPlatformNumberPlaceholder ? (
          <InteractiveImportRowCellPlaceholder />
        ) : (
          platformNumber
        )}

        {isReprocessing && platformNumber == null ? (
          <LoadingIndicator className={styles.reprocessing} size={20} />
        ) : null}
      </TableRowCellButton>

      <TableRowCellButton
        isDisabled={!game || requiresPlatformNumber}
        title={
          game && !requiresPlatformNumber
            ? translate('ClickToChangeEpisode')
            : undefined
        }
        onPress={onSelectEpisodePress}
      >
        {showRomNumbersPlaceholder ? (
          <InteractiveImportRowCellPlaceholder />
        ) : (
          romInfo
        )}
      </TableRowCellButton>

      <TableRowCellButton
        title={translate('ClickToChangeReleaseGroup')}
        onPress={onSelectReleaseGroupPress}
      >
        {showReleaseGroupPlaceholder ? (
          <InteractiveImportRowCellPlaceholder isOptional={true} />
        ) : (
          releaseGroup
        )}
      </TableRowCellButton>

      <TableRowCellButton
        className={styles.quality}
        title={translate('ClickToChangeQuality')}
        onPress={onSelectQualityPress}
      >
        {showQualityPlaceholder && <InteractiveImportRowCellPlaceholder />}

        {!showQualityPlaceholder && !!quality && (
          <RomQuality className={styles.label} quality={quality} />
        )}
      </TableRowCellButton>

      <TableRowCellButton
        className={styles.languages}
        title={translate('ClickToChangeLanguage')}
        onPress={onSelectLanguagePress}
      >
        {showLanguagePlaceholder && <InteractiveImportRowCellPlaceholder />}

        {!showLanguagePlaceholder && !!languages && (
          <RomLanguages className={styles.label} languages={languages} />
        )}
      </TableRowCellButton>

      <TableRowCell>{formatBytes(size)}</TableRowCell>

      <TableRowCellButton
        title={translate('ClickToChangeReleaseType')}
        onPress={onSelectReleaseTypePress}
      >
        {getReleaseTypeName(releaseType)}
      </TableRowCellButton>

      <TableRowCell>
        {customFormats?.length ? (
          <Popover
            anchor={formatCustomFormatScore(
              customFormatScore,
              customFormats.length
            )}
            title={translate('CustomFormats')}
            body={
              <div className={styles.customFormatTooltip}>
                <RomFormats formats={customFormats} />
              </div>
            }
            position={tooltipPositions.LEFT}
          />
        ) : null}
      </TableRowCell>

      {isIndexerFlagsColumnVisible ? (
        <TableRowCellButton
          title={translate('ClickToChangeIndexerFlags')}
          onPress={onSelectIndexerFlagsPress}
        >
          {showIndexerFlagsPlaceholder ? (
            <InteractiveImportRowCellPlaceholder isOptional={true} />
          ) : (
            <>
              {indexerFlags ? (
                <Popover
                  anchor={<Icon name={icons.FLAG} />}
                  title={translate('IndexerFlags')}
                  body={<IndexerFlags indexerFlags={indexerFlags} />}
                  position={tooltipPositions.LEFT}
                />
              ) : null}
            </>
          )}
        </TableRowCellButton>
      ) : null}

      <TableRowCell>
        {rejections.length ? (
          <Popover
            anchor={<Icon name={icons.DANGER} kind={kinds.DANGER} />}
            title={translate('ReleaseRejected')}
            body={
              <ul>
                {rejections.map((rejection, index) => {
                  return <li key={index}>{rejection.message}</li>;
                })}
              </ul>
            }
            position={tooltipPositions.LEFT}
            canFlip={false}
          />
        ) : null}
      </TableRowCell>

      <SelectGameModal
        isOpen={selectModalOpen === 'game'}
        modalTitle={modalTitle}
        onSeriesSelect={onSeriesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectPlatformModal
        isOpen={selectModalOpen === 'platform'}
        gameId={game?.id}
        modalTitle={modalTitle}
        onSeasonSelect={onSeasonSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectRomModal
        isOpen={selectModalOpen === 'rom'}
        selectedIds={[id]}
        gameId={game?.id}
        isAnime={isAnime}
        platformNumber={platformNumber}
        selectedDetails={relativePath}
        modalTitle={modalTitle}
        onEpisodesSelect={onEpisodesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectReleaseGroupModal
        isOpen={selectModalOpen === 'releaseGroup'}
        releaseGroup={releaseGroup ?? ''}
        modalTitle={modalTitle}
        onReleaseGroupSelect={onReleaseGroupSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectQualityModal
        isOpen={selectModalOpen === 'quality'}
        qualityId={quality ? quality.quality.id : 0}
        proper={quality ? quality.revision.version > 1 : false}
        real={quality ? quality.revision.real > 0 : false}
        modalTitle={modalTitle}
        onQualitySelect={onQualitySelect}
        onModalClose={onSelectModalClose}
      />

      <SelectLanguageModal
        isOpen={selectModalOpen === 'language'}
        languageIds={languages ? languages.map((l) => l.id) : []}
        modalTitle={modalTitle}
        onLanguagesSelect={onLanguagesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectReleaseTypeModal
        isOpen={selectModalOpen === 'releaseType'}
        releaseType={releaseType ?? 'unknown'}
        modalTitle={modalTitle}
        onReleaseTypeSelect={onReleaseTypeSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectIndexerFlagsModal
        isOpen={selectModalOpen === 'indexerFlags'}
        indexerFlags={indexerFlags ?? 0}
        modalTitle={modalTitle}
        onIndexerFlagsSelect={onIndexerFlagsSelect}
        onModalClose={onSelectModalClose}
      />
    </TableRow>
  );
}

export default InteractiveImportRow;
