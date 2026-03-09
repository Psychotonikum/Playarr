import React, { useEffect, useMemo, useRef } from 'react';
import { useParams } from 'react-router';
import { SelectProvider } from 'App/Select/SelectContext';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { kinds } from 'Helpers/Props';
import useRootFolders, { useRootFolder } from 'RootFolder/useRootFolders';
import translate from 'Utilities/String/translate';
import ImportGameFooter from './ImportGameFooter';
import { clearImportGame } from './importGameStore';
import ImportGameTable from './ImportGameTable';

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
      <PageContent title={translate('ImportGame')}>
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

          {!rootFoldersError &&
          rootFoldersFetched &&
          !!unmappedFolders.length &&
          scrollerRef.current ? (
            <ImportGameTable items={items} scrollerRef={scrollerRef} />
          ) : null}
        </PageContentBody>

        {!rootFoldersError && rootFoldersFetched && !!unmappedFolders.length ? (
          <ImportGameFooter />
        ) : null}
      </PageContent>
    </SelectProvider>
  );
}

export default ImportGame;
