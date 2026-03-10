import React, { useCallback, useState } from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import GameSystem from 'GameSystem/GameSystem';
import useGameSystems, {
  useAddGameSystem,
  useUpdateGameSystem,
} from 'GameSystem/useGameSystems';
import { inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { GameSystemPreset } from './gameSystemPresets';
import styles from './EditGameSystemModalContent.css';

interface EditGameSystemModalContentProps {
  id?: number;
  cloneId?: number;
  preset?: GameSystemPreset | null;
  onModalClose: () => void;
  onDeletePress?: () => void;
}

const SYSTEM_TYPE_OPTIONS = [
  { key: 0, value: 'Classic (single-file ROMs)' },
  { key: 1, value: 'Patchable (base/update/DLC)' },
];

function EditGameSystemModalContent({
  id,
  cloneId,
  preset,
  onModalClose,
  onDeletePress,
}: EditGameSystemModalContentProps) {
  const { data: systems } = useGameSystems();
  const sourceId = id ?? cloneId;
  const existing = sourceId
    ? systems.find((s) => s.id === sourceId)
    : undefined;

  const defaults = preset ?? existing;

  const [name, setName] = useState(
    cloneId && existing ? `${existing.name} (Copy)` : defaults?.name ?? ''
  );
  const [folderName, setFolderName] = useState(defaults?.folderName ?? '');
  const [systemType, setSystemType] = useState(defaults?.systemType ?? 0);
  const [fileExtensions, setFileExtensions] = useState(
    defaults?.fileExtensions?.join(', ') ?? ''
  );
  const [namingFormat, setNamingFormat] = useState(
    defaults?.namingFormat ?? '{Game Title} {Region}.{Extension}'
  );
  const [updateNamingFormat, setUpdateNamingFormat] = useState(
    defaults?.updateNamingFormat ?? '{Game Title} v{Version}.{Extension}'
  );
  const [dlcNamingFormat, setDlcNamingFormat] = useState(
    defaults?.dlcNamingFormat ?? '{Game Title} DLC{Index}.{Extension}'
  );
  const [baseFolderName, setBaseFolderName] = useState(
    defaults?.baseFolderName ?? 'base'
  );
  const [updateFolderName, setUpdateFolderName] = useState(
    defaults?.updateFolderName ?? 'update'
  );
  const [dlcFolderName, setDlcFolderName] = useState(
    defaults?.dlcFolderName ?? 'dlc'
  );

  const { addGameSystem, isAdding, addError } = useAddGameSystem();

  const updateHook = id ? useUpdateGameSystem(id) : null;

  const isPatchable = systemType === 1;

  const handleInputChange = useCallback(
    ({ name: fieldName, value }: InputChanged) => {
      switch (fieldName) {
        case 'name':
          setName(value as string);
          break;
        case 'folderName':
          setFolderName(value as string);
          break;
        case 'systemType':
          setSystemType(value as number);
          break;
        case 'fileExtensions':
          setFileExtensions(value as string);
          break;
        case 'namingFormat':
          setNamingFormat(value as string);
          break;
        case 'updateNamingFormat':
          setUpdateNamingFormat(value as string);
          break;
        case 'dlcNamingFormat':
          setDlcNamingFormat(value as string);
          break;
        case 'baseFolderName':
          setBaseFolderName(value as string);
          break;
        case 'updateFolderName':
          setUpdateFolderName(value as string);
          break;
        case 'dlcFolderName':
          setDlcFolderName(value as string);
          break;
      }
    },
    []
  );

  const handleSavePress = useCallback(() => {
    const extensions = fileExtensions
      .split(',')
      .map((e) => e.trim())
      .filter((e) => e.length > 0);

    const payload: GameSystem = {
      id: id ?? 0,
      name,
      folderName,
      systemType,
      fileExtensions: extensions,
      namingFormat,
      updateNamingFormat: isPatchable ? updateNamingFormat : '',
      dlcNamingFormat: isPatchable ? dlcNamingFormat : '',
      baseFolderName: isPatchable ? baseFolderName : '',
      updateFolderName: isPatchable ? updateFolderName : '',
      dlcFolderName: isPatchable ? dlcFolderName : '',
      tags: existing?.tags ?? [],
    };

    if (id && updateHook) {
      updateHook.updateGameSystem(payload);
    } else {
      addGameSystem(payload);
    }

    onModalClose();
  }, [
    id,
    name,
    folderName,
    systemType,
    fileExtensions,
    namingFormat,
    updateNamingFormat,
    dlcNamingFormat,
    baseFolderName,
    updateFolderName,
    dlcFolderName,
    isPatchable,
    existing,
    addGameSystem,
    updateHook,
    onModalClose,
  ]);

  const isNew = !id;
  const isSaving = isAdding || (updateHook?.isUpdating ?? false);
  const saveError = addError || updateHook?.updateError;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{isNew ? 'Add Game System' : `Edit ${name}`}</ModalHeader>

      <ModalBody>
        <Form>
          <FormGroup>
            <FormLabel>Name</FormLabel>
            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              value={name}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Folder Name</FormLabel>
            <FormInputGroup
              type={inputTypes.TEXT}
              name="folderName"
              value={folderName}
              helpText="Lowercase folder name under /roms/ (e.g., snes, switch)"
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>System Type</FormLabel>
            <FormInputGroup
              type={inputTypes.SELECT}
              name="systemType"
              value={systemType}
              values={SYSTEM_TYPE_OPTIONS}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>File Extensions</FormLabel>
            <FormInputGroup
              type={inputTypes.TEXT}
              name="fileExtensions"
              value={fileExtensions}
              helpText="Comma-separated file extensions (e.g., .sfc, .smc)"
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Naming Format</FormLabel>
            <FormInputGroup
              type={inputTypes.TEXT}
              name="namingFormat"
              value={namingFormat}
              helpText="Aerofoil naming: {Game Title} {Region}.{Extension}"
              onChange={handleInputChange}
            />
          </FormGroup>

          {isPatchable ? (
            <>
              <FormGroup>
                <FormLabel>Update Naming Format</FormLabel>
                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="updateNamingFormat"
                  value={updateNamingFormat}
                  helpText="{Game Title} v{Version}.{Extension}"
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>DLC Naming Format</FormLabel>
                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="dlcNamingFormat"
                  value={dlcNamingFormat}
                  helpText="{Game Title} DLC{Index}.{Extension}"
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Base Folder</FormLabel>
                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="baseFolderName"
                  value={baseFolderName}
                  helpText="Subfolder name for base game files"
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Update Folder</FormLabel>
                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="updateFolderName"
                  value={updateFolderName}
                  helpText="Subfolder name for update files"
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>DLC Folder</FormLabel>
                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="dlcFolderName"
                  value={dlcFolderName}
                  helpText="Subfolder name for DLC files"
                  onChange={handleInputChange}
                />
              </FormGroup>
            </>
          ) : null}
        </Form>

        {saveError ? (
          <Alert kind={kinds.DANGER}>Failed to save system.</Alert>
        ) : null}
      </ModalBody>

      <ModalFooter>
        {!isNew && onDeletePress ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeletePress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditGameSystemModalContent;
