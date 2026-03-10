import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  AddGameOptions,
  setAddGameOption,
  useAddGameOptions,
} from 'AddGame/addGameOptionsStore';
import { useSelect } from 'App/Select/SelectContext';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContentFooter from 'Components/Page/PageContentFooter';
import Popover from 'Components/Tooltip/Popover';
import { GameMonitor } from 'Game/Game';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
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
  const { monitor: defaultMonitor } = useAddGameOptions();

  const items = useImportGameItems();
  const isLookingUpSeries = useLookupQueueHasItems();

  const [monitor, setMonitor] = useState<GameMonitor | MixedType>(
    defaultMonitor
  );

  const { selectedCount, getSelectedIds } = useSelect<ImportGameItem>();

  const { importSeries, isImporting, importError } = useImportGame();

  const { hasUnsearchedItems, isMonitorMixed } = useMemo(() => {
    let isMonitorMixed = false;
    let hasUnsearchedItems = false;

    items.forEach((item) => {
      if (item.monitor !== defaultMonitor) {
        isMonitorMixed = true;
      }

      if (!item.hasSearched) {
        hasUnsearchedItems = true;
      }
    });

    return {
      hasUnsearchedItems: !isLookingUpSeries && hasUnsearchedItems,
      isMonitorMixed,
    };
  }, [defaultMonitor, items, isLookingUpSeries]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged<string | number | boolean | number[]>) => {
      if (name === 'monitor') {
        setMonitor(value as GameMonitor);
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
