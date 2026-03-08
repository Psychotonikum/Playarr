import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import Label from 'Components/Label';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import { icons, kinds } from 'Helpers/Props';
import GameSystem from 'GameSystem/GameSystem';
import { useDeleteGameSystem } from 'GameSystem/useGameSystems';
import translate from 'Utilities/String/translate';
import EditGameSystemModal from './EditGameSystemModal';
import styles from './GameSystemCard.css';

interface GameSystemCardProps extends GameSystem {
  onClonePress: (id: number) => void;
}

function GameSystemCard(props: GameSystemCardProps) {
  const {
    id,
    name,
    folderName,
    systemType,
    onClonePress,
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
    setIsEditModalOpen(false);
    setIsDeleteModalOpen(true);
  }, []);

  const handleDeleteConfirm = useCallback(() => {
    deleteGameSystem();
    setIsDeleteModalOpen(false);
  }, [deleteGameSystem]);

  const handleDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  const handleClonePress = useCallback(() => {
    onClonePress(id);
  }, [id, onClonePress]);

  return (
    <Card
      className={styles.gameSystem}
      overlayContent={true}
      onPress={handleEditPress}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name}</div>

        <div className={styles.topRight}>
          <Icon
            className={styles.systemIcon}
            name={isPatchable ? icons.FOLDER_OPEN : icons.FOLDER}
            size={18}
            title={isPatchable ? 'Patchable' : 'Classic'}
          />

          <IconButton
            className={styles.cloneButton}
            title="Clone System"
            name={icons.CLONE}
            onPress={handleClonePress}
          />
        </div>
      </div>

      <div className={styles.enabled}>
        <Label kind={isPatchable ? kinds.PRIMARY : kinds.DEFAULT}>
          {isPatchable ? 'Patchable' : 'Classic'}
        </Label>

        <Label kind={kinds.DEFAULT}>
          /{folderName}/
        </Label>
      </div>

      <EditGameSystemModal
        id={id}
        isOpen={isEditModalOpen}
        onModalClose={handleEditModalClose}
        onDeletePress={handleDeletePress}
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
