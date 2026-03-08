import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditGameSystemModalContent from './EditGameSystemModalContent';

interface EditGameSystemModalProps {
  id?: number;
  isOpen: boolean;
  onModalClose: () => void;
}

function EditGameSystemModal({
  id,
  isOpen,
  onModalClose,
}: EditGameSystemModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <EditGameSystemModalContent id={id} onModalClose={onModalClose} />
    </Modal>
  );
}

export default EditGameSystemModal;
