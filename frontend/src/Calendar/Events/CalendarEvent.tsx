import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { useQueueItemForEpisode } from 'Activity/Queue/Details/QueueDetailsProvider';
import { useCalendarOptions } from 'Calendar/calendarOptionsStore';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import RomDetailsModal from 'Rom/RomDetailsModal';
import romEntities from 'Rom/romEntities';
import getFinaleTypeName from 'Rom/getFinaleTypeName';
import { useRomFile } from 'RomFile/RomFileProvider';
import { icons, kinds } from 'Helpers/Props';
import { useSingleGame } from 'Game/useGame';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import { convertToTimezone } from 'Utilities/Date/convertToTimezone';
import formatTime from 'Utilities/Date/formatTime';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import styles from './CalendarEvent.css';

interface CalendarEventProps {
  id: number;
  romId: number;
  gameId: number;
  romFileId?: number;
  title: string;
  platformNumber: number;
  romNumber: number;
  absoluteRomNumber?: number;
  airDateUtc: string;
  monitored: boolean;
  unverifiedSceneNumbering?: boolean;
  finaleType?: string;
  hasFile: boolean;
  grabbed?: boolean;
  onEventModalOpenToggle: (isOpen: boolean) => void;
}

function CalendarEvent(props: CalendarEventProps) {
  const {
    id,
    gameId,
    romFileId,
    title,
    platformNumber,
    romNumber,
    absoluteRomNumber,
    airDateUtc,
    monitored,
    unverifiedSceneNumbering,
    finaleType,
    hasFile,
    grabbed,
    onEventModalOpenToggle,
  } = props;

  const game = useSingleGame(gameId);
  const romFile = useRomFile(romFileId);
  const queueItem = useQueueItemForEpisode(id);

  const { timeFormat, enableColorImpairedMode, timeZone } =
    useUiSettingsValues();

  const {
    showRomInformation,
    showFinaleIcon,
    showSpecialIcon,
    showCutoffUnmetIcon,
    fullColorEvents,
  } = useCalendarOptions();

  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);

  const handlePress = useCallback(() => {
    setIsDetailsModalOpen(true);
    onEventModalOpenToggle(true);
  }, [onEventModalOpenToggle]);

  const handleDetailsModalClose = useCallback(() => {
    setIsDetailsModalOpen(false);
    onEventModalOpenToggle(false);
  }, [onEventModalOpenToggle]);

  if (!game) {
    return null;
  }

  const startTime = convertToTimezone(airDateUtc, timeZone);
  const endTime = convertToTimezone(airDateUtc, timeZone).add(
    game.runtime,
    'minutes'
  );
  const isDownloading = !!(queueItem || grabbed);
  const isMonitored = game.monitored && monitored;
  const statusStyle = getStatusStyle(
    hasFile,
    isDownloading,
    startTime,
    endTime,
    isMonitored
  );
  const missingAbsoluteNumber =
    game.gameType === 'anime' && platformNumber > 0 && !absoluteRomNumber;

  return (
    <div
      className={classNames(
        styles.event,
        styles[statusStyle],
        enableColorImpairedMode && 'colorImpaired',
        fullColorEvents && 'fullColor'
      )}
    >
      <Link className={styles.underlay} onPress={handlePress} />

      <div className={styles.overlay}>
        <div className={styles.info}>
          <div className={styles.gameTitle}>{game.title}</div>

          <div
            className={classNames(
              styles.statusContainer,
              fullColorEvents && 'fullColor'
            )}
          >
            {missingAbsoluteNumber ? (
              <Icon
                className={styles.statusIcon}
                name={icons.WARNING}
                title={translate('EpisodeMissingAbsoluteNumber')}
              />
            ) : null}

            {unverifiedSceneNumbering && !missingAbsoluteNumber ? (
              <Icon
                className={styles.statusIcon}
                name={icons.WARNING}
                title={translate('SceneNumberNotVerified')}
              />
            ) : null}

            {queueItem ? (
              <span className={styles.statusIcon}>
                <CalendarEventQueueDetails {...queueItem} />
              </span>
            ) : null}

            {!queueItem && grabbed ? (
              <Icon
                className={styles.statusIcon}
                name={icons.DOWNLOADING}
                title={translate('EpisodeIsDownloading')}
              />
            ) : null}

            {showCutoffUnmetIcon &&
            !!romFile &&
            romFile.qualityCutoffNotMet ? (
              <Icon
                className={styles.statusIcon}
                name={icons.ROM_FILE}
                kind={kinds.WARNING}
                title={translate('QualityCutoffNotMet')}
              />
            ) : null}

            {romNumber === 1 && platformNumber > 0 ? (
              <Icon
                className={styles.statusIcon}
                name={icons.PREMIERE}
                kind={kinds.INFO}
                title={
                  platformNumber === 1
                    ? translate('SeriesPremiere')
                    : translate('SeasonPremiere')
                }
              />
            ) : null}

            {showFinaleIcon && finaleType ? (
              <Icon
                className={styles.statusIcon}
                name={
                  finaleType === 'game'
                    ? icons.FINALE_SERIES
                    : icons.FINALE_SEASON
                }
                kind={finaleType === 'game' ? kinds.DANGER : kinds.WARNING}
                title={getFinaleTypeName(finaleType)}
              />
            ) : null}

            {showSpecialIcon && (romNumber === 0 || platformNumber === 0) ? (
              <Icon
                className={styles.statusIcon}
                name={icons.INFO}
                kind={kinds.PINK}
                title={translate('Special')}
              />
            ) : null}
          </div>
        </div>

        {showRomInformation ? (
          <div className={styles.romInfo}>
            <div className={styles.romTitle}>{title}</div>

            <div>
              {platformNumber}x{padNumber(romNumber, 2)}
              {game.gameType === 'anime' && absoluteRomNumber ? (
                <span className={styles.absoluteRomNumber}>
                  ({absoluteRomNumber})
                </span>
              ) : null}
            </div>
          </div>
        ) : null}

        <div className={styles.airTime}>
          {formatTime(airDateUtc, timeFormat, { timeZone })} -{' '}
          {formatTime(endTime.toISOString(), timeFormat, {
            includeMinuteZero: true,
            timeZone,
          })}
        </div>
      </div>

      <RomDetailsModal
        isOpen={isDetailsModalOpen}
        romId={id}
        romEntity={romEntities.CALENDAR}
        gameId={game.id}
        romTitle={title}
        showOpenSeriesButton={true}
        onModalClose={handleDetailsModalClose}
      />
    </div>
  );
}

export default CalendarEvent;
