import { useMemo } from 'react';
import Provider from 'typings/Provider';
import {
  useProviderSettings,
  useManageProviderSettings,
  useDeleteProvider,
  useTestProvider,
} from 'Settings/useProviderSettings';
import {
  useProviderSchema,
  useSelectedSchema,
  SelectedSchema,
} from 'Settings/useProviderSchema';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';

export interface MetadataSourceProviderModel extends Provider {
  enableSearch: boolean;
  enableCalendar: boolean;
  downloadMetadata: boolean;
  supportsSearch: boolean;
  supportsCalendar: boolean;
  supportsMetadataDownload: boolean;
  tags: number[];
}

const PATH = '/metadatasourceprovider';

export const useMetadataSourceProviders = () => {
  return useProviderSettings<MetadataSourceProviderModel>({
    path: PATH,
  });
};

export const useSortedMetadataSourceProviders = () => {
  const result = useMetadataSourceProviders();

  const sortedData = useMemo(
    () => result.data.sort(sortByProp('name')),
    [result.data]
  );

  return {
    ...result,
    data: sortedData,
  };
};

export const useMetadataSourceProvider = (id: number | undefined) => {
  const { data } = useMetadataSourceProviders();

  if (id === undefined) {
    return undefined;
  }

  return data.find((p) => p.id === id);
};

export const useMetadataSourceProviderSchema = (enabled = true) => {
  return useProviderSchema<MetadataSourceProviderModel>(PATH, enabled);
};

export const useManageMetadataSourceProvider = (
  id: number | undefined,
  cloneId: number | undefined,
  selectedSchema?: SelectedSchema
) => {
  const schema = useSelectedSchema<MetadataSourceProviderModel>(
    PATH,
    selectedSchema
  );
  const cloneProvider = useMetadataSourceProvider(cloneId);

  const defaultProvider = useMemo(() => {
    if (cloneId && cloneProvider) {
      return {
        ...cloneProvider,
        id: 0,
        name: translate('DefaultNameCopiedProfile', {
          name: cloneProvider.name,
        }),
      };
    }

    if (selectedSchema && schema) {
      return {
        ...schema,
        name: schema.implementationName,
        enableSearch: schema.supportsSearch,
        enableCalendar: schema.supportsCalendar,
        downloadMetadata: false,
      };
    }

    return {} as MetadataSourceProviderModel;
  }, [cloneId, cloneProvider, schema, selectedSchema]);

  return useManageProviderSettings<MetadataSourceProviderModel>(
    id,
    defaultProvider,
    PATH
  );
};

export const useDeleteMetadataSourceProvider = (id: number) => {
  const result = useDeleteProvider<MetadataSourceProviderModel>(id, PATH);

  return {
    ...result,
    deleteProvider: result.deleteProvider,
  };
};

export const useTestMetadataSourceProvider = () => {
  return useTestProvider<MetadataSourceProviderModel>(PATH);
};
