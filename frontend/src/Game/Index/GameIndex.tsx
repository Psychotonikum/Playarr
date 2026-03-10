import React, { useCallback, useMemo, useRef, useState } from 'react';
import QueueDetailsProvider from 'Activity/Queue/Details/QueueDetailsProvider';
import { useAppDimension } from 'App/appStore';
import { SelectProvider } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar, { PageJumpBarItems } from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import withScrollPosition from 'Components/withScrollPosition';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import {
  setGameOption,
  setGameSort,
  setGameTableOptions,
  useGameOptions,
} from 'Game/gameOptionsStore';
import NoGame from 'Game/NoGame';
import { FILTERS, useGameIndex } from 'Game/useGame';
import { align, icons, kinds } from 'Helpers/Props';
import { DESCENDING } from 'Helpers/Props/sortDirections';
import ParseToolbarButton from 'Parse/ParseToolbarButton';
import scrollPositions from 'Store/scrollPositions';
import { TableOptionsChangePayload } from 'typings/Table';
import translate from 'Utilities/String/translate';
import GameIndexFooter from './GameIndexFooter';
import GameIndexRefreshGameButton from './GameIndexRefreshGameButton';
import GameIndexFilterMenu from './Menus/GameIndexFilterMenu';
import GameIndexSortMenu from './Menus/GameIndexSortMenu';
import GameIndexViewMenu from './Menus/GameIndexViewMenu';
import GameIndexOverviews from './Overview/GameIndexOverviews';
import GameIndexOverviewOptionsModal from './Overview/Options/GameIndexOverviewOptionsModal';
import GameIndexPosters from './Posters/GameIndexPosters';
import GameIndexPosterOptionsModal from './Posters/Options/GameIndexPosterOptionsModal';
import GameIndexSelectAllButton from './Select/GameIndexSelectAllButton';
import GameIndexSelectAllMenuItem from './Select/GameIndexSelectAllMenuItem';
import GameIndexSelectFooter from './Select/GameIndexSelectFooter';
import GameIndexSelectModeButton from './Select/GameIndexSelectModeButton';
import GameIndexSelectModeMenuItem from './Select/GameIndexSelectModeMenuItem';
import GameIndexTable from './Table/GameIndexTable';
import GameIndexTableOptions from './Table/GameIndexTableOptions';
import styles from './GameIndex.css';

function getViewComponent(view: string) {
  if (view === 'posters') {
    return GameIndexPosters;
  }

  if (view === 'overview') {
    return GameIndexOverviews;
  }

  return GameIndexTable;
}

interface GameIndexProps {
  initialScrollTop?: number;
}

