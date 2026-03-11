import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import GameTagList from 'Components/GameTagList';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import DeleteGameModal from 'Game/Delete/DeleteGameModal';
import EditGameModal from 'Game/Edit/EditGameModal';
import { Statistics } from 'Game/Game';
import { useGamePosterOptions } from 'Game/gameOptionsStore';
import GamePoster from 'Game/GamePoster';
import GameIndexProgressBar from 'Game/Index/ProgressBar/GameIndexProgressBar';
import GameIndexPosterSelect from 'Game/Index/Select/GameIndexPosterSelect';
import { icons } from 'Helpers/Props';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import translate from 'Utilities/String/translate';
import useGameIndexItem from '../useGameIndexItem';
import GameIndexPosterInfo from './GameIndexPosterInfo';
import styles from './GameIndexPoster.css';

interface GameIndexPosterProps {
  gameId: number;
  sortKey: string;
  isSelectMode: boolean;
  posterWidth: number;
  posterHeight: number;
}

function GameIndexPoster(props: GameIndexPosterProps) {
  const { gameId, sortKey, isSelectMode, posterWidth, posterHeight } = props;

  const { game, qualityProfile, isRefreshingSeries, isSearchingSeries } =
    useGameIndexItem(gameId);

  const {
    detailedProgressBar,
    showTitle,
    showMonitored,
    showQualityProfile,
    showTags,
    showSearchAction,
  } = useGamePosterOptions();

  const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
    useUiSettingsValues();

  const executeCommand = useExecuteCommand();
  const [hasPosterError, setHasPosterError] = useState(false);
  const [isEditGameModalOpen, setIsEditGameModalOpen] = useState(false);
  const [isDeleteGameModalOpen, setIsDeleteGameModalOpen] = useState(false);

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

  const onPosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, [setHasPosterError]);

  const onPosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, [setHasPosterError]);

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

  if (!game) {
    return null;
  }

  const {
    title,
    monitored,
    status,
    path,
    titleSlug,
    originalLanguage,
    added,
    statistics = {} as Statistics,
    images,
    ratings,
    tags,
  } = game;

  const {
    platformCount = 0,
    romCount = 0,
    romFileCount = 0,
    totalRomCount = 0,
    sizeOnDisk = 0,
  } = statistics;

  const link = `/game/${titleSlug}`;

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer} title={title}>
        {isSelectMode ? (
          <GameIndexPosterSelect gameId={gameId} titleSlug={titleSlug} />
        ) : null}

        <Label className={styles.controls}>
          <SpinnerIconButton
            className={styles.action}
            name={icons.REFRESH}
            title={translate('RefreshSeries')}
            isSpinning={isRefreshingSeries}
            tabIndex={-1}
            onPress={onRefreshPress}
          />

          {showSearchAction ? (
            <SpinnerIconButton
              className={styles.action}
              name={icons.SEARCH}
              title={translate('SearchForMonitoredEpisodes')}
              isSpinning={isSearchingSeries}
              tabIndex={-1}
              onPress={onSearchPress}
            />
          ) : null}

          <IconButton
            className={styles.action}
            name={icons.EDIT}
            title={translate('EditGame')}
            tabIndex={-1}
            onPress={onEditGamePress}
          />
        </Label>

        {status === 'deleted' ? (
          <div
            className={classNames(styles.status, styles.deleted)}
            title={translate('Deleted')}
          />
        ) : null}

        <Link className={styles.link} style={elementStyle} to={link}>
          <GamePoster
            style={elementStyle}
            images={images}
            size={250}
            lazy={false}
            overflow={true}
            title={title}
            onError={onPosterLoadError}
            onLoad={onPosterLoad}
          />

          {hasPosterError ? (
            <div className={styles.overlayTitle}>{title}</div>
          ) : null}
        </Link>
      </div>

      <GameIndexProgressBar
        gameId={gameId}
        monitored={monitored}
        status={status}
        romCount={romCount}
        romFileCount={romFileCount}
        totalRomCount={totalRomCount}
        width={posterWidth}
        detailedProgressBar={detailedProgressBar}
        isStandalone={false}
      />

      {showTitle ? (
        <div className={styles.title} title={title}>
          {title}
        </div>
      ) : null}

      {showMonitored ? (
        <div className={styles.title}>
          {monitored ? translate('Monitored') : translate('Unmonitored')}
        </div>
      ) : null}

      {showQualityProfile && !!qualityProfile?.name ? (
        <div className={styles.title} title={translate('QualityProfile')}>
          {qualityProfile.name}
        </div>
      ) : null}

      {showTags && tags.length ? (
        <div className={styles.tags}>
          <div className={styles.tagsList}>
            <GameTagList tags={tags} />
          </div>
        </div>
      ) : null}

      <GameIndexPosterInfo
        originalLanguage={originalLanguage}
        added={added}
        platformCount={platformCount}
        sizeOnDisk={sizeOnDisk}
        path={path}
        qualityProfile={qualityProfile}
        showQualityProfile={showQualityProfile}
        showRelativeDates={showRelativeDates}
        sortKey={sortKey}
        shortDateFormat={shortDateFormat}
        longDateFormat={longDateFormat}
        timeFormat={timeFormat}
        tags={tags}
        showTags={showTags}
        ratings={ratings}
      />

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
    </div>
  );
}

export default GameIndexPoster;
