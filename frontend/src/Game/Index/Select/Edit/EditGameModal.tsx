import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditGameModalContent, {
  EditGameModalContentProps,
} from './EditGameModalContent';

interface EditGameModalProps extends EditGameModalContentProps {
  isOpen: boolean;
}

function EditGameModal({
  isOpen,
  onSavePress,
  onModalClose,
}: EditGameModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <EditGameModalContent
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditGameModal;
