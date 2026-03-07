import React from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { scrollDirections } from 'Helpers/Props';
import InteractiveSearch from 'InteractiveSearch/InteractiveSearch';
import formatPlatform from 'Platform/formatPlatform';
import translate from 'Utilities/String/translate';
import styles from './PlatformInteractiveSearchModalContent.css';

export interface PlatformInteractiveSearchModalContentProps {
  romCount: number;
  gameId: number;
  platformNumber: number;
  onModalClose(): void;
}

function PlatformInteractiveSearchModalContent({
  romCount,
  gameId,
  platformNumber,
  onModalClose,
}: PlatformInteractiveSearchModalContentProps) {
  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {platformNumber === null
          ? translate('InteractiveSearchModalHeader')
          : translate('InteractiveSearchModalHeaderSeason', {
              platform: formatPlatform(platformNumber) as string,
            })}
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearch
          type="platform"
          searchPayload={{
            gameId,
            platformNumber,
          }}
        />
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div>
          {translate('EpisodesInSeason', {
            romCount,
          })}
        </div>

        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default PlatformInteractiveSearchModalContent;
