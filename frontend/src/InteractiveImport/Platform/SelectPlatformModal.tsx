import React from 'react';
import Modal from 'Components/Modal/Modal';
import SelectPlatformModalContent from './SelectPlatformModalContent';

interface SelectPlatformModalProps {
  isOpen: boolean;
  modalTitle: string;
  gameId?: number;
  onSeasonSelect(platformNumber: number): void;
  onModalClose(): void;
}

function SelectPlatformModal(props: SelectPlatformModalProps) {
  const { isOpen, modalTitle, gameId, onSeasonSelect, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectPlatformModalContent
        modalTitle={modalTitle}
        gameId={gameId}
        onSeasonSelect={onSeasonSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectPlatformModal;
