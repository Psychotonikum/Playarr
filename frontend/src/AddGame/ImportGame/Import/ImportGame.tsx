import React, { useCallback, useEffect, useMemo, useRef } from 'react';
import { useParams } from 'react-router';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import { icons, kinds } from 'Helpers/Props';
import useRootFolders, { useRootFolder } from 'RootFolder/useRootFolders';
import translate from 'Utilities/String/translate';
import ImportGameFooter from './ImportGameFooter';
import { clearImportGame, ImportGameItem, useLookupQueueHasItems } from './importGameStore';
import ImportGameTable from './ImportGameTable';
import { useImportGame } from './useImportGame';

function ImportGameContent({
  rootFoldersFetching,
  rootFoldersFetched,
  rootFoldersError,
  unmappedFolders,
  items,
  path,
  scrollerRef,
}: {
  rootFoldersFetching: boolean;
  rootFoldersFetched: boolean;
  rootFoldersError: unknown;
  unmappedFolders: { name: string; path: string; relativePath: string; id: string }[];
  items: { name: string; path: string; relativePath: string; id: string }[];
  path: string;
  scrollerRef: React.RefObject<HTMLDivElement>;
}) {
  const { selectedCount, getSelectedIds } = useSelect<ImportGameItem>();
  const isLookingUpSeries = useLookupQueueHasItems();
  const { importSeries, isImporting } = useImportGame();

  const handleImportPress = useCallback(() => {
    importSeries(getSelectedIds());
  }, [importSeries, getSelectedIds]);

  const showContent = !rootFoldersError && rootFoldersFetched && !!unmappedFolders.length;

  return (
    <PageContent title={translate('ImportGame')}>
      {showContent ? (
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('ImportCountSeries', { selectedCount })}
              iconName={icons.DOWNLOAD}
              isDisabled={!selectedCount || isLookingUpSeries}
              isSpinning={isImporting}
              onPress={handleImportPress}
            />
          </PageToolbarSection>
        </PageToolbar>
      ) : null}

      <PageContentBody ref={scrollerRef}>
        {rootFoldersFetching && !rootFoldersFetched ? (
          <LoadingIndicator />
        ) : null}

        {!rootFoldersFetching && !!rootFoldersError ? (
          <Alert kind={kinds.DANGER}>
            {translate('RootFoldersLoadError')}
          </Alert>
        ) : null}

        {!rootFoldersError &&
        !rootFoldersFetching &&
        rootFoldersFetched &&
        !unmappedFolders.length ? (
          <Alert kind={kinds.INFO}>
            {translate('AllSeriesInRootFolderHaveBeenImported', { path })}
          </Alert>
        ) : null}

        {showContent && scrollerRef.current ? (
          <ImportGameTable items={items} scrollerRef={scrollerRef} />
        ) : null}
      </PageContentBody>

      {showContent ? (
        <ImportGameFooter />
      ) : null}
    </PageContent>
  );
}

function ImportGame() {
  const { rootFolderId: rootFolderIdString } = useParams<{
    rootFolderId: string;
  }>();
  const rootFolderId = parseInt(rootFolderIdString);

  const {
    isFetching: rootFoldersFetching,
    isFetched: rootFoldersFetched,
    error: rootFoldersError,
    data: rootFolders,
  } = useRootFolders();

  useRootFolder(rootFolderId, false);

  const { path, unmappedFolders } = useMemo(() => {
    const rootFolder = rootFolders.find((r) => r.id === rootFolderId);

    return {
      path: rootFolder?.path ?? '',
      unmappedFolders:
        rootFolder?.unmappedFolders.map((unmappedFolders) => {
          return {
            ...unmappedFolders,
            id: unmappedFolders.name,
          };
        }) ?? [],
    };
  }, [rootFolders, rootFolderId]);

  const scrollerRef = useRef<HTMLDivElement>(null);

  const items = useMemo(() => {
    return unmappedFolders.map((unmappedFolder) => {
      return {
        ...unmappedFolder,
        id: unmappedFolder.name,
      };
    });
  }, [unmappedFolders]);

  useEffect(() => {
    return () => {
      clearImportGame();
    };
  }, [rootFolderId]);

  return (
    <SelectProvider items={items}>
      <ImportGameContent
        rootFoldersFetching={rootFoldersFetching}
        rootFoldersFetched={rootFoldersFetched}
        rootFoldersError={rootFoldersError}
        unmappedFolders={unmappedFolders}
        items={items}
        path={path}
        scrollerRef={scrollerRef}
      />
    </SelectProvider>
  );
}

export default ImportGame;
