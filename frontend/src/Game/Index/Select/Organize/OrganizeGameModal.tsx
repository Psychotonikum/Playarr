import React from 'react';
import Modal from 'Components/Modal/Modal';
import OrganizeGameModalContent, {
  OrganizeGameModalContentProps,
} from './OrganizeGameModalContent';

interface OrganizeGameModalProps extends OrganizeGameModalContentProps {
  isOpen: boolean;
}

function OrganizeGameModal(props: OrganizeGameModalProps) {
  const { isOpen, onModalClose, ...otherProps } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <OrganizeGameModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default OrganizeGameModal;
