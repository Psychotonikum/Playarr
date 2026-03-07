import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch } from 'react-redux';
import { useAppDimension } from 'App/appStore';
import CommandNames from 'Commands/CommandNames';
import { useCommands } from 'Commands/useCommands';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import SpinnerIcon from 'Components/SpinnerIcon';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import Popover from 'Components/Tooltip/Popover';
import Rom from 'Rom/Rom';
import {
  setEpisodeOptions,
  setEpisodeSort,
  useRomOptions,
} from 'Rom/romOptionsStore';
import { getQueryKey, useToggleEpisodesMonitored } from 'Rom/useRom';
import { useSeasonEpisodes } from 'Rom/useRoms';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { align, icons, sortDirections, tooltipPositions } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import OrganizePreviewModal from 'Organize/OrganizePreviewModal';
import GameHistoryModal from 'Game/History/GameHistoryModal';
import PlatformInteractiveSearchModal from 'Game/Search/PlatformInteractiveSearchModal';
import { Statistics } from 'Game/Game';
import { useSingleGame, useToggleSeasonMonitored } from 'Game/useGame';
import { TableOptionsChangePayload } from 'typings/Table';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import isAfter from 'Utilities/Date/isAfter';
import isBefore from 'Utilities/Date/isBefore';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import getToggledRange from 'Utilities/Table/getToggledRange';
import RomRow from './RomRow';
import PlatformInfo from './PlatformInfo';
import PlatformProgressLabel from './PlatformProgressLabel';
import styles from './GameDetailsPlatform.css';

function getPlatformStatistics(roms: Rom[]) {
  let romCount = 0;
  let romFileCount = 0;
  let totalRomCount = 0;
  let monitoredRomCount = 0;
  let hasMonitoredEpisodes = false;
  const sizeOnDisk = 0;

  roms.forEach((rom) => {
    if (
      rom.romFileId ||
      (rom.monitored && isBefore(rom.airDateUtc))
    ) {
      romCount++;
    }

    if (rom.romFileId) {
      romFileCount++;
    }

    if (rom.monitored) {
      monitoredRomCount++;
      hasMonitoredEpisodes = true;
    }

    totalRomCount++;
  });

  return {
    romCount,
    romFileCount,
    totalRomCount,
    monitoredRomCount,
    hasMonitoredEpisodes,
    sizeOnDisk,
  };
}

function useIsSearching(gameId: number, platformNumber: number) {
  const { data: commands } = useCommands();
  return isCommandExecuting(
    findCommand(commands, {
      name: CommandNames.SeasonSearch,
      gameId,
      platformNumber,
    })
  );
}

interface GameDetailsPlatformProps {
  gameId: number;
  monitored: boolean;
  platformNumber: number;
  statistics?: Statistics;
  isExpanded?: boolean;
  onExpandPress: (platformNumber: number, isExpanded: boolean) => void;
}

