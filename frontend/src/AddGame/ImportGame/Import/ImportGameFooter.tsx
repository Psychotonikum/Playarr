import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  AddGameOptions,
  setAddGameOption,
  useAddGameOptions,
} from 'AddGame/addGameOptionsStore';
import { useSelect } from 'App/Select/SelectContext';
import CheckInput from 'Components/Form/CheckInput';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContentFooter from 'Components/Page/PageContentFooter';
import Popover from 'Components/Tooltip/Popover';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import { GameMonitor, GameType } from 'Game/Game';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import {
  ImportGameItem,
  startProcessing,
  stopProcessing,
  updateImportGameItem,
  useImportGameItems,
  useLookupQueueHasItems,
} from './importGameStore';
import { useImportGame } from './useImportGame';
import styles from './ImportGameFooter.css';

type MixedType = 'mixed';

function ImportGameFooter() {
  const {
    monitor: defaultMonitor,
    qualityProfileId: defaultQualityProfileId,
    gameType: defaultGameType,
    platformFolder: defaultPlatformFolder,
  } = useAddGameOptions();

  const items = useImportGameItems();
  const isLookingUpSeries = useLookupQueueHasItems();

  const [monitor, setMonitor] = useState<GameMonitor | MixedType>(
    defaultMonitor
  );
  const [qualityProfileId, setQualityProfileId] = useState<number | MixedType>(
    defaultQualityProfileId
  );
  const [gameType, setGameType] = useState<GameType | MixedType>(
    defaultGameType
  );
  const [platformFolder, setPlatformFolder] = useState<boolean | MixedType>(
    defaultPlatformFolder
  );

  const { selectedCount, getSelectedIds } = useSelect<ImportGameItem>();

  const { importSeries, isImporting, importError } = useImportGame();

  const {
    hasUnsearchedItems,
    isMonitorMixed,
    isQualityProfileIdMixed,
    isGameTypeMixed,
    isPlatformFolderMixed,
  } = useMemo(() => {
    let isMonitorMixed = false;
    let isQualityProfileIdMixed = false;
    let isGameTypeMixed = false;
    let isPlatformFolderMixed = false;
    let hasUnsearchedItems = false;

    items.forEach((item) => {
      if (item.monitor !== defaultMonitor) {
        isMonitorMixed = true;
      }

      if (item.qualityProfileId !== defaultQualityProfileId) {
        isQualityProfileIdMixed = true;
      }

      if (item.gameType !== defaultGameType) {
        isGameTypeMixed = true;
      }

      if (item.platformFolder !== defaultPlatformFolder) {
        isPlatformFolderMixed = true;
      }

      if (!item.hasSearched) {
        hasUnsearchedItems = true;
      }
    });

    return {
      hasUnsearchedItems: !isLookingUpSeries && hasUnsearchedItems,
      isMonitorMixed,
      isQualityProfileIdMixed,
      isGameTypeMixed,
      isPlatformFolderMixed,
    };
  }, [
    defaultMonitor,
    defaultQualityProfileId,
    defaultPlatformFolder,
    defaultGameType,
    items,
    isLookingUpSeries,
  ]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged<string | number | boolean | number[]>) => {
      if (name === 'monitor') {
        setMonitor(value as GameMonitor);
      } else if (name === 'qualityProfileId') {
        setQualityProfileId(value as number);
      } else if (name === 'gameType') {
        setGameType(value as GameType);
      } else if (name === 'platformFolder') {
        setPlatformFolder(value as boolean);
      }

      setAddGameOption(name as keyof AddGameOptions, value);

      getSelectedIds().forEach((id) => {
        updateImportGameItem({
          id,
          [name]: value,
        });
      });
    },
    [getSelectedIds]
  );

  const handleLookupPress = useCallback(() => {
    startProcessing();
  }, []);

  const handleCancelLookupPress = useCallback(() => {
    stopProcessing();
  }, []);

  const handleImportPress = useCallback(() => {
    importSeries(getSelectedIds());
  }, [importSeries, getSelectedIds]);

  useEffect(() => {
    if (isMonitorMixed && monitor !== 'mixed') {
      setMonitor('mixed');
    } else if (!isMonitorMixed && monitor !== defaultMonitor) {
      setMonitor(defaultMonitor);
    }
  }, [defaultMonitor, isMonitorMixed, monitor]);

  useEffect(() => {
    if (isQualityProfileIdMixed && qualityProfileId !== 'mixed') {
      setQualityProfileId('mixed');
    } else if (
      !isQualityProfileIdMixed &&
      qualityProfileId !== defaultQualityProfileId
    ) {
      setQualityProfileId(defaultQualityProfileId);
    }
  }, [defaultQualityProfileId, isQualityProfileIdMixed, qualityProfileId]);

  useEffect(() => {
    if (isGameTypeMixed && gameType !== 'mixed') {
      setGameType('mixed');
    } else if (!isGameTypeMixed && gameType !== defaultGameType) {
      setGameType(defaultGameType);
    }
  }, [defaultGameType, isGameTypeMixed, gameType]);

  useEffect(() => {
    if (isPlatformFolderMixed && platformFolder !== 'mixed') {
      setPlatformFolder('mixed');
    } else if (!isPlatformFolderMixed && platformFolder !== defaultPlatformFolder) {
      setPlatformFolder(defaultPlatformFolder);
    }
  }, [defaultPlatformFolder, isPlatformFolderMixed, platformFolder]);

  return (
    <PageContentFooter>
      <div className={styles.inputContainer}>
        <div className={styles.label}>{translate('Monitor')}</div>

        <FormInputGroup
          type={inputTypes.MONITOR_EPISODES_SELECT}
          name="monitor"
          value={monitor}
          isDisabled={!selectedCount}
          includeMixed={isMonitorMixed}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <div className={styles.label}>{translate('QualityProfile')}</div>

        <FormInputGroup
          type={inputTypes.QUALITY_PROFILE_SELECT}
          name="qualityProfileId"
          value={qualityProfileId}
          isDisabled={!selectedCount}
          includeMixed={isQualityProfileIdMixed}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <div className={styles.label}>{translate('GameType')}</div>

        <FormInputGroup
          type={inputTypes.SERIES_TYPE_SELECT}
          name="gameType"
          value={gameType}
          isDisabled={!selectedCount}
          includeMixed={isGameTypeMixed}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <div className={styles.label}>{translate('PlatformFolder')}</div>

        <CheckInput
          name="platformFolder"
          value={platformFolder}
          isDisabled={!selectedCount}
          onChange={handleInputChange}
        />
      </div>

      <div>
        <div className={styles.label}>&nbsp;</div>

        <div className={styles.importButtonContainer}>
          <SpinnerButton
            className={styles.importButton}
            kind={kinds.PRIMARY}
            isSpinning={isImporting}
            isDisabled={!selectedCount || isLookingUpSeries}
            onPress={handleImportPress}
          >
            {translate('ImportCountSeries', { selectedCount })}
          </SpinnerButton>

          {isLookingUpSeries ? (
            <Button
              className={styles.loadingButton}
              kind={kinds.WARNING}
              onPress={handleCancelLookupPress}
            >
              {translate('CancelProcessing')}
            </Button>
          ) : null}

          {hasUnsearchedItems ? (
            <Button
              className={styles.loadingButton}
              kind={kinds.SUCCESS}
              onPress={handleLookupPress}
            >
              {translate('StartProcessing')}
            </Button>
          ) : null}

          {isLookingUpSeries ? (
            <LoadingIndicator className={styles.loading} size={24} />
          ) : null}

          {isLookingUpSeries ? translate('ProcessingFolders') : null}

          {importError ? (
            <Popover
              anchor={
                <Icon
                  className={styles.importError}
                  name={icons.WARNING}
                  kind={kinds.WARNING}
                />
              }
              title={translate('ImportErrors')}
              body={
                <ul>
                  {Array.isArray(importError.statusBody) ? (
                    importError.statusBody.map((error, index) => {
                      return <li key={index}>{error.errorMessage}</li>;
                    })
                  ) : (
                    <li>{JSON.stringify(importError.statusBody)}</li>
                  )}
                </ul>
              }
              position={tooltipPositions.RIGHT}
            />
          ) : null}
        </div>
      </div>
    </PageContentFooter>
  );
}

export default ImportGameFooter;
