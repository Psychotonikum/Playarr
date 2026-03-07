import React, { useCallback, useState } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import MoveGameModal from 'Game/MoveGame/MoveGameModal';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './EditGameModalContent.css';

interface SavePayload {
  monitored?: boolean;
  monitorNewItems?: string;
  qualityProfileId?: number;
  gameType?: string;
  platformFolder?: boolean;
  rootFolderPath?: string;
  moveFiles?: boolean;
}

export interface EditGameModalContentProps {
  onSavePress(payload: object): void;
  onModalClose(): void;
}

const NO_CHANGE = 'noChange';

const monitoredOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true,
  },
  {
    key: 'monitored',
    get value() {
      return translate('Monitored');
    },
  },
  {
    key: 'unmonitored',
    get value() {
      return translate('Unmonitored');
    },
  },
];

const platformFolderOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true,
  },
  {
    key: 'yes',
    get value() {
      return translate('Yes');
    },
  },
  {
    key: 'no',
    get value() {
      return translate('No');
    },
  },
];

function EditGameModalContent(props: EditGameModalContentProps) {
  const { onSavePress, onModalClose } = props;

  const [monitored, setMonitored] = useState(NO_CHANGE);
  const [monitorNewItems, setMonitorNewItems] = useState(NO_CHANGE);
  const [qualityProfileId, setQualityProfileId] = useState<string | number>(
    NO_CHANGE
  );
  const [gameType, setGameType] = useState(NO_CHANGE);
  const [platformFolder, setPlatformFolder] = useState(NO_CHANGE);
  const [rootFolderPath, setRootFolderPath] = useState(NO_CHANGE);
  const [isConfirmMoveModalOpen, setIsConfirmMoveModalOpen] = useState(false);
  const { selectedCount } = useSelect();

  const save = useCallback(
    (moveFiles: boolean) => {
      let hasChanges = false;
      const payload: SavePayload = {};

      if (monitored !== NO_CHANGE) {
        hasChanges = true;
        payload.monitored = monitored === 'monitored';
      }

      if (monitorNewItems !== NO_CHANGE) {
        hasChanges = true;
        payload.monitorNewItems = monitorNewItems;
      }

      if (qualityProfileId !== NO_CHANGE) {
        hasChanges = true;
        payload.qualityProfileId = qualityProfileId as number;
      }

      if (gameType !== NO_CHANGE) {
        hasChanges = true;
        payload.gameType = gameType;
      }

      if (platformFolder !== NO_CHANGE) {
        hasChanges = true;
        payload.platformFolder = platformFolder === 'yes';
      }

      if (rootFolderPath !== NO_CHANGE) {
        hasChanges = true;
        payload.rootFolderPath = rootFolderPath;
        payload.moveFiles = moveFiles;
      }

      if (hasChanges) {
        onSavePress(payload);
      }

      onModalClose();
    },
    [
      monitored,
      monitorNewItems,
      qualityProfileId,
      gameType,
      platformFolder,
      rootFolderPath,
      onSavePress,
      onModalClose,
    ]
  );

  const onInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      switch (name) {
        case 'monitored':
          setMonitored(value as string);
          break;
        case 'monitorNewItems':
          setMonitorNewItems(value as string);
          break;
        case 'qualityProfileId':
          setQualityProfileId(value as string);
          break;
        case 'gameType':
          setGameType(value as string);
          break;
        case 'platformFolder':
          setPlatformFolder(value as string);
          break;
        case 'rootFolderPath':
          setRootFolderPath(value as string);
          break;
        default:
          console.warn('EditGameModalContent Unknown Input');
      }
    },
    [setMonitored]
  );

  const onSavePressWrapper = useCallback(() => {
    if (rootFolderPath === NO_CHANGE) {
      save(false);
    } else {
      setIsConfirmMoveModalOpen(true);
    }
  }, [rootFolderPath, save]);

  const onCancelPress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
  }, [setIsConfirmMoveModalOpen]);

  const onDoNotMoveGamePress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
    save(false);
  }, [setIsConfirmMoveModalOpen, save]);

  const onMoveGamePress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
    save(true);
  }, [setIsConfirmMoveModalOpen, save]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditSelectedSeries')}</ModalHeader>

      <ModalBody>
        <FormGroup>
          <FormLabel>{translate('Monitored')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="monitored"
            value={monitored}
            values={monitoredOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('MonitorNewItems')}</FormLabel>

          <FormInputGroup
            type={inputTypes.MONITOR_NEW_ITEMS_SELECT}
            name="monitorNewItems"
            value={monitorNewItems}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('QualityProfile')}</FormLabel>

          <FormInputGroup
            type={inputTypes.QUALITY_PROFILE_SELECT}
            name="qualityProfileId"
            value={qualityProfileId}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('GameType')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SERIES_TYPE_SELECT}
            name="gameType"
            value={gameType}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            helpText={translate('GameTypesHelpText')}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('PlatformFolder')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="platformFolder"
            value={platformFolder}
            values={platformFolderOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('RootFolder')}</FormLabel>

          <FormInputGroup
            type={inputTypes.ROOT_FOLDER_SELECT}
            name="rootFolderPath"
            value={rootFolderPath}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            selectedValueOptions={{ includeFreeSpace: false }}
            helpText={translate('SeriesEditRootFolderHelpText')}
            onChange={onInputChange}
          />
        </FormGroup>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div className={styles.selected}>
          {translate('CountSeriesSelected', { count: selectedCount })}
        </div>

        <div>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <Button onPress={onSavePressWrapper}>
            {translate('ApplyChanges')}
          </Button>
        </div>
      </ModalFooter>

      <MoveGameModal
        isOpen={isConfirmMoveModalOpen}
        destinationRootFolder={rootFolderPath}
        onModalClose={onCancelPress}
        onSavePress={onDoNotMoveGamePress}
        onMoveGamePress={onMoveGamePress}
      />
    </ModalContent>
  );
}

export default EditGameModalContent;
