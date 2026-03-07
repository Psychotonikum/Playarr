import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './PlatformInfo.css';

interface PlatformInfoProps {
  totalRomCount: number;
  monitoredRomCount: number;
  romFileCount: number;
  sizeOnDisk: number;
}

function PlatformInfo({
  totalRomCount,
  monitoredRomCount,
  romFileCount,
  sizeOnDisk,
}: PlatformInfoProps) {
  return (
    <DescriptionList>
      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('Total')}
        data={totalRomCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('Monitored')}
        data={monitoredRomCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('WithFiles')}
        data={romFileCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('SizeOnDisk')}
        data={formatBytes(sizeOnDisk)}
      />
    </DescriptionList>
  );
}

export default PlatformInfo;
