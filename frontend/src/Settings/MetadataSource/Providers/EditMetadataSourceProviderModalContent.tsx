import React, { useCallback, useEffect } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import { EnhancedSelectInputChanged, InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { useManageMetadataSourceProvider } from '../useMetadataSourceProviders';
import styles from './EditMetadataSourceProviderModalContent.css';

export interface EditMetadataSourceProviderModalContentProps {
  id?: number;
  cloneId?: number;
  selectedSchema?: SelectedSchema;
  onModalClose: () => void;
  onDeleteMetadataSourceProviderPress?: () => void;
}

function EditMetadataSourceProviderModalContent({
  id,
  cloneId,
  selectedSchema,
  onModalClose,
  onDeleteMetadataSourceProviderPress,
}: EditMetadataSourceProviderModalContentProps) {
  const {
    item,
    updateFieldValue,
    updateValue,
    saveProvider,
    isSaving,
    saveError,
    testProvider,
    isTesting,
    validationErrors,
    validationWarnings,
  } = useManageMetadataSourceProvider(id, cloneId, selectedSchema);

  const wasSaving = usePrevious(isSaving);

  const {
    implementationName = '',
    name,
    enableSearch,
    enableCalendar,
    downloadMetadata,
    supportsSearch,
    supportsCalendar,
    supportsMetadataDownload,
    tags,
    fields,
  } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - InputChanged is not typed correctly
      updateValue(change.name, change.value);
    },
    [updateValue]
  );

  const handleFieldChange = useCallback(
    ({
      name,
      value,
      additionalProperties,
    }: EnhancedSelectInputChanged<unknown>) => {
      updateFieldValue({ [name]: value, ...additionalProperties });
    },
    [updateFieldValue]
  );

  const handleSavePress = useCallback(() => {
    saveProvider();
  }, [saveProvider]);

  const handleTestPress = useCallback(() => {
    testProvider();
  }, [testProvider]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? `Edit - ${implementationName}` : `Add - ${implementationName}`}
      </ModalHeader>

      <ModalBody>
        <Form
          validationErrors={validationErrors}
          validationWarnings={validationWarnings}
        >
          <FormGroup>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Enable Search</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enableSearch"
              helpText={
                supportsSearch?.value
                  ? 'Use this provider when searching for games'
                  : 'Search is not supported by this provider'
              }
              isDisabled={!supportsSearch?.value}
              {...enableSearch}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Enable Calendar</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enableCalendar"
              helpText={
                supportsCalendar?.value
                  ? 'Use this provider for calendar/upcoming game data'
                  : 'Calendar is not supported by this provider'
              }
              isDisabled={!supportsCalendar?.value}
              {...enableCalendar}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Download Metadata</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="downloadMetadata"
              helpText={
                supportsMetadataDownload?.value
                  ? 'Download metadata (images, descriptions) locally for offline access'
                  : 'Metadata download is not supported by this provider'
              }
              isDisabled={!supportsMetadataDownload?.value}
              {...downloadMetadata}
              onChange={handleInputChange}
            />
          </FormGroup>

          {fields?.map((field) => {
            return (
              <ProviderFieldFormGroup
                key={field.name}
                advancedSettings={false}
                provider="metadataSourceProvider"
                providerData={item}
                {...field}
                onChange={handleFieldChange}
              />
            );
          })}

          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              helpText="Tags to associate with this metadata source"
              {...tags}
              onChange={handleInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteMetadataSourceProviderPress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={handleTestPress}
        >
          {translate('Test')}
        </SpinnerErrorButton>

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditMetadataSourceProviderModalContent;
