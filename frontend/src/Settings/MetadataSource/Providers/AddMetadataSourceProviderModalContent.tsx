import React, { useCallback } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import { useMetadataSourceProviderSchema } from '../useMetadataSourceProviders';
import styles from './AddMetadataSourceProviderModalContent.css';

interface AddMetadataSourceProviderModalContentProps {
  onProviderSelect: (selectedSchema: SelectedSchema) => void;
  onModalClose: () => void;
}

function AddMetadataSourceProviderModalContent({
  onProviderSelect,
  onModalClose,
}: AddMetadataSourceProviderModalContentProps) {
  const { isSchemaFetching, isSchemaFetched, schemaError, schema } =
    useMetadataSourceProviderSchema();

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>Add Metadata Source</ModalHeader>

      <ModalBody>
        {isSchemaFetching ? <LoadingIndicator /> : null}

        {!isSchemaFetching && !!schemaError ? (
          <Alert kind={kinds.DANGER}>
            Unable to load metadata source providers
          </Alert>
        ) : null}

        {isSchemaFetched && !schemaError ? (
          <div>
            <Alert kind={kinds.INFO}>
              Add metadata sources to enrich your game library with data from
              different providers. Each provider can be enabled for search,
              calendar, and metadata download independently.
            </Alert>

            <FieldSet legend="Available Sources">
              <div className={styles.providers}>
                {schema.map((item) => {
                  return (
                    <ProviderItem
                      key={item.implementation}
                      implementation={item.implementation}
                      implementationName={item.implementationName}
                      onProviderSelect={onProviderSelect}
                    />
                  );
                })}
              </div>
            </FieldSet>
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

interface ProviderItemProps {
  implementation: string;
  implementationName: string;
  onProviderSelect: (schema: SelectedSchema) => void;
}

function ProviderItem({
  implementation,
  implementationName,
  onProviderSelect,
}: ProviderItemProps) {
  const handlePress = useCallback(() => {
    onProviderSelect({ implementation, implementationName });
  }, [implementation, implementationName, onProviderSelect]);

  return (
    <div className={styles.provider}>
      <Link className={styles.underlay} onPress={handlePress} />
      <div className={styles.overlay}>
        <div className={styles.name}>{implementationName}</div>
      </div>
    </div>
  );
}

export default AddMetadataSourceProviderModalContent;
