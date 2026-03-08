import React, { useCallback } from 'react';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { inputTypes } from 'Helpers/Props';
import SettingsToolbar from 'Settings/SettingsToolbar';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import {
  MetadataSourceSettingsModel,
  useManageMetadataSourceSettings,
} from './useMetadataSourceSettings';

function MetadataSourceSettings() {
  const {
    isFetching,
    isFetched: isPopulated,
    isSaving,
    hasPendingChanges,
    settings,
    saveSettings: saveMetadataSourceSettings,
    updateSetting,
  } = useManageMetadataSourceSettings();

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      updateSetting(
        change.name as keyof MetadataSourceSettingsModel,
        change.value as MetadataSourceSettingsModel[keyof MetadataSourceSettingsModel]
      );
    },
    [updateSetting]
  );

  const handleSavePress = useCallback(() => {
    saveMetadataSourceSettings();
  }, [saveMetadataSourceSettings]);

  return (
    <PageContent title={translate('MetadataSourceSettings')}>
      <SettingsToolbar
        isSaving={isSaving}
        hasPendingChanges={hasPendingChanges}
        onSavePress={handleSavePress}
      />

      <PageContentBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && isPopulated ? (
          <Form>
            <FieldSet legend="IGDB / Twitch API">
              <FormGroup>
                <FormLabel>Twitch Client ID</FormLabel>
                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="twitchClientId"
                  helpText="Client ID from your Twitch Developer Application (required for IGDB game search)"
                  helpLink="https://dev.twitch.tv/console/apps"
                  onChange={handleInputChange}
                  {...settings.twitchClientId}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Twitch Client Secret</FormLabel>
                <FormInputGroup
                  type={inputTypes.PASSWORD}
                  name="twitchClientSecret"
                  helpText="Client Secret from your Twitch Developer Application"
                  onChange={handleInputChange}
                  {...settings.twitchClientSecret}
                />
              </FormGroup>
            </FieldSet>

            <FieldSet legend="Rating Source">
              <FormGroup>
                <FormLabel>Score Source</FormLabel>
                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="ratingSource"
                  values={[
                    { key: 'igdb', value: 'IGDB' },
                    { key: 'metacritic', value: 'Metacritic' },
                  ]}
                  helpText="Choose which service provides game ratings/scores"
                  onChange={handleInputChange}
                  {...settings.ratingSource}
                />
              </FormGroup>
            </FieldSet>
          </Form>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default MetadataSourceSettings;
