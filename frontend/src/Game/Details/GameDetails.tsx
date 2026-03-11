import moment from 'moment';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommands, useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import MetadataAttribution from 'Components/MetadataAttribution';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import DeleteGameModal from 'Game/Delete/DeleteGameModal';
import EditGameModal from 'Game/Edit/EditGameModal';
import { Image, Statistics } from 'Game/Game';
import GameGenres from 'Game/GameGenres';
import GamePoster from 'Game/GamePoster';
import { getGameStatusDetails } from 'Game/GameStatus';
import GameHistoryModal from 'Game/History/GameHistoryModal';
import MonitoringOptionsModal from 'Game/MonitoringOptions/MonitoringOptionsModal';
import useGame, { useSingleGame, useToggleGameMonitored } from 'Game/useGame';
import { useGameSystem } from 'GameSystem/useGameSystems';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  align,
  icons,
  kinds,
  sizes,
  sortDirections,
  tooltipPositions,
} from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import OrganizePreviewModal from 'Organize/OrganizePreviewModal';
import useRoms from 'Rom/useRoms';
import useRomFiles from 'RomFile/useRomFiles';
import QualityProfileName from 'Settings/Profiles/Quality/QualityProfileName';
import sortByProp from 'Utilities/Array/sortByProp';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import filterAlternateTitles from 'Utilities/Game/filterAlternateTitles';
import formatBytes from 'Utilities/Number/formatBytes';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import toggleSelected from 'Utilities/Table/toggleSelected';
import GameAlternateTitles from './GameAlternateTitles';
import GameDetailsLinks from './GameDetailsLinks';
import GameDetailsPlatform from './GameDetailsPlatform';
import GameDetailsProvider from './GameDetailsProvider';
import GamePatchInfo from './GamePatchInfo';
import GameProgressLabel from './GameProgressLabel';
import GameTags from './GameTags';
import styles from './GameDetails.css';

function getFanartUrl(images: Image[]) {
  return images.find((image) => image.coverType === 'fanart')?.url;
}

function getDateYear(date: string | undefined) {
  const dateDate = moment.utc(date);

  return dateDate.format('YYYY');
}

interface ExpandedState {
  allExpanded: boolean;
  allCollapsed: boolean;
  platforms: Record<number, boolean>;
}

interface GameDetailsProps {
  gameId: number;
}

