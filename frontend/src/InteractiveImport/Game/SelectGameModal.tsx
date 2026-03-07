import React from 'react';
import Modal from 'Components/Modal/Modal';
import Game from 'Game/Game';
import SelectGameModalContent from './SelectGameModalContent';

interface SelectGameModalProps {
  isOpen: boolean;
  modalTitle: string;
  onSeriesSelect(game: Game): void;
  onModalClose(): void;
}

function SelectGameModal(props: SelectGameModalProps) {
  const { isOpen, modalTitle, onSeriesSelect, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectGameModalContent
        modalTitle={modalTitle}
        onSeriesSelect={onSeriesSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectGameModal;
