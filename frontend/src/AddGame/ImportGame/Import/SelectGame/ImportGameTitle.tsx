import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ImportGameTitle.css';

interface ImportGameTitleProps {
  title: string;
  year: number;
  network?: string;
  isExistingSeries: boolean;
}

function ImportGameTitle({
  title,
  year,
  network,
  isExistingSeries,
}: ImportGameTitleProps) {
  return (
    <div className={styles.titleContainer}>
      <div className={styles.title}>{title}</div>

      {year > 0 && !title.includes(String(year)) ? (
        <span className={styles.year}>({year})</span>
      ) : null}

      {network ? <Label>{network}</Label> : null}

      {isExistingSeries ? (
        <Label kind={kinds.WARNING}>{translate('Existing')}</Label>
      ) : null}
    </div>
  );
}

export default ImportGameTitle;
