import React, { useCallback, useState } from 'react';
import Link from 'Components/Link/Link';
import RomDetailsModal from 'Rom/RomDetailsModal';
import { RomEntity } from 'Rom/useRom';
import FinaleType from './FinaleType';
import styles from './RomTitleLink.css';

interface RomTitleLinkProps {
  romId: number;
  gameId: number;
  romEntity: RomEntity;
  romTitle: string;
  finaleType?: string;
  showOpenSeriesButton: boolean;
}

function RomTitleLink(props: RomTitleLinkProps) {
  const { romTitle, finaleType, ...otherProps } = props;
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const handleLinkPress = useCallback(() => {
    setIsDetailsModalOpen(true);
  }, [setIsDetailsModalOpen]);
  const handleModalClose = useCallback(() => {
    setIsDetailsModalOpen(false);
  }, [setIsDetailsModalOpen]);

  return (
    <div className={styles.container}>
      <Link className={styles.link} onPress={handleLinkPress}>
        {romTitle}
      </Link>

      {finaleType ? <FinaleType finaleType={finaleType} /> : null}

      <RomDetailsModal
        isOpen={isDetailsModalOpen}
        romTitle={romTitle}
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </div>
  );
}

export default RomTitleLink;
