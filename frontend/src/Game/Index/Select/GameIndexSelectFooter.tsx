import React, { useCallback, useEffect, useState } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting } from 'Commands/useCommands';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import Game from 'Game/Game';
import {
  useBulkDeleteGame,
  useSaveGameEditor,
  useUpdateGameMonitor,
} from 'Game/useGame';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import DeleteGameModal from './Delete/DeleteGameModal';
import EditGameModal from './Edit/EditGameModal';
import OrganizeGameModal from './Organize/OrganizeGameModal';
import ChangeMonitoringModal from './PlatformPass/ChangeMonitoringModal';
import TagsModal from './Tags/TagsModal';
import styles from './GameIndexSelectFooter.css';

interface SavePayload {
  monitored?: boolean;
  qualityProfileId?: number;
  gameType?: string;
  platformFolder?: boolean;
  rootFolderPath?: string;
  moveFiles?: boolean;
}

function GameIndexSelectFooter() {
  const { saveGameEditor, isSavingGameEditor } = useSaveGameEditor();
  const { updateGameMonitor, isUpdatingGameMonitor } = useUpdateGameMonitor();
  const { isBulkDeleting, bulkDeleteError } = useBulkDeleteGame();

  const isOrganizingSeries = useCommandExecuting(CommandNames.RenameSeries);

  const isSaving = isSavingGameEditor || isUpdatingGameMonitor;
  const isDeleting = isBulkDeleting;
  const deleteError = bulkDeleteError;

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isTagsModalOpen, setIsTagsModalOpen] = useState(false);
  const [isMonitoringModalOpen, setIsMonitoringModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isSavingSeries, setIsSavingSeries] = useState(false);
  const [isSavingTags, setIsSavingTags] = useState(false);
  const [isSavingMonitoring, setIsSavingMonitoring] = useState(false);
  const previousIsDeleting = usePrevious(isDeleting);
  const { selectedCount, unselectAll, useSelectedIds } = useSelect<Game>();
  const gameIds = useSelectedIds();

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const onSavePress = useCallback(
    (payload: SavePayload) => {
      setIsSavingSeries(true);
      setIsEditModalOpen(false);

      saveGameEditor({
        ...payload,
        gameIds,
      });
    },
    [gameIds, saveGameEditor]
  );

  const onOrganizePress = useCallback(() => {
    setIsOrganizeModalOpen(true);
  }, [setIsOrganizeModalOpen]);

  const onOrganizeModalClose = useCallback(() => {
    setIsOrganizeModalOpen(false);
  }, [setIsOrganizeModalOpen]);

  const onTagsPress = useCallback(() => {
    setIsTagsModalOpen(true);
  }, [setIsTagsModalOpen]);

  const onTagsModalClose = useCallback(() => {
    setIsTagsModalOpen(false);
  }, [setIsTagsModalOpen]);

  const onApplyTagsPress = useCallback(
    (tags: number[], _applyTags: string) => {
      setIsSavingTags(true);
      setIsTagsModalOpen(false);

      saveGameEditor({
        gameIds,
        tags,
      });
    },
    [gameIds, saveGameEditor]
  );

  const onMonitoringPress = useCallback(() => {
    setIsMonitoringModalOpen(true);
  }, [setIsMonitoringModalOpen]);

  const onMonitoringClose = useCallback(() => {
    setIsMonitoringModalOpen(false);
  }, [setIsMonitoringModalOpen]);

  const onMonitoringSavePress = useCallback(
    (monitor: string) => {
      setIsSavingMonitoring(true);
      setIsMonitoringModalOpen(false);

      updateGameMonitor({
        game: gameIds.map((id) => ({ id })),
        monitoringOptions: { monitor },
      });
    },
    [gameIds, updateGameMonitor]
  );

  const onDeletePress = useCallback(() => {
    setIsDeleteModalOpen(true);
  }, [setIsDeleteModalOpen]);

  const onDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  useEffect(() => {
    if (!isSaving) {
      setIsSavingSeries(false);
      setIsSavingTags(false);
      setIsSavingMonitoring(false);
    }
  }, [isSaving]);

  useEffect(() => {
    if (previousIsDeleting && !isDeleting && !deleteError) {
      unselectAll();
    }
  }, [previousIsDeleting, isDeleting, deleteError, unselectAll]);

  const anySelected = selectedCount > 0;

  return (
    <PageContentFooter className={styles.footer}>
      <div className={styles.buttons}>
        <div className={styles.actionButtons}>
          <SpinnerButton
            isSpinning={isSaving && isSavingSeries}
            isDisabled={!anySelected || isOrganizingSeries}
            onPress={onEditPress}
          >
            {translate('Edit')}
          </SpinnerButton>

          <SpinnerButton
            kind={kinds.WARNING}
            isSpinning={isOrganizingSeries}
            isDisabled={!anySelected || isOrganizingSeries}
            onPress={onOrganizePress}
          >
            {translate('RenameFiles')}
          </SpinnerButton>

          <SpinnerButton
            isSpinning={isSaving && isSavingTags}
            isDisabled={!anySelected || isOrganizingSeries}
            onPress={onTagsPress}
          >
            {translate('SetTags')}
          </SpinnerButton>

          <SpinnerButton
            isSpinning={isSaving && isSavingMonitoring}
            isDisabled={!anySelected || isOrganizingSeries}
            onPress={onMonitoringPress}
          >
            {translate('UpdateMonitoring')}
          </SpinnerButton>
        </div>

        <div className={styles.deleteButtons}>
          <SpinnerButton
            kind={kinds.DANGER}
            isSpinning={isDeleting}
            isDisabled={!anySelected || isDeleting}
            onPress={onDeletePress}
          >
            {translate('Delete')}
          </SpinnerButton>
        </div>
      </div>

      <div className={styles.selected}>
        {translate('CountSeriesSelected', { count: selectedCount })}
      </div>

      <EditGameModal
        isOpen={isEditModalOpen}
        onSavePress={onSavePress}
        onModalClose={onEditModalClose}
      />

      <TagsModal
        isOpen={isTagsModalOpen}
        onApplyTagsPress={onApplyTagsPress}
        onModalClose={onTagsModalClose}
      />

      <ChangeMonitoringModal
        isOpen={isMonitoringModalOpen}
        onSavePress={onMonitoringSavePress}
        onModalClose={onMonitoringClose}
      />

      <OrganizeGameModal
        isOpen={isOrganizeModalOpen}
        onModalClose={onOrganizeModalClose}
      />

      <DeleteGameModal
        isOpen={isDeleteModalOpen}
        onModalClose={onDeleteModalClose}
      />
    </PageContentFooter>
  );
}

export default GameIndexSelectFooter;
