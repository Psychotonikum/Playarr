import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import { icons, kinds } from 'Helpers/Props';
import GameSystem from 'GameSystem/GameSystem';
import { useDeleteGameSystem } from 'GameSystem/useGameSystems';
import translate from 'Utilities/String/translate';
import EditGameSystemModal from './EditGameSystemModal';
import styles from './GameSystemCard.css';

interface GameSystemCardProps extends GameSystem {}

function GameSystemCard(props: GameSystemCardProps) {
  const {
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
  } = props;

  const { deleteGameSystem } = useDeleteGameSystem(id);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

  const isPatchable = systemType === 1;

  const handleEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, []);

  const handleEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, []);

  const handleDeletePress = useCallback(() => {
    setIsDeleteModalOpen(true);
  }, []);

  const handleDeleteConfirm = useCallback(() => {
    deleteGameSystem();
    setIsDeleteModalOpen(false);
  }, [deleteGameSystem]);

  const handleDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  return (
    <Card className={styles.gameSystem} overlayContent={true}>
      <div className={styles.name}>{name}</div>

      <div className={styles.folder}>/{folderName}/</div>

      <div className={styles.type}>
        <Label kind={isPatchable ? kinds.PRIMARY : kinds.DEFAULT}>
          {isPatchable ? 'Patchable' : 'Classic'}
        </Label>
      </div>

      {fileExtensions.length > 0 && (
        <div className={styles.extensions}>
          {fileExtensions.join(', ')}
        </div>
      )}

      <div className={styles.naming}>
        <div>
          <span className={styles.namingLabel}>Naming:</span>
          {namingFormat}
        </div>

        {isPatchable && updateNamingFormat && (
          <div>
            <span className={styles.namingLabel}>Update:</span>
            {updateNamingFormat}
          </div>
        )}

        {isPatchable && dlcNamingFormat && (
          <div>
            <span className={styles.namingLabel}>DLC:</span>
            {dlcNamingFormat}
          </div>
        )}

        {isPatchable && (
          <div>
            <span className={styles.namingLabel}>Folders:</span>
            {baseFolderName}/{updateFolderName}/{dlcFolderName}
          </div>
        )}
      </div>

      <div className={styles.actions}>
        <IconButton name={icons.EDIT} onPress={handleEditPress} />
        <IconButton name={icons.DELETE} kind={kinds.DANGER} onPress={handleDeletePress} />
      </div>

      <EditGameSystemModal
        id={id}
        isOpen={isEditModalOpen}
        onModalClose={handleEditModalClose}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('Delete')}
        message={`Are you sure you want to delete the system '${name}'?`}
        confirmLabel={translate('Delete')}
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteModalClose}
      />
    </Card>
  );
}

export default GameSystemCard;
