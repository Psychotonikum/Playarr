import React from 'react';
import { AlternateTitle } from 'Game/Game';
import styles from './GameAlternateTitles.css';

interface GameAlternateTitlesProps {
  alternateTitles: AlternateTitle[];
}

function GameAlternateTitles({
  alternateTitles,
}: GameAlternateTitlesProps) {
  return (
    <ul>
      {alternateTitles.map((alternateTitle) => {
        return (
          <li key={alternateTitle.title} className={styles.alternateTitle}>
            {alternateTitle.title}
            {alternateTitle.comment ? (
              <span className={styles.comment}> {alternateTitle.comment}</span>
            ) : null}
          </li>
        );
      })}
    </ul>
  );
}

export default GameAlternateTitles;
