import { useManageSettings, useSettings } from 'Settings/useSettings';

export interface MetadataSourceSettingsModel {
  twitchClientId: string;
  twitchClientSecret: string;
  ratingSource: string;
}

const PATH = '/settings/metadatasource';

export const useMetadataSourceSettings = () => {
  return useSettings<MetadataSourceSettingsModel>(PATH);
};

export const useManageMetadataSourceSettings = () => {
  return useManageSettings<MetadataSourceSettingsModel>(PATH);
};
