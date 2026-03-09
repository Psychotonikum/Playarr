import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import AddMetadataSourceProviderModalContent from './AddMetadataSourceProviderModalContent';

interface AddMetadataSourceProviderModalProps {
  isOpen: boolean;
  onProviderSelect: (schema: SelectedSchema) => void;
  onModalClose: () => void;
}

function AddMetadataSourceProviderModal({
  isOpen,
  onProviderSelect,
  onModalClose,
}: AddMetadataSourceProviderModalProps) {
  return (
    <Modal size={sizes.LARGE} isOpen={isOpen} onModalClose={onModalClose}>
      <AddMetadataSourceProviderModalContent
        onProviderSelect={onProviderSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default AddMetadataSourceProviderModal;
