import React, { useCallback, useMemo } from 'react';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import GAME_SYSTEM_PRESETS, { GameSystemPreset } from './gameSystemPresets';
import styles from './AddGameSystemModalContent.css';

interface AddGameSystemModalContentProps {
  onPresetSelect: (preset: GameSystemPreset | null) => void;
  onModalClose: () => void;
}

function AddGameSystemModalContent({
  onPresetSelect,
  onModalClose,
}: AddGameSystemModalContentProps) {
  const groups = useMemo(() => {
    const grouped = new Map<string, GameSystemPreset[]>();

    for (const preset of GAME_SYSTEM_PRESETS) {
      const existing = grouped.get(preset.generation) ?? [];
      existing.push(preset);
      grouped.set(preset.generation, existing);
    }

    return grouped;
  }, []);

  const handleCustomClick = useCallback(() => {
    onPresetSelect(null);
  }, [onPresetSelect]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>Add Game System</ModalHeader>

      <ModalBody>
        <FieldSet legend="Custom">
          <div className={styles.presetGrid}>
            <div className={styles.customCard} onClick={handleCustomClick}>
              <div className={styles.presetName}>Custom System</div>
              <div className={styles.presetFolder}>Configure from scratch</div>
            </div>
          </div>
        </FieldSet>

        {Array.from(groups.entries()).map(([generation, presets]) => (
          <FieldSet key={generation} legend={generation}>
            <div className={styles.presetGrid}>
              {presets.map((preset) => (
                <div
                  key={preset.folderName}
                  className={styles.presetCard}
                  onClick={() => onPresetSelect(preset)}
                >
                  <div className={styles.presetName}>{preset.name}</div>
                  <div className={styles.presetFolder}>
                    /{preset.folderName}
                  </div>
                  <div className={styles.presetType}>
                    {preset.systemType === 1 ? 'Patchable' : 'Classic'}
                  </div>
                </div>
              ))}
            </div>
          </FieldSet>
        ))}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>Close</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddGameSystemModalContent;
