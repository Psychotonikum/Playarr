import React, { SyntheticEvent, useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import styles from './GameIndexPosterSelect.css';

interface GameIndexPosterSelectProps {
  gameId: number;
  titleSlug: string;
}

function GameIndexPosterSelect({
  gameId,
  titleSlug,
}: GameIndexPosterSelectProps) {
  const { toggleSelected, useIsSelected } = useSelect();
  const isSelected = useIsSelected(gameId);

  const onSelectPress = useCallback(
    (event: SyntheticEvent<HTMLElement, PointerEvent>) => {
      if (event.nativeEvent.ctrlKey || event.nativeEvent.metaKey) {
        window.open(`${window.Playarr.urlBase}/game/${titleSlug}`, '_blank');
        return;
      }

      const shiftKey = event.nativeEvent.shiftKey;

      toggleSelected({
        id: gameId,
        isSelected: !isSelected,
        shiftKey,
      });
    },
    [gameId, titleSlug, isSelected, toggleSelected]
  );

  return (
    <Link className={styles.checkButton} onPress={onSelectPress}>
      <span className={styles.checkContainer}>
        <Icon
          className={isSelected ? styles.selected : styles.unselected}
          name={isSelected ? icons.CHECK_CIRCLE : icons.CIRCLE_OUTLINE}
          size={20}
        />
      </span>
    </Link>
  );
}

export default GameIndexPosterSelect;
