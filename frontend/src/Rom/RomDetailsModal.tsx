import React, { useCallback, useState } from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import RomDetailsTab from 'Rom/RomDetailsTab';
import { RomEntity } from 'Rom/useRom';
import RomDetailsModalContent from './RomDetailsModalContent';

interface RomDetailsModalProps {
  isOpen: boolean;
  romId: number;
  romEntity: RomEntity;
  gameId: number;
  romTitle: string;
  isSaving?: boolean;
  showOpenSeriesButton?: boolean;
  selectedTab?: RomDetailsTab;
  startInteractiveSearch?: boolean;
  onModalClose(): void;
}

function RomDetailsModal(props: RomDetailsModalProps) {
  const { selectedTab, isOpen, onModalClose, ...otherProps } = props;

  const [closeOnBackgroundClick, setCloseOnBackgroundClick] = useState(
    selectedTab !== 'search'
  );

  const handleTabChange = useCallback(
    (isSearch: boolean) => {
      setCloseOnBackgroundClick(!isSearch);
    },
    [setCloseOnBackgroundClick]
  );

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.EXTRA_EXTRA_LARGE}
      closeOnBackgroundClick={closeOnBackgroundClick}
      onModalClose={onModalClose}
    >
      <RomDetailsModalContent
        {...otherProps}
        selectedTab={selectedTab}
        onTabChange={handleTabChange}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default RomDetailsModal;
