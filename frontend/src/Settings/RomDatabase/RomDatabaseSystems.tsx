import React from 'react';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import RomDatabaseSystemCard from './RomDatabaseSystemCard';
import { useRomDatabaseSystems } from './useRomDatabase';
import styles from './RomDatabaseSystems.css';

function RomDatabaseSystems() {
  const { data: systems, isLoading, error } = useRomDatabaseSystems();

  if (isLoading) {
    return <LoadingIndicator />;
  }

  if (error) {
    return <div>Failed to load ROM database systems</div>;
  }

  return (
    <FieldSet legend="ROM Verification Databases">
      <div className={styles.systems}>
        {systems.map((system) => (
          <RomDatabaseSystemCard key={system.id} system={system} />
        ))}
      </div>
    </FieldSet>
  );
}

export default RomDatabaseSystems;
