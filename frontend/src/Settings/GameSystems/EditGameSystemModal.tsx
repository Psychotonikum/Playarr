import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditGameSystemModalContent from './EditGameSystemModalContent';

interface EditGameSystemModalProps {
  id?: number;
  cloneId?: number;
  isOpen: boolean;
  onModalClose: () => void;
  onDeletePress?: () => void;
}

function EditGameSystemModal({
  id,
  cloneId,
  isOpen,
  onModalClose,
  onDeletePress,
}: EditGameSystemModalProps) {
  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EditGameSystemModalContent
        id={id}
        cloneId={cloneId}
        onModalClose={onModalClose}
        onDeletePress={onDeletePress}
      />
    </Modal>
  );
}

export default EditGameSystemModal;
