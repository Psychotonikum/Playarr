import React, { useCallback, useEffect, useMemo, useState } from 'react';
import GameMonitorNewItemsOptionsPopoverContent from 'AddGame/GameMonitorNewItemsOptionsPopoverContent';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import { usePendingChangesStore } from 'Helpers/Hooks/usePendingChangesStore';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  icons,
  inputTypes,
  kinds,
  sizes,
  tooltipPositions,
} from 'Helpers/Props';
import MoveGameModal from 'Game/MoveGame/MoveGameModal';
import Game from 'Game/Game';
import { useSaveSeries, useSingleGame } from 'Game/useGame';
import selectSettings from 'Store/Selectors/selectSettings';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import RootFolderModal from './RootFolder/RootFolderModal';
import { RootFolderUpdated } from './RootFolder/RootFolderModalContent';
import styles from './EditGameModalContent.css';

export interface EditGameModalContentProps {
  gameId: number;
  onModalClose: () => void;
  onDeleteGamePress: () => void;
}
function EditGameModalContent({
  gameId,
  onModalClose,
  onDeleteGamePress,
}: EditGameModalContentProps) {
  const game = useSingleGame(gameId)!;

  const {
    title,
    monitored,
    monitorNewItems,
    platformFolder,
    qualityProfileId,
    gameType,
    path,
    tags,
    rootFolderPath: initialRootFolderPath,
  } = game;

  const { pendingChanges, setPendingChange } = usePendingChangesStore<Game>(
    {}
  );

  const [isRootFolderModalOpen, setIsRootFolderModalOpen] = useState(false);
  const [rootFolderPath, setRootFolderPath] = useState(initialRootFolderPath);
  const isPathChanging = !!(
    pendingChanges.path && path !== pendingChanges.path
  );
  const [isConfirmMoveModalOpen, setIsConfirmMoveModalOpen] = useState(false);

  const { saveSeries, isSaving, saveError } = useSaveSeries(isPathChanging);
  const wasSaving = usePrevious(isSaving);

  const { settings, ...otherSettings } = useMemo(() => {
    return selectSettings(
      {
        monitored,
        monitorNewItems,
        platformFolder,
        qualityProfileId,
        gameType,
        path,
        tags,
      },
      pendingChanges,
      saveError
    );
  }, [
    monitored,
    monitorNewItems,
    platformFolder,
    qualityProfileId,
    gameType,
    path,
    tags,
    pendingChanges,
    saveError,
  ]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error name needs to be keyof Game
      setPendingChange(name, value);
    },
    [setPendingChange]
  );

  const handleRootFolderPress = useCallback(() => {
    setIsRootFolderModalOpen(true);
  }, []);

  const handleRootFolderModalClose = useCallback(() => {
    setIsRootFolderModalOpen(false);
  }, []);

  const handleRootFolderChange = useCallback(
    ({
      path: newPath,
      rootFolderPath: newRootFolderPath,
    }: RootFolderUpdated) => {
      setIsRootFolderModalOpen(false);
      setRootFolderPath(newRootFolderPath);
      handleInputChange({ name: 'path', value: newPath });
    },
    [handleInputChange]
  );

  const handleCancelPress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
  }, []);

  const handleSavePress = useCallback(() => {
    if (isPathChanging && !isConfirmMoveModalOpen) {
      setIsConfirmMoveModalOpen(true);
    } else {
      setIsConfirmMoveModalOpen(false);

      saveSeries({
        ...game,
        ...pendingChanges,
      });
    }
  }, [
    game,
    isPathChanging,
    isConfirmMoveModalOpen,
    pendingChanges,
    saveSeries,
  ]);

  const handleMoveGamePress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);

    saveSeries({
      ...game,
      ...pendingChanges,
    });
  }, [game, pendingChanges, saveSeries]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditGameModalHeader', { title })}</ModalHeader>

      <ModalBody>
        <Form {...otherSettings}>
          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('Monitored')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="monitored"
              helpText={translate('MonitoredEpisodesHelpText')}
              {...settings.monitored}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>
              {translate('MonitorNewSeasons')}
              <Popover
                anchor={<Icon className={styles.labelIcon} name={icons.INFO} />}
                title={translate('MonitorNewSeasons')}
                body={<GameMonitorNewItemsOptionsPopoverContent />}
                position={tooltipPositions.RIGHT}
              />
            </FormLabel>

            <FormInputGroup
              type={inputTypes.MONITOR_NEW_ITEMS_SELECT}
              name="monitorNewItems"
              helpText={translate('MonitorNewSeasonsHelpText')}
              {...settings.monitorNewItems}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('UsePlatformFolder')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="platformFolder"
              helpText={translate('UsePlatformFolderHelpText')}
              {...settings.platformFolder}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('QualityProfile')}</FormLabel>

            <FormInputGroup
              type={inputTypes.QUALITY_PROFILE_SELECT}
              name="qualityProfileId"
              {...settings.qualityProfileId}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('GameType')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SERIES_TYPE_SELECT}
              name="gameType"
              {...settings.gameType}
              helpText={translate('GameTypesHelpText')}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('Path')}</FormLabel>

            <FormInputGroup
              type={inputTypes.PATH}
              name="path"
              {...settings.path}
              buttons={[
                <FormInputButton
                  key="fileBrowser"
                  kind={kinds.DEFAULT}
                  title={translate('RootFolder')}
                  onPress={handleRootFolderPress}
                >
                  <Icon name={icons.ROOT_FOLDER} />
                </FormInputButton>,
              ]}
              includeFiles={false}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              {...settings.tags}
              onChange={handleInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button
          className={styles.deleteButton}
          kind={kinds.DANGER}
          onPress={onDeleteGamePress}
        >
          {translate('Delete')}
        </Button>

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          error={saveError}
          isSpinning={isSaving}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>

      <RootFolderModal
        isOpen={isRootFolderModalOpen}
        gameId={gameId}
        rootFolderPath={rootFolderPath}
        onSavePress={handleRootFolderChange}
        onModalClose={handleRootFolderModalClose}
      />

      <MoveGameModal
        originalPath={path}
        destinationPath={pendingChanges.path}
        isOpen={isConfirmMoveModalOpen}
        onModalClose={handleCancelPress}
        onSavePress={handleSavePress}
        onMoveGamePress={handleMoveGamePress}
      />
    </ModalContent>
  );
}

export default EditGameModalContent;
