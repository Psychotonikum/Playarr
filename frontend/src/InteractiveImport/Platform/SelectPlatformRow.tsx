import React, { useCallback } from 'react';
import Link from 'Components/Link/Link';
import translate from 'Utilities/String/translate';
import styles from './SelectPlatformRow.css';

interface SelectPlatformRowProps {
  platformNumber: number;
  onSeasonSelect(platform: number): unknown;
}

function SelectPlatformRow(props: SelectPlatformRowProps) {
  const { platformNumber, onSeasonSelect } = props;

  const onSeasonSelectWrapper = useCallback(() => {
    onSeasonSelect(platformNumber);
  }, [platformNumber, onSeasonSelect]);

  return (
    <Link
      className={styles.platform}
      component="div"
      onPress={onSeasonSelectWrapper}
    >
      {platformNumber === 0
        ? translate('Specials')
        : translate('PlatformNumberToken', { platformNumber })}
    </Link>
  );
}

export default SelectPlatformRow;
