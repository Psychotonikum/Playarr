import React, { useCallback, useMemo, useState } from 'react';
import Alert from 'Components/Alert';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { icons, kinds, sizes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './ScraperImportPage.css';

interface ScraperImportFile {
  sourcePath: string;
  fileName: string;
  size: number;
  fileType: string;
}

interface ScraperImportItem {
  gameName: string;
  systemName: string;
  systemFolder: string;
  systemType: string;
  files: ScraperImportFile[];
}

interface ScraperImportRequest {
  gameName: string;
  systemFolder: string;
  igdbId: number;
  qualityProfileId: number;
  files: ScraperImportFile[];
}

interface ScraperImportResult {
  gameName: string;
  gameId: number;
  success: boolean;
  filesImported: number;
  error?: string;
}

function ScraperImportPage() {
  const [scanPath, setScanPath] = useState('');
  const [isFileBrowserOpen, setIsFileBrowserOpen] = useState(false);
  const [selectedItems, setSelectedItems] = useState<Set<string>>(new Set());
  const [isScanning, setIsScanning] = useState(false);
  const [importResults, setImportResults] = useState<
    ScraperImportResult[] | null
  >(null);

  const {
    data: scanResults,
    isLoading: isScanLoading,
    refetch: doScan,
  } = useApiQuery<ScraperImportItem[]>({
    path: '/game/scraperimport',
    queryParams: { path: scanPath },
    queryOptions: {
      enabled: false,
    },
  });

  const { mutate: doImport, isPending: isImporting } = useApiMutation<
    ScraperImportResult[],
    ScraperImportRequest[]
  >({
    path: '/game/scraperimport',
    method: 'POST',
    mutationOptions: {
      onSuccess: (data) => {
        setImportResults(data);
      },
    },
  });

  const items = scanResults ?? [];

  const handleFolderSelect = useCallback(
    ({ value }: InputChanged<string>) => {
      setScanPath(value);
      setIsFileBrowserOpen(false);
    },
    []
  );

  const handleScanPress = useCallback(() => {
    if (!scanPath) {
      return;
    }

    setIsScanning(true);
    setImportResults(null);
    setSelectedItems(new Set());
    doScan().finally(() => setIsScanning(false));
  }, [scanPath, doScan]);

  const handleToggleItem = useCallback(
    (gameName: string) => {
      setSelectedItems((prev) => {
        const next = new Set(prev);

        if (next.has(gameName)) {
          next.delete(gameName);
        } else {
          next.add(gameName);
        }

        return next;
      });
    },
    []
  );

  const handleSelectAll = useCallback(() => {
    if (selectedItems.size === items.length) {
      setSelectedItems(new Set());
    } else {
      setSelectedItems(new Set(items.map((i) => i.gameName)));
    }
  }, [items, selectedItems.size]);

  const handleImportPress = useCallback(() => {
    const requests: ScraperImportRequest[] = items
      .filter((item) => selectedItems.has(item.gameName))
      .map((item) => ({
        gameName: item.gameName,
        systemFolder: item.systemFolder,
        igdbId: 0,
        qualityProfileId: 1,
        files: item.files,
      }));

    if (requests.length > 0) {
      doImport(requests);
    }
  }, [items, selectedItems, doImport]);

  const totalSize = useMemo(() => {
    return items
      .filter((item) => selectedItems.has(item.gameName))
      .reduce(
        (acc, item) => acc + item.files.reduce((s, f) => s + f.size, 0),
        0
      );
  }, [items, selectedItems]);

  const allSelected = items.length > 0 && selectedItems.size === items.length;
  const hasResults = items.length > 0;
  const hasImportResults = importResults !== null;

  return (
    <PageContent title="Game Import">
      <PageContentBody>
        <div className={styles.header}>
          <h1>Game Import</h1>
          <p>
            Scan a directory organized by system folders (Batocera short names)
            and import discovered games into your library.
          </p>
        </div>

        <div className={styles.scanSection}>
          <div className={styles.pathInput}>
            <Button
              kind={kinds.PRIMARY}
              size={sizes.LARGE}
              onPress={() => setIsFileBrowserOpen(true)}
            >
              <Icon name={icons.FOLDER_OPEN} />
              {scanPath || ' Browse for folder...'}
            </Button>
          </div>

          <Button
            kind={kinds.SUCCESS}
            size={sizes.LARGE}
            isDisabled={!scanPath || isScanning || isScanLoading}
            onPress={handleScanPress}
          >
            <Icon
              name={isScanning ? icons.SPINNER : icons.SEARCH}
              isSpinning={isScanning}
            />
            {' Scan'}
          </Button>
        </div>

        <FileBrowserModal
          isOpen={isFileBrowserOpen}
          name="scraperPath"
          value={scanPath}
          onChange={handleFolderSelect}
          onModalClose={() => setIsFileBrowserOpen(false)}
        />

        {(isScanning || isScanLoading) && <LoadingIndicator />}

        {!isScanning && !isScanLoading && hasResults && !hasImportResults && (
          <div>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th className={styles.headerCell}>
                    <input
                      type="checkbox"
                      checked={allSelected}
                      onChange={handleSelectAll}
                    />
                  </th>
                  <th className={styles.headerCell}>Game</th>
                  <th className={styles.headerCell}>System</th>
                  <th className={styles.headerCell}>Type</th>
                  <th className={styles.headerCell}>Files</th>
                  <th className={styles.headerCell}>Size</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => {
                  const itemSize = item.files.reduce(
                    (acc, f) => acc + f.size,
                    0
                  );
                  const isSelected = selectedItems.has(item.gameName);

                  return (
                    <tr
                      key={`${item.systemFolder}-${item.gameName}`}
                      className={styles.row}
                    >
                      <td className={styles.checkboxCell}>
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => handleToggleItem(item.gameName)}
                        />
                      </td>
                      <td className={styles.nameCell}>{item.gameName}</td>
                      <td className={styles.systemCell}>
                        <span className={styles.systemBadge}>
                          {item.systemName}
                        </span>
                      </td>
                      <td className={styles.typeCell}>{item.systemType}</td>
                      <td className={styles.filesCell}>
                        {item.files.length}
                      </td>
                      <td className={styles.sizeCell}>
                        {formatBytes(itemSize)}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>

            <div className={styles.footer}>
              <div className={styles.footerLeft}>
                <span className={styles.selectedCount}>
                  {selectedItems.size} of {items.length} selected
                </span>
                {selectedItems.size > 0 && (
                  <span>({formatBytes(totalSize)})</span>
                )}
              </div>

              <Button
                kind={kinds.SUCCESS}
                size={sizes.LARGE}
                isDisabled={selectedItems.size === 0 || isImporting}
                onPress={handleImportPress}
              >
                {isImporting ? (
                  <Icon
                    className={styles.importingSpinner}
                    name={icons.SPINNER}
                    isSpinning={true}
                  />
                ) : null}
                {isImporting
                  ? ` Importing ${selectedItems.size} games...`
                  : ` Import ${selectedItems.size} games`}
              </Button>
            </div>
          </div>
        )}

        {!isScanning &&
          !isScanLoading &&
          !hasResults &&
          scanPath &&
          !isScanning && (
            <Alert kind={kinds.INFO}>
              {translate('NoGamesFoundInPath')}
            </Alert>
          )}

        {hasImportResults && (
          <div className={styles.resultsSection}>
            <h2>Import Results</h2>
            {importResults.map((result) => (
              <div
                key={result.gameName}
                className={
                  result.success ? styles.resultSuccess : styles.resultError
                }
              >
                <Icon
                  name={result.success ? icons.CHECK : icons.DANGER}
                />
                {` ${result.gameName}`}
                {result.success
                  ? ` - ${result.filesImported} files imported`
                  : ` - Error: ${result.error}`}
              </div>
            ))}
          </div>
        )}
      </PageContentBody>
    </PageContent>
  );
}

export default ScraperImportPage;
