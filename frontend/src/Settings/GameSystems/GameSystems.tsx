import React, { useCallback, useState } from 'react';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Modal from 'Components/Modal/Modal';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons, sizes } from 'Helpers/Props';
import useGameSystems from 'GameSystem/useGameSystems';
import AddGameSystemModalContent from './AddGameSystemModalContent';
import EditGameSystemModal from './EditGameSystemModal';
import { GameSystemPreset } from './gameSystemPresets';
import GameSystemCard from './GameSystemCard';
import styles from './GameSystems.css';

function GameSystems() {
  const { data: systems, isFetching, isFetched, error } = useGameSystems();
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [cloneId, setCloneId] = useState<number | undefined>(undefined);
  const [selectedPreset, setSelectedPreset] = useState<
    GameSystemPreset | null | undefined
  >(undefined);

  const handleAddPress = useCallback(() => {
    setCloneId(undefined);
    setSelectedPreset(undefined);
    setIsAddModalOpen(true);
  }, []);

  const handleClonePress = useCallback((id: number) => {
    setCloneId(id);
    setSelectedPreset(undefined);
    setIsEditModalOpen(true);
  }, []);

  const handlePresetSelect = useCallback(
    (preset: GameSystemPreset | null) => {
      setSelectedPreset(preset);
      setIsAddModalOpen(false);
      setIsEditModalOpen(true);
    },
    []
  );

  const handleAddModalClose = useCallback(() => {
    setIsAddModalOpen(false);
  }, []);

  const handleEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
    setCloneId(undefined);
    setSelectedPreset(undefined);
  }, []);

  return (
    <FieldSet legend="Game Systems">
      <PageSectionContent
        errorMessage="Unable to load game systems"
        error={error}
        isFetching={isFetching}
        isPopulated={isFetched}
      >
        <div className={styles.systems}>
          {systems.map((system) => (
            <GameSystemCard
              key={system.id}
              {...system}
              onClonePress={handleClonePress}
            />
          ))}

          <div
            className={styles.addSystem}
            onClick={handleAddPress}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </div>
        </div>
      </PageSectionContent>

      <Modal
        size={sizes.LARGE}
        isOpen={isAddModalOpen}
        onModalClose={handleAddModalClose}
      >
        <AddGameSystemModalContent
          onPresetSelect={handlePresetSelect}
          onModalClose={handleAddModalClose}
        />
      </Modal>

      <EditGameSystemModal
        cloneId={cloneId}
        preset={selectedPreset}
        isOpen={isEditModalOpen}
        onModalClose={handleEditModalClose}
      />
    </FieldSet>
  );
}

export default GameSystems;
