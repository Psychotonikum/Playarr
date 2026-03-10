import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import CheckInput from 'Components/Form/CheckInput';
import GameTagList from 'Components/GameTagList';
import HeartRating from 'Components/HeartRating';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import Column from 'Components/Table/Column';
import DeleteGameModal from 'Game/Delete/DeleteGameModal';
import EditGameModal from 'Game/Edit/EditGameModal';
import { Statistics } from 'Game/Game';
import GameBanner from 'Game/GameBanner';
import { useGameTableOptions } from 'Game/gameOptionsStore';
import GameTitleLink from 'Game/GameTitleLink';
import { icons } from 'Helpers/Props';
import useCountryName from 'Internationalization/useCountryName';
import { SelectStateInputProps } from 'typings/props';
import formatBytes from 'Utilities/Number/formatBytes';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import GameIndexProgressBar from '../ProgressBar/GameIndexProgressBar';
import useGameIndexItem from '../useGameIndexItem';
import GameStatusCell from './GameStatusCell';
import hasGrowableColumns from './hasGrowableColumns';
import PlatformsCell from './PlatformsCell';
import styles from './GameIndexRow.css';

interface GameIndexRowProps {
  gameId: number;
  sortKey: string;
  columns: Column[];
  isSelectMode: boolean;
}