const GameIndex = withScrollPosition((props: GameIndexProps) => {
  const {
    isLoading: isFetching,
    isFetched,
    isError: error,
    data,
    totalItems,
  } = useGameIndex();

  const { selectedFilterKey, sortKey, sortDirection, view, columns } =
    useGameOptions();
  const filters = FILTERS;

  const customFilters = useCustomFiltersList('game');

  const executeCommand = useExecuteCommand();
  const isRssSyncExecuting = useCommandExecuting(CommandNames.RssSync);
  const isSmallScreen = useAppDimension('isSmallScreen');
  const scrollerRef = useRef<HTMLDivElement>(null);
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [jumpToCharacter, setJumpToCharacter] = useState<string | undefined>(
    undefined
  );
  const [isSelectMode, setIsSelectMode] = useState(false);

  const onRssSyncPress = useCallback(() => {
    executeCommand({
      name: CommandNames.RssSync,
    });
  }, [executeCommand]);

  const onSelectModePress = useCallback(() => {
    setIsSelectMode(!isSelectMode);
  }, [isSelectMode, setIsSelectMode]);

  const onTableOptionChange = useCallback(
    (
      payload: TableOptionsChangePayload & {
        tableOptions?: { showBanners?: boolean; showSearchAction?: boolean };
      }
    ) => {
      if (payload.tableOptions) {
        setGameTableOptions(payload.tableOptions);
      } else if (payload.columns) {
        setGameOption('columns', payload.columns);
      }
    },
    []
  );

  const onViewSelect = useCallback(
    (value: string) => {
      setGameOption('view', value);

      if (scrollerRef.current) {
        scrollerRef.current.scrollTo(0, 0);
      }
    },
    [scrollerRef]
  );

  const onSortSelect = useCallback((value: string) => {
    setGameSort({ sortKey: value });
  }, []);

  const onFilterSelect = useCallback((value: string | number) => {
    setGameOption('selectedFilterKey', value);
  }, []);

  const onOptionsPress = useCallback(() => {
    setIsOptionsModalOpen(true);
  }, [setIsOptionsModalOpen]);

  const onOptionsModalClose = useCallback(() => {
    setIsOptionsModalOpen(false);
  }, [setIsOptionsModalOpen]);

  const onJumpBarItemPress = useCallback(
    (character: string) => {
      setJumpToCharacter(character);
    },
    [setJumpToCharacter]
  );

  const onScroll = useCallback(
    ({ scrollTop }: { scrollTop: number }) => {
      setJumpToCharacter(undefined);
      scrollPositions.seriesIndex = scrollTop;
    },
    [setJumpToCharacter]
  );

  const jumpBarItems: PageJumpBarItems = useMemo(() => {
    // Reset if not sorting by sortTitle
    if (sortKey !== 'sortTitle') {
      return {
        characters: {},
        order: [],
      };
    }

    const characters = data.reduce((acc: Record<string, number>, item) => {
      let char = item.sortTitle.charAt(0);

      if (!isNaN(Number(char))) {
        char = '#';
      }

      if (char in acc) {
        acc[char] = acc[char] + 1;
      } else {
        acc[char] = 1;
      }

      return acc;
    }, {});

    const order = Object.keys(characters).sort();

    // Reverse if sorting descending
    if (sortDirection === DESCENDING) {
      order.reverse();
    }

    return {
      characters,
      order,
    };
  }, [data, sortKey, sortDirection]);
  const ViewComponent = useMemo(() => getViewComponent(view), [view]);

  const isLoaded = !!(!error && isFetched && data.length);
  const hasNoGame = !totalItems;

  return (
    <QueueDetailsProvider all={true}>
      <SelectProvider items={data}>
        <PageContent>
          <PageToolbar>
            <PageToolbarSection>
              <GameIndexRefreshGameButton
                isSelectMode={isSelectMode}
                selectedFilterKey={selectedFilterKey}
              />

              <PageToolbarButton
                label={translate('RssSync')}
                iconName={icons.RSS}
                isSpinning={isRssSyncExecuting}
                isDisabled={hasNoGame}
                onPress={onRssSyncPress}
              />

              <PageToolbarSeparator />

              <GameIndexSelectModeButton
                label={
                  isSelectMode
                    ? translate('StopSelecting')
                    : translate('SelectSeries')
                }
                iconName={isSelectMode ? icons.SERIES_ENDED : icons.CHECK}
                isSelectMode={isSelectMode}
                overflowComponent={GameIndexSelectModeMenuItem}
                onPress={onSelectModePress}
              />

              <GameIndexSelectAllButton
                label="SelectAll"
                isSelectMode={isSelectMode}
                overflowComponent={GameIndexSelectAllMenuItem}
              />

              <PageToolbarSeparator />
              <ParseToolbarButton />
            </PageToolbarSection>

            <PageToolbarSection
              alignContent={align.RIGHT}
              collapseButtons={false}
            >
              {view === 'table' ? (
                <TableOptionsModalWrapper
                  columns={columns}
                  optionsComponent={GameIndexTableOptions}
                  onTableOptionChange={onTableOptionChange}
                >
                  <PageToolbarButton
                    label={translate('Options')}
                    iconName={icons.TABLE}
                  />
                </TableOptionsModalWrapper>
              ) : (
                <PageToolbarButton
                  label={translate('Options')}
                  iconName={view === 'posters' ? icons.POSTER : icons.OVERVIEW}
                  isDisabled={hasNoGame}
                  onPress={onOptionsPress}
                />
              )}

              <PageToolbarSeparator />

              <GameIndexViewMenu
                view={view}
                isDisabled={hasNoGame}
                onViewSelect={onViewSelect}
              />

              <GameIndexSortMenu
                sortKey={sortKey}
                sortDirection={sortDirection}
                isDisabled={hasNoGame}
                onSortSelect={onSortSelect}
              />

              <GameIndexFilterMenu
                selectedFilterKey={selectedFilterKey}
                filters={filters}
                customFilters={customFilters}
                isDisabled={hasNoGame}
                onFilterSelect={onFilterSelect}
              />
            </PageToolbarSection>
          </PageToolbar>
          <div className={styles.pageContentBodyWrapper}>
            <PageContentBody
              ref={scrollerRef}
              className={styles.contentBody}
              // eslint-disable-next-line @typescript-eslint/ban-ts-comment
              // @ts-ignore
              innerClassName={styles[`${view}InnerContentBody`]}
              initialScrollTop={props.initialScrollTop}
              onScroll={onScroll}
            >
              {isFetching && !isFetched ? <LoadingIndicator /> : null}

              {!isFetching && !!error ? (
                <Alert kind={kinds.DANGER}>
                  {translate('SeriesLoadError')}
                </Alert>
              ) : null}

              {isLoaded ? (
                <div className={styles.contentBodyContainer}>
                  <ViewComponent
                    scrollerRef={scrollerRef}
                    items={data}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    jumpToCharacter={jumpToCharacter}
                    isSelectMode={isSelectMode}
                    isSmallScreen={isSmallScreen}
                  />

                  <GameIndexFooter />
                </div>
              ) : null}

              {!error && isFetched && !data.length ? (
                <NoGame totalItems={totalItems} />
              ) : null}
            </PageContentBody>
            {isLoaded && !!jumpBarItems.order.length ? (
              <PageJumpBar
                items={jumpBarItems}
                onItemPress={onJumpBarItemPress}
              />
            ) : null}
          </div>

          {isSelectMode ? <GameIndexSelectFooter /> : null}

          {view === 'posters' ? (
            <GameIndexPosterOptionsModal
              isOpen={isOptionsModalOpen}
              onModalClose={onOptionsModalClose}
            />
          ) : null}
          {view === 'overview' ? (
            <GameIndexOverviewOptionsModal
              isOpen={isOptionsModalOpen}
              onModalClose={onOptionsModalClose}
            />
          ) : null}
        </PageContent>
      </SelectProvider>
    </QueueDetailsProvider>
  );
}, 'seriesIndex');

export default GameIndex;