function GameDetailsPlatform({
  gameId,
  monitored,
  platformNumber,
  statistics = {} as Statistics,
  isExpanded,
  onExpandPress,
}: GameDetailsPlatformProps) {
  const dispatch = useDispatch();
  const { monitored: seriesMonitored, path } = useSingleGame(gameId)!;
  const { data: items } = useSeasonEpisodes(gameId, platformNumber);

  const { columns, sortKey, sortDirection } = useRomOptions();

  const isSmallScreen = useAppDimension('isSmallScreen');
  const isSearching = useIsSearching(gameId, platformNumber);

  const { sizeOnDisk = 0 } = statistics;

  const {
    romCount,
    romFileCount,
    totalRomCount,
    monitoredRomCount,
    hasMonitoredEpisodes,
  } = getPlatformStatistics(items);

  const previousRomFileCount = usePrevious(romFileCount);

  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isManageEpisodesOpen, setIsManageEpisodesOpen] = useState(false);
  const [isHistoryModalOpen, setIsHistoryModalOpen] = useState(false);
  const [isInteractiveSearchModalOpen, setIsInteractiveSearchModalOpen] =
    useState(false);

  const { toggleEpisodesMonitored, isToggling, togglingRomIds } =
    useToggleEpisodesMonitored(getQueryKey('roms')!);

  const { toggleSeasonMonitored, isTogglingSeasonMonitored } =
    useToggleSeasonMonitored(gameId);

  const lastToggledEpisode = useRef<number | null>(null);
  const hasSetInitalExpand = useRef(false);

  const platformNumberTitle =
    platformNumber === 0
      ? translate('Specials')
      : translate('PlatformNumberToken', { platformNumber });

  const handleMonitorSeasonPress = useCallback(
    (value: boolean) => {
      toggleSeasonMonitored({
        platformNumber,
        monitored: value,
      });
    },
    [platformNumber, toggleSeasonMonitored]
  );

  const handleExpandPress = useCallback(() => {
    onExpandPress(platformNumber, !isExpanded);
  }, [platformNumber, isExpanded, onExpandPress]);

  const handleMonitorEpisodePress = useCallback(
    (
      romId: number,
      value: boolean,
      { shiftKey }: { shiftKey: boolean }
    ) => {
      const lastToggled = lastToggledEpisode.current;
      const romIds = new Set([romId]);

      if (shiftKey && lastToggled) {
        const { lower, upper } = getToggledRange(items, romId, lastToggled);
        for (let i = lower; i < upper; i++) {
          romIds.add(items[i].id);
        }
      }

      lastToggledEpisode.current = romId;

      toggleEpisodesMonitored({
        romIds: Array.from(romIds),
        monitored: value,
      });
    },
    [items, toggleEpisodesMonitored]
  );

  const handleSearchPress = useCallback(() => {
    dispatch({
      name: CommandNames.SeasonSearch,
      gameId,
      platformNumber,
    });
  }, [gameId, platformNumber, dispatch]);

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

  const handleHistoryPress = useCallback(() => {
    setIsHistoryModalOpen(true);
  }, []);

  const handleHistoryModalClose = useCallback(() => {
    setIsHistoryModalOpen(false);
  }, []);

  const handleInteractiveSearchPress = useCallback(() => {
    setIsInteractiveSearchModalOpen(true);
  }, []);

  const handleInteractiveSearchModalClose = useCallback(() => {
    setIsInteractiveSearchModalOpen(false);
  }, []);

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setEpisodeSort({
        sortKey,
        sortDirection,
      });
    },
    []
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      setEpisodeOptions(payload);
    },
    []
  );

  useEffect(() => {
    if (hasSetInitalExpand.current || items.length === 0) {
      return;
    }

    hasSetInitalExpand.current = true;

    const expand =
      items.some(
        (item) =>
          isAfter(item.airDateUtc) || isAfter(item.airDateUtc, { days: -30 })
      ) || items.every((item) => !item.airDateUtc);

    onExpandPress(platformNumber, expand && platformNumber > 0);
  }, [items, gameId, platformNumber, onExpandPress]);

  useEffect(() => {
    if ((previousRomFileCount ?? 0) > 0 && romFileCount === 0) {
      setIsOrganizeModalOpen(false);
      setIsManageEpisodesOpen(false);
    }
  }, [romFileCount, previousRomFileCount]);

  return (
    <div className={styles.platform}>
      <div className={styles.header}>
        <div className={styles.left}>
          <MonitorToggleButton
            monitored={monitored}
            isDisabled={!seriesMonitored}
            isSaving={isTogglingSeasonMonitored}
            size={24}
            onPress={handleMonitorSeasonPress}
          />

          <div className={styles.seasonInfo}>
            <div className={styles.platformNumber}>{platformNumberTitle}</div>
          </div>

          <div className={styles.seasonStats}>
            <Popover
              className={styles.romCountTooltip}
              canFlip={true}
              anchor={
                <PlatformProgressLabel
                  className={styles.seasonStatsLabel}
                  gameId={gameId}
                  platformNumber={platformNumber}
                  monitored={monitored}
                  romCount={romCount}
                  romFileCount={romFileCount}
                />
              }
              title={translate('PlatformInformation')}
              body={
                <div>
                  <PlatformInfo
                    totalRomCount={totalRomCount}
                    monitoredRomCount={monitoredRomCount}
                    romFileCount={romFileCount}
                    sizeOnDisk={sizeOnDisk}
                  />
                </div>
              }
              position={tooltipPositions.BOTTOM}
            />

            {sizeOnDisk ? (
              <Label
                className={styles.seasonStatsLabel}
                kind="default"
                size="large"
              >
                {formatBytes(sizeOnDisk)}
              </Label>
            ) : null}
          </div>
        </div>

        <Link className={styles.expandButton} onPress={handleExpandPress}>
          <Icon
            className={styles.expandButtonIcon}
            name={isExpanded ? icons.COLLAPSE : icons.EXPAND}
            title={
              isExpanded ? translate('HideEpisodes') : translate('ShowEpisodes')
            }
            size={24}
          />
          {isSmallScreen ? null : <span>&nbsp;</span>}
        </Link>

        {isSmallScreen ? (
          <Menu
            className={styles.actionsMenu}
            alignMenu={align.RIGHT}
            enforceMaxHeight={false}
          >
            <MenuButton>
              <Icon name={icons.ACTIONS} size={22} />
            </MenuButton>

            <MenuContent className={styles.actionsMenuContent}>
              <MenuItem
                isDisabled={
                  isSearching || !hasMonitoredEpisodes || !seriesMonitored
                }
                onPress={handleSearchPress}
              >
                <SpinnerIcon
                  className={styles.actionMenuIcon}
                  name={icons.SEARCH}
                  isSpinning={isSearching}
                />

                {translate('Search')}
              </MenuItem>

              <MenuItem
                isDisabled={!totalRomCount}
                onPress={handleInteractiveSearchPress}
              >
                <Icon
                  className={styles.actionMenuIcon}
                  name={icons.INTERACTIVE}
                />

                {translate('InteractiveSearch')}
              </MenuItem>

              <MenuItem
                isDisabled={!romFileCount}
                onPress={handleOrganizePress}
              >
                <Icon className={styles.actionMenuIcon} name={icons.ORGANIZE} />

                {translate('PreviewRename')}
              </MenuItem>

              <MenuItem
                isDisabled={!romFileCount}
                onPress={handleManageEpisodesPress}
              >
                <Icon
                  className={styles.actionMenuIcon}
                  name={icons.ROM_FILE}
                />

                {translate('ManageEpisodes')}
              </MenuItem>

              <MenuItem
                isDisabled={!totalRomCount}
                onPress={handleHistoryPress}
              >
                <Icon className={styles.actionMenuIcon} name={icons.HISTORY} />

                {translate('History')}
              </MenuItem>
            </MenuContent>
          </Menu>
        ) : (
          <div className={styles.actions}>
            <SpinnerIconButton
              className={styles.actionButton}
              name={icons.SEARCH}
              title={
                hasMonitoredEpisodes && seriesMonitored
                  ? translate('SearchForMonitoredEpisodesSeason')
                  : translate('NoMonitoredEpisodesSeason')
              }
              size={24}
              isSpinning={isSearching}
              isDisabled={
                isSearching || !hasMonitoredEpisodes || !seriesMonitored
              }
              onPress={handleSearchPress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.INTERACTIVE}
              title={translate('InteractiveSearchSeason')}
              size={24}
              isDisabled={!totalRomCount}
              onPress={handleInteractiveSearchPress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.ORGANIZE}
              title={translate('PreviewRenameSeason')}
              size={24}
              isDisabled={!romFileCount}
              onPress={handleOrganizePress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.ROM_FILE}
              title={translate('ManageEpisodesSeason')}
              size={24}
              isDisabled={!romFileCount}
              onPress={handleManageEpisodesPress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.HISTORY}
              title={translate('HistorySeason')}
              size={24}
              isDisabled={!totalRomCount}
              onPress={handleHistoryPress}
            />
          </div>
        )}
      </div>

      <div>
        {isExpanded ? (
          <div className={styles.roms}>
            {items.length ? (
              <Table
                columns={columns}
                sortKey={sortKey}
                sortDirection={sortDirection}
                onSortPress={handleSortPress}
                onTableOptionChange={handleTableOptionChange}
              >
                <TableBody>
                  {items.map((item) => {
                    return (
                      <RomRow
                        key={item.id}
                        columns={columns}
                        {...item}
                        isSaving={
                          isToggling && togglingRomIds.includes(item.id)
                        }
                        onMonitorEpisodePress={handleMonitorEpisodePress}
                      />
                    );
                  })}
                </TableBody>
              </Table>
            ) : (
              <div className={styles.noRoms}>
                {translate('NoEpisodesInThisSeason')}
              </div>
            )}

            <div className={styles.collapseButtonContainer}>
              <IconButton
                iconClassName={styles.collapseButtonIcon}
                name={icons.COLLAPSE}
                size={20}
                title={translate('HideEpisodes')}
                onPress={handleExpandPress}
              />
            </div>
          </div>
        ) : null}
      </div>

      <OrganizePreviewModal
        isOpen={isOrganizeModalOpen}
        gameId={gameId}
        platformNumber={platformNumber}
        onModalClose={handleOrganizeModalClose}
      />

      <InteractiveImportModal
        isOpen={isManageEpisodesOpen}
        gameId={gameId}
        platformNumber={platformNumber}
        title={platformNumberTitle}
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
        isOpen={isHistoryModalOpen}
        gameId={gameId}
        platformNumber={platformNumber}
        onModalClose={handleHistoryModalClose}
      />

      <PlatformInteractiveSearchModal
        isOpen={isInteractiveSearchModalOpen}
        romCount={totalRomCount}
        gameId={gameId}
        platformNumber={platformNumber}
        onModalClose={handleInteractiveSearchModalClose}
      />
    </div>
  );
}

export default GameDetailsPlatform;
