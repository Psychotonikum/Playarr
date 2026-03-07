import React, { useCallback, useEffect, useMemo, useState } from 'react';
import AddGame from 'AddGame/AddGame';
import {
  AddGameOptions,
  setAddGameOption,
  useAddGameOptions,
} from 'AddGame/addGameOptionsStore';
import GameMonitoringOptionsPopoverContent from 'AddGame/GameMonitoringOptionsPopoverContent';
import GameTypePopoverContent from 'AddGame/GameTypePopoverContent';
import { useAppDimension } from 'App/appStore';
import CheckInput from 'Components/Form/CheckInput';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import { getValidationFailures } from 'Helpers/Hooks/useApiMutation';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import { GameType } from 'Game/Game';
import GamePoster from 'Game/GamePoster';
import selectSettings from 'Store/Selectors/selectSettings';
import { useIsWindows } from 'System/Status/useSystemStatus';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { useAddGame } from './useAddGame';
import styles from './AddNewGameModalContent.css';

export interface AddNewGameModalContentProps {
  game: AddGame;
  initialGameType: GameType;
  onModalClose: () => void;
}

function AddNewGameModalContent({
  game,
  initialGameType,
  onModalClose,
}: AddNewGameModalContentProps) {
  const { title, year, overview, images, folder } = game;
  const options = useAddGameOptions();
  const isSmallScreen = useAppDimension('isSmallScreen');
  const isWindows = useIsWindows();

  const { isAdding, addError, addGame } = useAddGame();

  const { settings, validationErrors, validationWarnings } = useMemo(() => {
    return {
      ...selectSettings(options, {}),
      ...getValidationFailures(addError),
    };
  }, [options, addError]);

  const [gameType, setGameType] = useState<GameType>(
    initialGameType === 'standard'
      ? settings.gameType.value
      : initialGameType
  );

  const {
    monitor,
    qualityProfileId,
    rootFolderPath,
    searchForCutoffUnmetRoms,
    searchForMissingRoms,
    platformFolder,
    gameType: gameTypeSetting,
    tags,
  } = settings;

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged<string | number | boolean | number[]>) => {
      setAddGameOption(name as keyof AddGameOptions, value);
    },
    []
  );

  const handleQualityProfileIdChange = useCallback(
    ({ value }: InputChanged<string | number>) => {
      setAddGameOption('qualityProfileId', value as number);
    },
    []
  );

  const handleAddGamePress = useCallback(() => {
    addGame({
      ...game,
      rootFolderPath: rootFolderPath.value,
      addOptions: {
        monitor: monitor.value,
        searchForMissingRoms: searchForMissingRoms.value,
        searchForCutoffUnmetRoms: searchForCutoffUnmetRoms.value,
      },
      qualityProfileId: qualityProfileId.value,
      gameType,
      platformFolder: platformFolder.value,
      tags: tags.value,
    });
  }, [
    game,
    gameType,
    rootFolderPath,
    monitor,
    qualityProfileId,
    platformFolder,
    searchForMissingRoms,
    searchForCutoffUnmetRoms,
    tags,
    addGame,
  ]);

  useEffect(() => {
    setGameType(gameTypeSetting.value);
  }, [gameTypeSetting]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {title}

        {!title.includes(String(year)) && year ? (
          <span className={styles.year}>({year})</span>
        ) : null}
      </ModalHeader>

      <ModalBody>
        <div className={styles.container}>
          {isSmallScreen ? null : (
            <div className={styles.poster}>
              <GamePoster
                className={styles.poster}
                images={images}
                size={250}
                title={title}
              />
            </div>
          )}

          <div className={styles.info}>
            {overview ? (
              <div className={styles.overview}>{overview}</div>
            ) : null}

            <Form
              validationErrors={validationErrors}
              validationWarnings={validationWarnings}
            >
              <FormGroup>
                <FormLabel>{translate('RootFolder')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.ROOT_FOLDER_SELECT}
                  name="rootFolderPath"
                  valueOptions={{
                    seriesFolder: folder,
                    isWindows,
                  }}
                  selectedValueOptions={{
                    seriesFolder: folder,
                    isWindows,
                  }}
                  helpText={translate('AddNewGameRootFolderHelpText', {
                    folder,
                  })}
                  onChange={handleInputChange}
                  {...rootFolderPath}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  {translate('Monitor')}

                  <Popover
                    anchor={
                      <Icon className={styles.labelIcon} name={icons.INFO} />
                    }
                    title={translate('MonitoringOptions')}
                    body={<GameMonitoringOptionsPopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.MONITOR_EPISODES_SELECT}
                  name="monitor"
                  onChange={handleInputChange}
                  {...monitor}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('QualityProfile')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  onChange={handleQualityProfileIdChange}
                  {...qualityProfileId}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  {translate('GameType')}

                  <Popover
                    anchor={
                      <Icon className={styles.labelIcon} name={icons.INFO} />
                    }
                    title={translate('GameTypes')}
                    body={<GameTypePopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.SERIES_TYPE_SELECT}
                  name="gameType"
                  onChange={handleInputChange}
                  {...gameTypeSetting}
                  value={gameType}
                  helpText={translate('GameTypesHelpText')}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('PlatformFolder')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="platformFolder"
                  onChange={handleInputChange}
                  {...platformFolder}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('Tags')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  onChange={handleInputChange}
                  {...tags}
                />
              </FormGroup>
            </Form>
          </div>
        </div>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div>
          <label className={styles.searchLabelContainer}>
            <span className={styles.searchLabel}>
              {translate('AddNewGameSearchForMissingEpisodes')}
            </span>

            <CheckInput
              containerClassName={styles.searchInputContainer}
              className={styles.searchInput}
              name="searchForMissingRoms"
              onChange={handleInputChange}
              {...searchForMissingRoms}
            />
          </label>

          <label className={styles.searchLabelContainer}>
            <span className={styles.searchLabel}>
              {translate('AddNewGameSearchForCutoffUnmetEpisodes')}
            </span>

            <CheckInput
              containerClassName={styles.searchInputContainer}
              className={styles.searchInput}
              name="searchForCutoffUnmetRoms"
              onChange={handleInputChange}
              {...searchForCutoffUnmetRoms}
            />
          </label>
        </div>

        <SpinnerButton
          className={styles.addButton}
          kind={kinds.SUCCESS}
          isSpinning={isAdding}
          onPress={handleAddGamePress}
        >
          {translate('AddGameWithTitle', { title })}
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddNewGameModalContent;
