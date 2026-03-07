import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import PlatformInteractiveSearchModalContent, {
  PlatformInteractiveSearchModalContentProps,
} from './PlatformInteractiveSearchModalContent';

interface PlatformInteractiveSearchModalProps
  extends PlatformInteractiveSearchModalContentProps {
  isOpen: boolean;
}

function PlatformInteractiveSearchModal(
  props: PlatformInteractiveSearchModalProps
) {
  const { isOpen, romCount, gameId, platformNumber, onModalClose } = props;

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.EXTRA_EXTRA_LARGE}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <PlatformInteractiveSearchModalContent
        romCount={romCount}
        gameId={gameId}
        platformNumber={platformNumber}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default PlatformInteractiveSearchModal;
