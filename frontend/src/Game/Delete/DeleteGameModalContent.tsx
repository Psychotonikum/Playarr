import React, { useCallback, useState } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import { Statistics } from 'Game/Game';
import {
  setGameDeleteOptions,
  useGameDeleteOptions,
} from 'Game/gameOptionsStore';
import { useDeleteGame, useSingleGame } from 'Game/useGame';
import { CheckInputChanged } from 'typings/inputs';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './DeleteGameModalContent.css';

export interface DeleteGameModalContentProps {
  gameId: number;
  onModalClose: () => void;
}

function DeleteGameModalContent({
  gameId,
  onModalClose,
}: DeleteGameModalContentProps) {
  const {
    title,
    path,
    statistics = {} as Statistics,
  } = useSingleGame(gameId)!;

  const { addImportListExclusion } = useGameDeleteOptions();
  const { romFileCount = 0, sizeOnDisk = 0 } = statistics;
  const [deleteFiles, setDeleteFiles] = useState(false);

  const { deleteGame } = useDeleteGame(gameId, {
    deleteFiles,
    addImportListExclusion,
  });

  const handleDeleteFilesChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setDeleteFiles(value);
    },
    []
  );

  const handleDeleteGameConfirmed = useCallback(() => {
    deleteGame();

    onModalClose();
  }, [deleteGame, onModalClose]);

  const handleDeleteOptionChange = useCallback(
    ({ name, value }: CheckInputChanged) => {
      setGameDeleteOptions({ [name]: value });
    },
    []
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('DeleteGameModalHeader', { title })}
      </ModalHeader>

      <ModalBody>
        <div className={styles.pathContainer}>
          <Icon className={styles.pathIcon} name={icons.FOLDER} />

          {path}
        </div>

        <FormGroup>
          <FormLabel>{translate('AddListExclusion')}</FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="addImportListExclusion"
            value={addImportListExclusion}
            helpText={translate('AddListExclusionSeriesHelpText')}
            onChange={handleDeleteOptionChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>
            {romFileCount === 0
              ? translate('DeleteGameFolder')
              : translate('DeleteEpisodesFiles', { romFileCount })}
          </FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="deleteFiles"
            value={deleteFiles}
            helpText={
              romFileCount === 0
                ? translate('DeleteGameFolderHelpText')
                : translate('DeleteEpisodesFilesHelpText')
            }
            kind={kinds.DANGER}
            onChange={handleDeleteFilesChange}
          />
        </FormGroup>

        {deleteFiles ? (
          <div className={styles.deleteFilesMessage}>
            <div>
              <InlineMarkdown
                data={translate('DeleteGameFolderConfirmation', { path })}
                blockClassName={styles.folderPath}
              />
            </div>

            {romFileCount ? (
              <div className={styles.deleteCount}>
                {translate('DeleteGameFolderRomCount', {
                  romFileCount,
                  size: formatBytes(sizeOnDisk),
                })}
              </div>
            ) : null}
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>

        <Button kind={kinds.DANGER} onPress={handleDeleteGameConfirmed}>
          {translate('Delete')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default DeleteGameModalContent;
