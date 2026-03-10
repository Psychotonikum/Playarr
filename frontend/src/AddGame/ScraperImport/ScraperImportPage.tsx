import React, { useCallback, useMemo, useRef, useState } from 'react';
import Alert from 'Components/Alert';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { icons, kinds, sizes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import getQueryPath from 'Utilities/Fetch/getQueryPath';
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

interface IgdbMatch {
  igdbId: number;
  title: string;
  year: number;
  remotePoster?: string;
  overview?: string;
}

interface LookupResult {
  igdbId: number;
  title: string;
  year: number;
  remotePoster?: string;
  overview?: string;
}

function ScraperImportPage() {
  const [scanPath, setScanPath] = useState('');
  const [isFileBrowserOpen, setIsFileBrowserOpen] = useState(false);
  const [selectedItems, setSelectedItems] = useState<Set<string>>(new Set());
  const [isScanning, setIsScanning] = useState(false);
  const [importResults, setImportResults] = useState<
    ScraperImportResult[] | null
  >(null);

  // IGDB matching state
  const [igdbMatches, setIgdbMatches] = useState<Record<string, IgdbMatch>>({});
  const [identifyingItem, setIdentifyingItem] = useState<string | null>(null);
  const [identifySearchTerm, setIdentifySearchTerm] = useState('');
  const [identifyResults, setIdentifyResults] = useState<LookupResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [isAutoMatching, setIsAutoMatching] = useState(false);
  const [autoMatchProgress, setAutoMatchProgress] = useState(0);
  const autoMatchCancelRef = useRef(false);

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
    setIgdbMatches({});
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
        gameName: igdbMatches[item.gameName]?.title ?? item.gameName,
        systemFolder: item.systemFolder,
        igdbId: igdbMatches[item.gameName]?.igdbId ?? 0,
        qualityProfileId: 1,
        files: item.files,
      }));

    if (requests.length > 0) {
      doImport(requests);
    }
  }, [items, selectedItems, igdbMatches, doImport]);

  // IGDB identify functions
  const searchIgdb = useCallback(async (term: string): Promise<LookupResult[]> => {
    const url = `${getQueryPath('/game/lookup')}?term=${encodeURIComponent(term)}`;
    const response = await fetch(url, {
      headers: {
        'X-Api-Key': window.Playarr.apiKey,
      },
    });

    if (!response.ok) {
      return [];
    }

    const data = await response.json();

    return (data as Array<Record<string, unknown>>).map((g) => ({
      igdbId: g.igdbId as number,
      title: g.title as string,
      year: g.year as number,
      remotePoster: g.remotePoster as string | undefined,
      overview: g.overview as string | undefined,
    }));
  }, []);

  const handleIdentifyPress = useCallback(
    (gameName: string) => {
      setIdentifyingItem(gameName);
      setIdentifySearchTerm(gameName);
      setIdentifyResults([]);
      setIsSearching(true);

      searchIgdb(gameName).then((results) => {
        setIdentifyResults(results);
        setIsSearching(false);
      });
    },
    [searchIgdb]
  );

  const handleIdentifySearch = useCallback(() => {
    if (!identifySearchTerm) {
      return;
    }

    setIsSearching(true);
    searchIgdb(identifySearchTerm).then((results) => {
      setIdentifyResults(results);
      setIsSearching(false);
    });
  }, [identifySearchTerm, searchIgdb]);

  const handleSelectMatch = useCallback(
    (match: LookupResult) => {
      if (!identifyingItem) {
        return;
      }

      setIgdbMatches((prev) => ({
        ...prev,
        [identifyingItem]: {
          igdbId: match.igdbId,
          title: match.title,
          year: match.year,
          remotePoster: match.remotePoster,
          overview: match.overview,
        },
      }));
      setIdentifyingItem(null);
    },
    [identifyingItem]
  );

  const handleClearMatch = useCallback((gameName: string) => {
    setIgdbMatches((prev) => {
      const next = { ...prev };
      delete next[gameName];
      return next;
    });
  }, []);

  const handleAutoMatchAll = useCallback(async () => {
    autoMatchCancelRef.current = false;
    setIsAutoMatching(true);
    setAutoMatchProgress(0);

    const unmatched = items.filter((item) => !igdbMatches[item.gameName]);

    for (let i = 0; i < unmatched.length; i++) {
      if (autoMatchCancelRef.current) {
        break;
      }

      const item = unmatched[i];
      setAutoMatchProgress(i + 1);

      try {
        const results = await searchIgdb(item.gameName);

        if (results.length > 0) {
          // Pick the best match - first result with exact or close name match
          const exactMatch = results.find(
            (r) => r.title.toLowerCase() === item.gameName.toLowerCase()
          );
          const bestMatch = exactMatch ?? results[0];

          setIgdbMatches((prev) => ({
            ...prev,
            [item.gameName]: {
              igdbId: bestMatch.igdbId,
              title: bestMatch.title,
              year: bestMatch.year,
              remotePoster: bestMatch.remotePoster,
              overview: bestMatch.overview,
            },
          }));
        }
      } catch {
        // Skip failures silently
      }

      // Small delay to avoid rate limiting
      await new Promise((resolve) => setTimeout(resolve, 300));
    }

    setIsAutoMatching(false);
  }, [items, igdbMatches, searchIgdb]);

  const handleCancelAutoMatch = useCallback(() => {
    autoMatchCancelRef.current = true;
  }, []);

  const totalSize = useMemo(() => {
    return items
      .filter((item) => selectedItems.has(item.gameName))
      .reduce(
        (acc, item) => acc + item.files.reduce((s, f) => s + f.size, 0),
        0
      );
  }, [items, selectedItems]);

  const matchedCount = Object.keys(igdbMatches).length;
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
            <div className={styles.matchToolbar}>
              <span className={styles.matchCount}>
                {matchedCount} of {items.length} matched with IGDB
              </span>

              {isAutoMatching ? (
                <span className={styles.autoMatchProgress}>
                  <Icon name={icons.SPINNER} isSpinning={true} />
                  {` Matching ${autoMatchProgress} of ${items.filter((i) => !igdbMatches[i.gameName]).length + autoMatchProgress}...`}
                  <Button
                    kind={kinds.DANGER}
                    size={sizes.SMALL}
                    onPress={handleCancelAutoMatch}
                  >
                    Cancel
                  </Button>
                </span>
              ) : (
                <Button
                  kind={kinds.PRIMARY}
                  size={sizes.MEDIUM}
                  isDisabled={matchedCount === items.length}
                  onPress={handleAutoMatchAll}
                >
                  <Icon name={icons.SEARCH} />
                  {' Auto-Match All with IGDB'}
                </Button>
              )}
            </div>

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
                  <th className={styles.headerCell}>IGDB Match</th>
                  <th className={styles.headerCell}>System</th>
                  <th className={styles.headerCell}>Files</th>
                  <th className={styles.headerCell}>Size</th>
                  <th className={styles.headerCell} />
                </tr>
              </thead>
              <tbody>
                {items.map((item) => {
                  const itemSize = item.files.reduce(
                    (acc, f) => acc + f.size,
                    0
                  );
                  const isSelected = selectedItems.has(item.gameName);
                  const match = igdbMatches[item.gameName];

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
                      <td className={styles.matchCell}>
                        {match ? (
                          <span className={styles.matchedBadge}>
                            <Icon name={icons.CHECK} />
                            {` ${match.title}`}
                            {match.year ? ` (${match.year})` : ''}
                            <button
                              className={styles.clearMatchButton}
                              onClick={() => handleClearMatch(item.gameName)}
                              title="Clear match"
                            >
                              ×
                            </button>
                          </span>
                        ) : (
                          <span className={styles.unmatchedBadge}>
                            Unmatched
                          </span>
                        )}
                      </td>
                      <td className={styles.systemCell}>
                        <span className={styles.systemBadge}>
                          {item.systemName}
                        </span>
                      </td>
                      <td className={styles.filesCell}>
                        {item.files.length}
                      </td>
                      <td className={styles.sizeCell}>
                        {formatBytes(itemSize)}
                      </td>
                      <td className={styles.actionCell}>
                        <Button
                          kind={match ? kinds.DEFAULT : kinds.WARNING}
                          size={sizes.SMALL}
                          onPress={() => handleIdentifyPress(item.gameName)}
                        >
                          <Icon name={icons.SEARCH} />
                          {match ? ' Re-identify' : ' Identify'}
                        </Button>
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

        <Modal
          isOpen={identifyingItem !== null}
          size={sizes.LARGE}
          onModalClose={() => setIdentifyingItem(null)}
        >
          <ModalContent onModalClose={() => setIdentifyingItem(null)}>
            <ModalHeader>
              Identify Game: {identifyingItem}
            </ModalHeader>

            <ModalBody>
              <div className={styles.identifySearch}>
                <input
                  type="text"
                  className={styles.identifyInput}
                  value={identifySearchTerm}
                  onChange={(e) => setIdentifySearchTerm(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      handleIdentifySearch();
                    }
                  }}
                  placeholder="Search IGDB..."
                />
                <Button
                  kind={kinds.PRIMARY}
                  size={sizes.MEDIUM}
                  isDisabled={isSearching || !identifySearchTerm}
                  onPress={handleIdentifySearch}
                >
                  <Icon
                    name={isSearching ? icons.SPINNER : icons.SEARCH}
                    isSpinning={isSearching}
                  />
                  {' Search'}
                </Button>
              </div>

              {isSearching && <LoadingIndicator />}

              {!isSearching && identifyResults.length === 0 && (
                <Alert kind={kinds.INFO}>
                  No results found. Try a different search term.
                </Alert>
              )}

              {!isSearching && identifyResults.length > 0 && (
                <div className={styles.identifyResults}>
                  {identifyResults.map((result) => (
                    <div
                      key={result.igdbId}
                      className={styles.identifyResultRow}
                      onClick={() => handleSelectMatch(result)}
                    >
                      <div className={styles.identifyPoster}>
                        {result.remotePoster ? (
                          <img
                            src={result.remotePoster}
                            alt={result.title}
                            className={styles.identifyPosterImage}
                          />
                        ) : (
                          <div className={styles.identifyPosterPlaceholder}>
                            <Icon name={icons.SERIES_CONTINUING} />
                          </div>
                        )}
                      </div>
                      <div className={styles.identifyInfo}>
                        <div className={styles.identifyTitle}>
                          {result.title}
                          {result.year ? (
                            <span className={styles.identifyYear}>
                              {` (${result.year})`}
                            </span>
                          ) : null}
                        </div>
                        {result.overview && (
                          <div className={styles.identifyOverview}>
                            {result.overview.length > 200
                              ? `${result.overview.substring(0, 200)}...`
                              : result.overview}
                          </div>
                        )}
                        <div className={styles.identifyIgdbId}>
                          IGDB ID: {result.igdbId}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </ModalBody>

            <ModalFooter>
              <Button
                kind={kinds.DEFAULT}
                onPress={() => setIdentifyingItem(null)}
              >
                Close
              </Button>
            </ModalFooter>
          </ModalContent>
        </Modal>
      </PageContentBody>
    </PageContent>
  );
}

export default ScraperImportPage;
