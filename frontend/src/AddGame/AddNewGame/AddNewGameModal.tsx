import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddNewGameModalContent, {
  AddNewGameModalContentProps,
} from './AddNewGameModalContent';

interface AddNewGameModalProps extends AddNewGameModalContentProps {
  isOpen: boolean;
}

function AddNewGameModal({
  isOpen,
  onModalClose,
  ...otherProps
}: AddNewGameModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <AddNewGameModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default AddNewGameModal;
