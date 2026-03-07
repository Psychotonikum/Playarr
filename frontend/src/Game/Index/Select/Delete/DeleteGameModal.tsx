import React from 'react';
import Modal from 'Components/Modal/Modal';
import DeleteGameModalContent, {
  DeleteGameModalContentProps,
} from './DeleteGameModalContent';

interface DeleteGameModalProps extends DeleteGameModalContentProps {
  isOpen: boolean;
}

function DeleteGameModal(props: DeleteGameModalProps) {
  const { isOpen, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <DeleteGameModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default DeleteGameModal;