function GameIndexRow(props: GameIndexRowProps) {
  const { gameId, columns, isSelectMode } = props;

  const {
    game,
    qualityProfile,
    latestPlatform,
    isRefreshingSeries,
    isSearchingSeries,
  } = useGameIndexItem(gameId);

  const { showBanners, showSearchAction } = useGameTableOptions();

  const executeCommand = useExecuteCommand();
  const [hasBannerError, setHasBannerError] = useState(false);
  const [isEditGameModalOpen, setIsEditGameModalOpen] = useState(false);
  const [isDeleteGameModalOpen, setIsDeleteGameModalOpen] = useState(false);
  const { getIsSelected, toggleSelected } = useSelect();
  const originalCountryName = useCountryName(game?.originalCountry);

  const onRefreshPress = useCallback(() => {
    executeCommand({
      name: CommandNames.RefreshSeries,
      gameIds: [gameId],
    });
  }, [gameId, executeCommand]);

  const onSearchPress = useCallback(() => {
    executeCommand({
      name: CommandNames.SeriesSearch,
      gameId,
    });
  }, [gameId, executeCommand]);

  const onBannerLoadError = useCallback(() => {
    setHasBannerError(true);
  }, [setHasBannerError]);

  const onBannerLoad = useCallback(() => {
    setHasBannerError(false);
  }, [setHasBannerError]);

  const onEditGamePress = useCallback(() => {
    setIsEditGameModalOpen(true);
  }, [setIsEditGameModalOpen]);

  const onEditGameModalClose = useCallback(() => {
    setIsEditGameModalOpen(false);
  }, [setIsEditGameModalOpen]);

  const onDeleteGamePress = useCallback(() => {
    setIsEditGameModalOpen(false);
    setIsDeleteGameModalOpen(true);
  }, [setIsDeleteGameModalOpen]);

  const onDeleteGameModalClose = useCallback(() => {
    setIsDeleteGameModalOpen(false);
  }, [setIsDeleteGameModalOpen]);

  const checkInputCallback = useCallback(() => {
    // Mock handler to satisfy `onChange` being required for `CheckInput`.
  }, []);

  const onSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [toggleSelected]
  );

  if (!game) {
    return null;
  }

  const {
    title,
    monitored,
    monitorNewItems,
    status,
    path,
    titleSlug,
    nextAiring,
    previousAiring,
    added,
    statistics = {} as Statistics,
    platformFolder,
    images,
    gameType,
    network,
    originalLanguage,
    certification,
    year,
    genres = [],
    ratings,
    platforms = [],
    tags = [],
  } = game;

  const {
    platformCount = 0,
    romCount = 0,
    romFileCount = 0,
    totalRomCount = 0,
    sizeOnDisk = 0,
    releaseGroups = [],
  } = statistics;

  return (
    <>
      {isSelectMode ? (
        <VirtualTableSelectCell
          id={gameId}
          isSelected={getIsSelected(gameId)}
          isDisabled={false}
          onSelectedChange={onSelectedChange}
        />
      ) : null}

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'status') {
          return (
            <GameStatusCell
              key={name}
              className={styles[name]}
              gameId={gameId}
              monitored={monitored}
              status={status}
              isSelectMode={isSelectMode}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'sortTitle') {
          return (
            <VirtualTableRowCell
              key={name}
              className={classNames(
                styles[name],
                showBanners && styles.banner,
                showBanners && !hasGrowableColumns(columns) && styles.bannerGrow
              )}
            >
              {showBanners ? (
                <Link className={styles.link} to={`/game/${titleSlug}`}>
                  <GameBanner
                    className={styles.bannerImage}
                    images={images}
                    lazy={false}
                    overflow={true}
                    title={title}
                    onError={onBannerLoadError}
                    onLoad={onBannerLoad}
                  />

                  {hasBannerError && (
                    <div className={styles.overlayTitle}>{title}</div>
                  )}
                </Link>
              ) : (
                <GameTitleLink titleSlug={titleSlug} title={title} />
              )}
            </VirtualTableRowCell>
          );
        }

        if (name === 'gameType') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {titleCase(gameType)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'network') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {network}
            </VirtualTableRowCell>
          );
        }

        if (name === 'originalCountry') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {originalCountryName}
            </VirtualTableRowCell>
          );
        }

        if (name === 'originalLanguage') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {originalLanguage.name}
            </VirtualTableRowCell>
          );
        }

        if (name === 'qualityProfileId') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {qualityProfile?.name ?? ''}
            </VirtualTableRowCell>
          );
        }

        if (name === 'nextAiring') {
          return (
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore ts(2739)
            <RelativeDateCell
              key={name}
              className={styles[name]}
              date={nextAiring}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'previousAiring') {
          return (
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore ts(2739)
            <RelativeDateCell
              key={name}
              className={styles[name]}
              date={previousAiring}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'added') {
          return (
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore ts(2739)
            <RelativeDateCell
              key={name}
              className={styles[name]}
              date={added}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'platformCount') {
          return (
            <PlatformsCell
              key={name}
              className={styles[name]}
              gameId={gameId}
              platformCount={platformCount}
              platforms={platforms}
              isSelectMode={isSelectMode}
            />
          );
        }

        if (name === 'platformFolder') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <CheckInput
                name="platformFolder"
                value={platformFolder}
                isDisabled={true}
                onChange={checkInputCallback}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'romProgress') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <GameIndexProgressBar
                gameId={gameId}
                monitored={monitored}
                status={status}
                romCount={romCount}
                romFileCount={romFileCount}
                totalRomCount={totalRomCount}
                width={125}
                detailedProgressBar={true}
                isStandalone={true}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'latestPlatform') {
          if (!latestPlatform) {
            return <VirtualTableRowCell key={name} className={styles[name]} />;
          }

          const platformStatistics = latestPlatform.statistics || {};

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <GameIndexProgressBar
                gameId={gameId}
                platformNumber={latestPlatform.platformNumber}
                monitored={monitored}
                status={status}
                romCount={platformStatistics.romCount}
                romFileCount={platformStatistics.romFileCount}
                totalRomCount={platformStatistics.totalRomCount}
                width={125}
                detailedProgressBar={true}
                isStandalone={true}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'romCount') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {totalRomCount}
            </VirtualTableRowCell>
          );
        }

        if (name === 'year') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {year}
            </VirtualTableRowCell>
          );
        }

        if (name === 'path') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {path}
            </VirtualTableRowCell>
          );
        }

        if (name === 'sizeOnDisk') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {formatBytes(sizeOnDisk)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'genres') {
          const joinedGenres = genres.join(', ');

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={joinedGenres}>{joinedGenres}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'ratings') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <HeartRating rating={ratings.value} votes={ratings.votes} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'certification') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {certification}
            </VirtualTableRowCell>
          );
        }

        if (name === 'releaseGroups') {
          const joinedReleaseGroups = releaseGroups.join(', ');
          const truncatedReleaseGroups =
            releaseGroups.length > 3
              ? `${releaseGroups.slice(0, 3).join(', ')}...`
              : joinedReleaseGroups;

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={joinedReleaseGroups}>{truncatedReleaseGroups}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'tags') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <GameTagList tags={tags} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'monitorNewItems') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {monitorNewItems === 'all'
                ? translate('SeasonsMonitoredAll')
                : translate('SeasonsMonitoredNone')}
            </VirtualTableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <SpinnerIconButton
                name={icons.REFRESH}
                title={translate('RefreshSeries')}
                isSpinning={isRefreshingSeries}
                onPress={onRefreshPress}
              />

              {showSearchAction ? (
                <SpinnerIconButton
                  name={icons.SEARCH}
                  title={translate('SearchForMonitoredEpisodes')}
                  isSpinning={isSearchingSeries}
                  onPress={onSearchPress}
                />
              ) : null}

              <IconButton
                name={icons.EDIT}
                title={translate('EditGame')}
                onPress={onEditGamePress}
              />
            </VirtualTableRowCell>
          );
        }

        return null;
      })}

      <EditGameModal
        isOpen={isEditGameModalOpen}
        gameId={gameId}
        onModalClose={onEditGameModalClose}
        onDeleteGamePress={onDeleteGamePress}
      />

      <DeleteGameModal
        isOpen={isDeleteGameModalOpen}
        gameId={gameId}
        onModalClose={onDeleteGameModalClose}
      />
    </>
  );
}

export default GameIndexRow;