function GameDetails({ gameId }: GameDetailsProps) {
  const executeCommand = useExecuteCommand();

  const game = useSingleGame(gameId);
  const { toggleGameMonitored, isTogglingGameMonitored } =
    useToggleGameMonitored(gameId);
  const { data: allGames } = useGame();

  const {
    isFetching: isEpisodesFetching,
    isFetched: isEpisodesFetched,
    error: episodesError,
    data,
    refetch: refetchEpisodes,
  } = useRoms({ gameId });

  const { hasRoms, hasMonitoredRoms } = useMemo(() => {
    return {
      hasRoms: data.length > 0,
      hasMonitoredRoms: data.some((e) => e.monitored),
    };
  }, [data]);

  const {
    isFetching: isRomFilesFetching,
    isFetched: isRomFilesFetched,
    error: romFilesError,
    data: romFiles,
    hasRomFiles,
    refetch: refetchRomFiles,
  } = useRomFiles({ gameId });

  const { data: gameSystem } = useGameSystem(game?.gameSystemId ?? 0);

  const patchInfo = useMemo(() => {
    const systemType = gameSystem?.systemType ?? 0;
    const baseFile = romFiles.find((f) => f.romFileType === 0);
    const updates = romFiles.filter((f) => f.romFileType === 1);
    const dlcs = romFiles.filter((f) => f.romFileType === 2);
    const isMissingBase = !baseFile && (updates.length > 0 || dlcs.length > 0);

    return { systemType, baseFile, updates, dlcs, isMissingBase };
  }, [romFiles, gameSystem]);

  const { data: commands } = useCommands();

  const { isRefreshing, isRenaming, isSearching } = useMemo(() => {
    const seriesRefreshingCommand = findCommand(commands, {
      name: CommandNames.RefreshSeries,
    });

    const isSeriesRefreshingCommandExecuting = isCommandExecuting(
      seriesRefreshingCommand
    );

    const allGamesRefreshing =
      isSeriesRefreshingCommandExecuting &&
      seriesRefreshingCommand &&
      (!('gameIds' in seriesRefreshingCommand.body) ||
        seriesRefreshingCommand.body.gameIds.length === 0);

    const isSeriesRefreshing =
      isSeriesRefreshingCommandExecuting &&
      seriesRefreshingCommand &&
      'gameIds' in seriesRefreshingCommand.body &&
      seriesRefreshingCommand.body.gameIds.includes(gameId);

    const isSearchingExecuting = isCommandExecuting(
      findCommand(commands, {
        name: CommandNames.SeriesSearch,
        gameId,
      })
    );

    const isRenamingFiles = isCommandExecuting(
      findCommand(commands, {
        name: CommandNames.RenameFiles,
        gameId,
      })
    );

    const isRenamingSeriesCommand = findCommand(commands, {
      name: CommandNames.RenameSeries,
    });

    const isRenamingSeries =
      isCommandExecuting(isRenamingSeriesCommand) &&
      isRenamingSeriesCommand &&
      'gameIds' in isRenamingSeriesCommand.body &&
      isRenamingSeriesCommand.body.gameIds.includes(gameId);

    return {
      isRefreshing: isSeriesRefreshing || allGamesRefreshing,
      isRenaming: isRenamingFiles || isRenamingSeries,
      isSearching: isSearchingExecuting,
    };
  }, [gameId, commands]);

  const { nextSeries, previousSeries } = useMemo(() => {
    const sortedSeries = [...allGames].sort(sortByProp('sortTitle'));
    const seriesIndex = sortedSeries.findIndex((game) => game.id === gameId);

    if (seriesIndex === -1) {
      return {
        nextSeries: undefined,
        previousSeries: undefined,
      };
    }

    const nextSeries = sortedSeries[seriesIndex + 1] ?? sortedSeries[0];
    const previousSeries =
      sortedSeries[seriesIndex - 1] ?? sortedSeries[sortedSeries.length - 1];

    return {
      nextSeries: {
        title: nextSeries.title,
        titleSlug: nextSeries.titleSlug,
      },
      previousSeries: {
        title: previousSeries.title,
        titleSlug: previousSeries.titleSlug,
      },
    };
  }, [gameId, allGames]);

  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isManageEpisodesOpen, setIsManageEpisodesOpen] = useState(false);
  const [isEditGameModalOpen, setIsEditGameModalOpen] = useState(false);
  const [isDeleteGameModalOpen, setIsDeleteGameModalOpen] = useState(false);
  const [isGameHistoryModalOpen, setIsGameHistoryModalOpen] = useState(false);
  const [isMonitorOptionsModalOpen, setIsMonitorOptionsModalOpen] =
    useState(false);
  const [expandedState, setExpandedState] = useState<ExpandedState>({
    allExpanded: false,
    allCollapsed: false,
    platforms: {},
  });
  const wasRefreshing = usePrevious(isRefreshing);
  const wasRenaming = usePrevious(isRenaming);

  const alternateTitles = useMemo(() => {
    if (!game) {
      return [];
    }

    return filterAlternateTitles(
      game.alternateTitles,
      game.title,
      game.useSceneNumbering
    );
  }, [game]);

  const handleOrganizePress = useCallback(() => {
    setIsOrganizeModalOpen(true);
  }, []);

  const handleOrganizeModalClose = useCallback(() => {
    setIsOrganizeModalOpen(false);
  }, []);

  const handleManageEpisodesPress = useCallback(() => {
    setIsManageEpisodesOpen(true);
  }, []);

  const handleManageEpisodesModalClose = useCallback(() => {
    setIsManageEpisodesOpen(false);
  }, []);

  const handleEditGamePress = useCallback(() => {
    setIsEditGameModalOpen(true);
  }, []);

  const handleEditGameModalClose = useCallback(() => {
    setIsEditGameModalOpen(false);
  }, []);

  const handleDeleteGamePress = useCallback(() => {
    setIsEditGameModalOpen(false);
    setIsDeleteGameModalOpen(true);
  }, []);

  const handleDeleteGameModalClose = useCallback(() => {
    setIsDeleteGameModalOpen(false);
  }, []);

  const handleSeriesHistoryPress = useCallback(() => {
    setIsGameHistoryModalOpen(true);
  }, []);

  const handleGameHistoryModalClose = useCallback(() => {
    setIsGameHistoryModalOpen(false);
  }, []);

  const handleMonitorOptionsPress = useCallback(() => {
    setIsMonitorOptionsModalOpen(true);
  }, []);

  const handleMonitorOptionsClose = useCallback(() => {
    setIsMonitorOptionsModalOpen(false);
  }, []);

  const handleExpandAllPress = useCallback(() => {
    const expandAll = !expandedState.allExpanded;

    const newSeasons = Object.keys(expandedState.platforms).reduce<
      Record<number | string, boolean>
    >((acc, item) => {
      acc[item] = expandAll;
      return acc;
    }, {});

    setExpandedState({
      allExpanded: expandAll,
      allCollapsed: !expandAll,
      platforms: newSeasons,
    });
  }, [expandedState]);

  const handleExpandPress = useCallback(
    (platformNumber: number, isExpanded: boolean) => {
      setExpandedState((state) => {
        const { allExpanded, allCollapsed } = state;

        const convertedState = {
          allSelected: allExpanded,
          allUnselected: allCollapsed,
          selectedState: state.platforms,
          lastToggled: null,
        };

        const newState = toggleSelected(
          convertedState,
          [],
          platformNumber,
          isExpanded,
          false
        );

        return {
          allExpanded: newState.allSelected,
          allCollapsed: newState.allUnselected,
          platforms: newState.selectedState,
        };
      });
    },
    []
  );

  const handleMonitorTogglePress = useCallback(
    (value: boolean) => {
      toggleGameMonitored({
        monitored: value,
      });
    },
    [toggleGameMonitored]
  );

  const handleRefreshPress = useCallback(() => {
    executeCommand({
      name: CommandNames.RefreshSeries,
      gameId,
    });
  }, [gameId, executeCommand]);

  const handleSearchPress = useCallback(() => {
    executeCommand({
      name: CommandNames.SeriesSearch,
      gameId,
    });
  }, [gameId, executeCommand]);

  const populate = useCallback(() => {
    refetchEpisodes();
    refetchRomFiles();
  }, [refetchEpisodes, refetchRomFiles]);

  useEffect(() => {
    populate();
  }, [populate]);

  useEffect(() => {
    registerPagePopulator(populate, ['seriesUpdated']);

    return () => {
      unregisterPagePopulator(populate);
    };
  }, [populate]);

  useEffect(() => {
    if ((!isRefreshing && wasRefreshing) || (!isRenaming && wasRenaming)) {
      populate();
    }
  }, [isRefreshing, wasRefreshing, isRenaming, wasRenaming, populate]);

  if (!game) {
    return null;
  }

  const {
    igdbId,
    rawgId,
    titleSlug,
    title,
    runtime,
    ratings,
    path,
    statistics = {} as Statistics,
    qualityProfileId,
    monitored,
    status,
    originalLanguage,
    overview,
    images,
    platforms,
    genres,
    tags,
    year,
  } = game;

  const {
    romCount = 0,
    romFileCount = 0,
    sizeOnDisk = 0,
    lastAired,
  } = statistics;

  const statusDetails = getGameStatusDetails(status);
  const runningYears =
    status === 'ended' ? `${year}-${getDateYear(lastAired)}` : `${year}-`;

  let romFilesCountMessage = translate('GameDetailsNoRomFiles');

  if (romFileCount === 1) {
    romFilesCountMessage = translate('GameDetailsOneRomFile');
  } else if (romFileCount > 1) {
    romFilesCountMessage = translate('GameDetailsCountRomFiles', {
      romFileCount,
    });
  }

  let expandIcon = icons.EXPAND_INDETERMINATE;

  if (expandedState.allExpanded) {
    expandIcon = icons.COLLAPSE;
  } else if (expandedState.allCollapsed) {
    expandIcon = icons.EXPAND;
  }

  const fanartUrl = getFanartUrl(images);
  const isFetching = isEpisodesFetching || isRomFilesFetching;
  const isPopulated = isEpisodesFetched && isRomFilesFetched;

  return (
    <GameDetailsProvider gameId={gameId}>
      <PageContent title={title}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('RefreshAndScan')}
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              title={translate('RefreshAndScanTooltip')}
              isSpinning={isRefreshing}
              onPress={handleRefreshPress}
            />

            <PageToolbarButton
              label={translate('SearchMonitored')}
              iconName={icons.SEARCH}
              isDisabled={!monitored || !hasMonitoredRoms || !hasRoms}
              isSpinning={isSearching}
              title={
                hasMonitoredRoms
                  ? undefined
                  : translate('NoMonitoredEpisodes')
              }
              onPress={handleSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('PreviewRename')}
              iconName={icons.ORGANIZE}
              isDisabled={!hasRomFiles}
              onPress={handleOrganizePress}
            />

            <PageToolbarButton
              label={translate('ManageEpisodes')}
              iconName={icons.ROM_FILE}
              onPress={handleManageEpisodesPress}
            />

            <PageToolbarButton
              label={translate('History')}
              iconName={icons.HISTORY}
              isDisabled={!hasRoms}
              onPress={handleSeriesHistoryPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('EpisodeMonitoring')}
              iconName={icons.MONITORED}
              onPress={handleMonitorOptionsPress}
            />

            <PageToolbarButton
              label={translate('Edit')}
              iconName={icons.EDIT}
              onPress={handleEditGamePress}
            />

            <PageToolbarButton
              label={translate('Delete')}
              iconName={icons.DELETE}
              onPress={handleDeleteGamePress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <PageToolbarButton
              label={
                expandedState.allExpanded
                  ? translate('CollapseAll')
                  : translate('ExpandAll')
              }
              iconName={expandIcon}
              onPress={handleExpandAllPress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody innerClassName={styles.innerContentBody}>
          <div className={styles.header}>
            <div
              className={styles.backdrop}
              style={
                fanartUrl ? { backgroundImage: `url(${fanartUrl})` } : undefined
              }
            >
              <div className={styles.backdropOverlay} />
            </div>

            <div className={styles.headerContent}>
              <GamePoster
                className={styles.poster}
                images={images}
                size={500}
                lazy={false}
                title={title}
              />

              <div className={styles.info}>
                <div className={styles.titleRow}>
                  <div className={styles.titleContainer}>
                    <div className={styles.toggleMonitoredContainer}>
                      <MonitorToggleButton
                        className={styles.monitorToggleButton}
                        monitored={monitored}
                        isSaving={isTogglingGameMonitored}
                        size={40}
                        onPress={handleMonitorTogglePress}
                      />
                    </div>

                    <div className={styles.title}>{title}</div>

                    {alternateTitles.length ? (
                      <div className={styles.alternateTitlesIconContainer}>
                        <Popover
                          anchor={
                            <Icon name={icons.ALTERNATE_TITLES} size={20} />
                          }
                          title={translate('AlternateTitles')}
                          body={
                            <GameAlternateTitles
                              alternateTitles={alternateTitles}
                            />
                          }
                          position={tooltipPositions.BOTTOM}
                        />
                      </div>
                    ) : null}
                  </div>

                  <div className={styles.seriesNavigationButtons}>
                    {previousSeries ? (
                      <IconButton
                        className={styles.seriesNavigationButton}
                        name={icons.ARROW_LEFT}
                        size={30}
                        title={translate('GameDetailsGoTo', {
                          title: previousSeries.title,
                        })}
                        to={`/game/${previousSeries.titleSlug}`}
                      />
                    ) : null}

                    {nextSeries ? (
                      <IconButton
                        className={styles.seriesNavigationButton}
                        name={icons.ARROW_RIGHT}
                        size={30}
                        title={translate('GameDetailsGoTo', {
                          title: nextSeries.title,
                        })}
                        to={`/game/${nextSeries.titleSlug}`}
                      />
                    ) : null}
                  </div>
                </div>

                <div className={styles.details}>
                  <div>
                    {runtime ? (
                      <span className={styles.runtime}>
                        {translate('GameDetailsRuntime', { runtime })}
                      </span>
                    ) : null}

                    {ratings?.value ? (
                      <HeartRating
                        rating={ratings.value}
                        votes={ratings.votes}
                        iconSize={20}
                      />
                    ) : null}

                    <GameGenres className={styles.genres} genres={genres} />

                    <span>{runningYears}</span>
                  </div>
                </div>

                <div>
                  <Label className={styles.detailsLabel} size={sizes.LARGE}>
                    <div>
                      <Icon name={icons.FOLDER} size={17} />
                      <span className={styles.path}>{path}</span>
                    </div>
                  </Label>

                  <Tooltip
                    anchor={
                      <Label className={styles.detailsLabel} size={sizes.LARGE}>
                        <div>
                          <Icon name={icons.DRIVE} size={17} />

                          <span className={styles.sizeOnDisk}>
                            {formatBytes(sizeOnDisk)}
                          </span>
                        </div>
                      </Label>
                    }
                    tooltip={<span>{romFilesCountMessage}</span>}
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                  <Label
                    className={styles.detailsLabel}
                    title={translate('QualityProfile')}
                    size={sizes.LARGE}
                  >
                    <div>
                      <Icon name={icons.PROFILE} size={17} />
                      <span className={styles.qualityProfileName}>
                        <QualityProfileName
                          qualityProfileId={qualityProfileId}
                        />
                      </span>
                    </div>
                  </Label>

                  <Label className={styles.detailsLabel} size={sizes.LARGE}>
                    <div>
                      <Icon
                        name={monitored ? icons.MONITORED : icons.UNMONITORED}
                        size={17}
                      />
                      <span className={styles.qualityProfileName}>
                        {monitored
                          ? translate('Monitored')
                          : translate('Unmonitored')}
                      </span>
                    </div>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title={statusDetails.message}
                    size={sizes.LARGE}
                    kind={status === 'deleted' ? kinds.INVERSE : undefined}
                  >
                    <div>
                      <Icon name={statusDetails.icon} size={17} />
                      <span className={styles.statusName}>
                        {statusDetails.title}
                      </span>
                    </div>
                  </Label>

                  {originalLanguage?.name ? (
                    <Label
                      className={styles.detailsLabel}
                      title={translate('OriginalLanguage')}
                      size={sizes.LARGE}
                    >
                      <div>
                        <Icon name={icons.LANGUAGE} size={17} />
                        <span className={styles.originalLanguageName}>
                          {originalLanguage.name}
                        </span>
                      </div>
                    </Label>
                  ) : null}

                  <Tooltip
                    anchor={
                      <Label className={styles.detailsLabel} size={sizes.LARGE}>
                        <div>
                          <Icon name={icons.EXTERNAL_LINK} size={17} />
                          <span className={styles.links}>
                            {translate('Links')}
                          </span>
                        </div>
                      </Label>
                    }
                    tooltip={
                      <GameDetailsLinks
                        igdbId={igdbId}
                        rawgId={rawgId}
                        titleSlug={titleSlug}
                      />
                    }
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                  {tags.length ? (
                    <Tooltip
                      anchor={
                        <Label
                          className={styles.detailsLabel}
                          size={sizes.LARGE}
                        >
                          <Icon name={icons.TAGS} size={17} />

                          <span className={styles.tags}>
                            {translate('Tags')}
                          </span>
                        </Label>
                      }
                      tooltip={<GameTags gameId={gameId} />}
                      kind={kinds.INVERSE}
                      position={tooltipPositions.BOTTOM}
                    />
                  ) : null}

                  <GameProgressLabel
                    className={styles.seriesProgressLabel}
                    gameId={gameId}
                    monitored={monitored}
                    romCount={romCount}
                    romFileCount={romFileCount}
                  />
                </div>

                <div className={styles.overview}>{overview}</div>

                <MetadataAttribution />
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {!isPopulated && !episodesError && !romFilesError ? (
              <LoadingIndicator />
            ) : null}

            {!isFetching && episodesError ? (
              <Alert kind={kinds.DANGER}>
                {translate('EpisodesLoadError')}
              </Alert>
            ) : null}

            {!isFetching && romFilesError ? (
              <Alert kind={kinds.DANGER}>
                {translate('RomFilesLoadError')}
              </Alert>
            ) : null}

            {isPopulated && !!platforms.length ? (
              <div>
                <GamePatchInfo
                  systemType={patchInfo.systemType}
                  baseFile={
                    patchInfo.baseFile
                      ? {
                          fileName: patchInfo.baseFile.relativePath,
                          fileType: 0,
                          version: patchInfo.baseFile.patchVersion,
                          dlcIndex: patchInfo.baseFile.dlcIndex,
                        }
                      : undefined
                  }
                  updates={patchInfo.updates.map((f) => ({
                    fileName: f.relativePath,
                    fileType: 1,
                    version: f.patchVersion,
                    dlcIndex: f.dlcIndex,
                  }))}
                  dlcs={patchInfo.dlcs.map((f) => ({
                    fileName: f.relativePath,
                    fileType: 2,
                    version: f.patchVersion,
                    dlcIndex: f.dlcIndex,
                  }))}
                  isMissingBase={patchInfo.isMissingBase}
                />
                {platforms
                  .slice(0)
                  .reverse()
                  .map((platform) => {
                    return (
                      <GameDetailsPlatform
                        key={platform.platformNumber}
                        gameId={gameId}
                        {...platform}
                        isExpanded={
                          expandedState.platforms[platform.platformNumber]
                        }
                        onExpandPress={handleExpandPress}
                      />
                    );
                  })}
              </div>
            ) : null}

            {isPopulated && !platforms.length ? (
              <Alert kind={kinds.WARNING}>
                {translate('NoRomInformation')}
              </Alert>
            ) : null}
          </div>

          <OrganizePreviewModal
            isOpen={isOrganizeModalOpen}
            gameId={gameId}
            onModalClose={handleOrganizeModalClose}
          />

          <InteractiveImportModal
            isOpen={isManageEpisodesOpen}
            gameId={gameId}
            title={title}
            folder={path}
            initialSortKey="relativePath"
            initialSortDirection={sortDirections.DESCENDING}
            showSeries={false}
            allowSeriesChange={false}
            showDelete={true}
            showImportMode={false}
            modalTitle={translate('ManageEpisodes')}
            onModalClose={handleManageEpisodesModalClose}
          />

          <GameHistoryModal
            isOpen={isGameHistoryModalOpen}
            gameId={gameId}
            onModalClose={handleGameHistoryModalClose}
          />

          <EditGameModal
            isOpen={isEditGameModalOpen}
            gameId={gameId}
            onModalClose={handleEditGameModalClose}
            onDeleteGamePress={handleDeleteGamePress}
          />

          <DeleteGameModal
            isOpen={isDeleteGameModalOpen}
            gameId={gameId}
            onModalClose={handleDeleteGameModalClose}
          />

          <MonitoringOptionsModal
            isOpen={isMonitorOptionsModalOpen}
            gameId={gameId}
            onModalClose={handleMonitorOptionsClose}
          />
        </PageContentBody>
      </PageContent>
    </GameDetailsProvider>
  );
}

export default GameDetails;
