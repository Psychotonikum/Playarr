import React, { useMemo } from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { Platform } from 'Game/Game';
import { useSingleGame } from 'Game/useGame';
import translate from 'Utilities/String/translate';
import SelectPlatformRow from './SelectPlatformRow';

interface SelectPlatformModalContentProps {
  gameId?: number;
  modalTitle: string;
  onSeasonSelect(platformNumber: number): void;
  onModalClose(): void;
}

function SelectPlatformModalContent(props: SelectPlatformModalContentProps) {
  const { gameId, modalTitle, onSeasonSelect, onModalClose } = props;
  const game = useSingleGame(gameId);
  const platforms = useMemo<Platform[]>(() => {
    return game?.platforms.slice(0).reverse() || [];
  }, [game]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SelectPlatformModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody>
        {platforms.map((item) => {
          return (
            <SelectPlatformRow
              key={item.platformNumber}
              platformNumber={item.platformNumber}
              onSeasonSelect={onSeasonSelect}
            />
          );
        })}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectPlatformModalContent;
