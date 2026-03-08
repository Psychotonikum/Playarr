import React, { useCallback, useState } from 'react';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import useGameSystems from 'GameSystem/useGameSystems';
import EditGameSystemModal from './EditGameSystemModal';
import GameSystemCard from './GameSystemCard';
import styles from './GameSystems.css';

function GameSystems() {
  const { data: systems, isFetching, isFetched, error } = useGameSystems();
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);

  const handleAddPress = useCallback(() => {
    setIsAddModalOpen(true);
  }, []);

  const handleAddModalClose = useCallback(() => {
    setIsAddModalOpen(false);
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
            <GameSystemCard key={system.id} {...system} />
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

      <EditGameSystemModal
        isOpen={isAddModalOpen}
        onModalClose={handleAddModalClose}
      />
    </FieldSet>
  );
}

export default GameSystems;
