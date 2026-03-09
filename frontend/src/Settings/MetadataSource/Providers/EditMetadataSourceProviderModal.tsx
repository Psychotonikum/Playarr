import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditMetadataSourceProviderModalContent, {
  EditMetadataSourceProviderModalContentProps,
} from './EditMetadataSourceProviderModalContent';

interface EditMetadataSourceProviderModalProps
  extends EditMetadataSourceProviderModalContentProps {
  isOpen: boolean;
}

function EditMetadataSourceProviderModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditMetadataSourceProviderModalProps) {
  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EditMetadataSourceProviderModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditMetadataSourceProviderModal;
