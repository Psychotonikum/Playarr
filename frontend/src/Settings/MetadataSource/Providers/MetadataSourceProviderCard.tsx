import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { icons, kinds } from 'Helpers/Props';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import {
  MetadataSourceProviderModel,
  useDeleteMetadataSourceProvider,
} from '../useMetadataSourceProviders';
import EditMetadataSourceProviderModal from './EditMetadataSourceProviderModal';
import styles from './MetadataSourceProviderCard.css';

interface MetadataSourceProviderCardProps extends MetadataSourceProviderModel {
  onClonePress: (id: number) => void;
}

function MetadataSourceProviderCard({
  id,
  name,
  enableSearch,
  enableCalendar,
  downloadMetadata,
  tags,
  onClonePress,
}: MetadataSourceProviderCardProps) {
  const tagList = useTagList();
  const { deleteProvider } = useDeleteMetadataSourceProvider(id);

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

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

  const handleDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  const handleConfirmDelete = useCallback(() => {
    deleteProvider();
  }, [deleteProvider]);

  const handleClonePress = useCallback(() => {
    onClonePress(id);
  }, [id, onClonePress]);

  return (
    <Card
      className={styles.provider}
      overlayContent={true}
      onPress={handleEditPress}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name}</div>

        <IconButton
          className={styles.cloneButton}
          title="Clone"
          name={icons.CLONE}
          onPress={handleClonePress}
        />
      </div>

      <div className={styles.enabled}>
        {enableSearch ? (
          <Label kind={kinds.SUCCESS}>Search</Label>
        ) : null}

        {enableCalendar ? (
          <Label kind={kinds.SUCCESS}>Calendar</Label>
        ) : null}

        {downloadMetadata ? (
          <Label kind={kinds.SUCCESS}>Download Metadata</Label>
        ) : null}

        {!enableSearch && !enableCalendar ? (
          <Label kind={kinds.DISABLED} outline={true}>
            {translate('Disabled')}
          </Label>
        ) : null}
      </div>

      <TagList tags={tags} tagList={tagList} />

      <EditMetadataSourceProviderModal
        id={id}
        isOpen={isEditModalOpen}
        onModalClose={handleEditModalClose}
        onDeleteMetadataSourceProviderPress={handleDeletePress}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title="Delete Metadata Source"
        message={`Are you sure you want to delete the metadata source '${name}'?`}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDelete}
        onCancel={handleDeleteModalClose}
      />
    </Card>
  );
}

export default MetadataSourceProviderCard;
