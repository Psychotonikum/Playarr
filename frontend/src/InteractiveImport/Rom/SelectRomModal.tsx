import React from 'react';
import Modal from 'Components/Modal/Modal';
import SelectRomModalContent, {
  SelectedEpisode,
} from './SelectRomModalContent';

interface SelectRomModalProps {
  isOpen: boolean;
  selectedIds: number[] | string[];
  gameId?: number;
  platformNumber?: number;
  selectedDetails?: string;
  isAnime: boolean;
  modalTitle: string;
  onEpisodesSelect(selectedEpisodes: SelectedEpisode[]): void;
  onModalClose(): void;
}

function SelectRomModal(props: SelectRomModalProps) {
  const {
    isOpen,
    selectedIds,
    gameId,
    platformNumber,
    selectedDetails,
    isAnime,
    modalTitle,
    onEpisodesSelect,
    onModalClose,
  } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectRomModalContent
        selectedIds={selectedIds}
        gameId={gameId}
        platformNumber={platformNumber}
        selectedDetails={selectedDetails}
        isAnime={isAnime}
        modalTitle={modalTitle}
        onEpisodesSelect={onEpisodesSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectRomModal;
