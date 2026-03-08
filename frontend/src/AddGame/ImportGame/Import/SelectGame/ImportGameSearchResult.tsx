import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import useExistingGame from 'Game/useExistingGame';
import ImportGameTitle from './ImportGameTitle';
import styles from './ImportGameSearchResult.css';

interface ImportGameSearchResultProps {
  igdbId: number;
  title: string;
  year: number;
  network?: string;
  onPress: (igdbId: number) => void;
}

function ImportGameSearchResult({
  igdbId,
  title,
  year,
  network,
  onPress,
}: ImportGameSearchResultProps) {
  const isExistingSeries = useExistingGame(igdbId);

  const handlePress = useCallback(() => {
    onPress(igdbId);
  }, [igdbId, onPress]);

  return (
    <div className={styles.container}>
      <Link className={styles.game} onPress={handlePress}>
        <ImportGameTitle
          title={title}
          year={year}
          network={network}
          isExistingSeries={isExistingSeries}
        />
      </Link>

      <Link
        className={styles.igdbLink}
        to={`https://www.theigdb.com/?tab=game&id=${igdbId}`}
      >
        <Icon
          className={styles.igdbLinkIcon}
          name={icons.EXTERNAL_LINK}
          size={16}
        />
      </Link>
    </div>
  );
}

export default ImportGameSearchResult;
