import ModelBase from 'App/ModelBase';
import useApiQuery from 'Helpers/Hooks/useApiQuery';

export interface OrganizePreviewModel extends ModelBase {
  gameId: number;
  platformNumber: number;
  romNumbers: number[];
  romFileId: number;
  existingPath: string;
  newPath: string;
}

const DEFAULT_ORGANIZE_PREVIEW: OrganizePreviewModel[] = [];

const useOrganizePreview = (gameId: number, platformNumber?: number) => {
  const queryParams: { gameId: number; platformNumber?: number } = { gameId };

  if (platformNumber != null) {
    queryParams.platformNumber = platformNumber;
  }

  const { data, ...result } = useApiQuery<OrganizePreviewModel[]>({
    path: '/rename',
    queryParams,
  });

  return {
    items: data ?? DEFAULT_ORGANIZE_PREVIEW,
    ...result,
  };
};

export default useOrganizePreview;
