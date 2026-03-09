import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import { useSortedMetadataSourceProviders } from '../useMetadataSourceProviders';
import AddMetadataSourceProviderModal from './AddMetadataSourceProviderModal';
import EditMetadataSourceProviderModal from './EditMetadataSourceProviderModal';
import MetadataSourceProviderCard from './MetadataSourceProviderCard';
import styles from './MetadataSourceProviders.css';

function MetadataSourceProviders() {
  const { isFetching, isFetched, data, error } =
    useSortedMetadataSourceProviders();

  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [cloneId, setCloneId] = useState<number | null>(null);
  const [selectedSchema, setSelectedSchema] = useState<
    SelectedSchema | undefined
  >(undefined);

  const handleAddPress = useCallback(() => {
    setCloneId(null);
    setIsAddModalOpen(true);
  }, []);

  const handleClonePress = useCallback((id: number) => {
    setCloneId(id);
    setIsEditModalOpen(true);
  }, []);

  const handleProviderSelect = useCallback((selected: SelectedSchema) => {
    setSelectedSchema(selected);
    setIsAddModalOpen(false);
    setIsEditModalOpen(true);
  }, []);

  const handleAddModalClose = useCallback(() => {
    setIsAddModalOpen(false);
  }, []);

  const handleEditModalClose = useCallback(() => {
    setCloneId(null);
    setIsEditModalOpen(false);
  }, []);

  return (
    <FieldSet legend="Metadata Sources">
      <PageSectionContent
        errorMessage="Unable to load metadata sources"
        error={error}
        isFetching={isFetching}
        isPopulated={isFetched}
      >
        <div className={styles.providers}>
          {data.map((item) => {
            return (
              <MetadataSourceProviderCard
                key={item.id}
                {...item}
                onClonePress={handleClonePress}
              />
            );
          })}

          <Card
            className={styles.addProvider}
            onPress={handleAddPress}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
        </div>

        <AddMetadataSourceProviderModal
          isOpen={isAddModalOpen}
          onProviderSelect={handleProviderSelect}
          onModalClose={handleAddModalClose}
        />

        <EditMetadataSourceProviderModal
          isOpen={isEditModalOpen}
          cloneId={cloneId ?? undefined}
          selectedSchema={selectedSchema}
          onModalClose={handleEditModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default MetadataSourceProviders;
