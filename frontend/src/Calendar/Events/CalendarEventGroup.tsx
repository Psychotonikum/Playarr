import classNames from 'classnames';
import React, { useCallback, useMemo, useState } from 'react';
import { useIsDownloadingEpisodes } from 'Activity/Queue/Details/QueueDetailsProvider';
import { useCalendarOptions } from 'Calendar/calendarOptionsStore';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import getFinaleTypeName from 'Rom/getFinaleTypeName';
import { icons, kinds } from 'Helpers/Props';
import { useSingleGame } from 'Game/useGame';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import { CalendarItem } from 'typings/Calendar';
import { convertToTimezone } from 'Utilities/Date/convertToTimezone';
import formatTime from 'Utilities/Date/formatTime';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import CalendarEvent from './CalendarEvent';
import styles from './CalendarEventGroup.css';

interface CalendarEventGroupProps {
  romIds: number[];
  gameId: number;
  events: CalendarItem[];
  onEventModalOpenToggle: (isOpen: boolean) => void;
}

function CalendarEventGroup({
  romIds,
  gameId,
  events,
  onEventModalOpenToggle,
}: CalendarEventGroupProps) {
  const isDownloading = useIsDownloadingEpisodes(romIds);
  const game = useSingleGame(gameId)!;

  const { timeFormat, enableColorImpairedMode, timeZone } =
    useUiSettingsValues();

  const { showRomInformation, showFinaleIcon, fullColorEvents } =
    useCalendarOptions();

  const [isExpanded, setIsExpanded] = useState(false);

  const firstEpisode = events[0];
  const lastEpisode = events[events.length - 1];
  const airDateUtc = firstEpisode.airDateUtc;
  const startTime = convertToTimezone(airDateUtc, timeZone);
  const endTime = convertToTimezone(lastEpisode.airDateUtc, timeZone).add(
    game.runtime,
    'minutes'
  );
  const platformNumber = firstEpisode.platformNumber;

  const { allDownloaded, anyGrabbed, anyMonitored, allAbsoluteRomNumbers } =
    useMemo(() => {
      let files = 0;
      let grabbed = 0;
      let monitored = 0;
      let absoluteRomNumbers = 0;

      events.forEach((event) => {
        if (event.romFileId) {
          files++;
        }

        if (event.grabbed) {
          grabbed++;
        }

        if (game.monitored && event.monitored) {
          monitored++;
        }

        if (event.absoluteRomNumber) {
          absoluteRomNumbers++;
        }
      });

      return {
        allDownloaded: files === events.length,
        anyGrabbed: grabbed > 0,
        anyMonitored: monitored > 0,
        allAbsoluteRomNumbers: absoluteRomNumbers === events.length,
      };
    }, [game, events]);

  const anyDownloading = isDownloading || anyGrabbed;

  const statusStyle = getStatusStyle(
    allDownloaded,
    anyDownloading,
    startTime,
    endTime,
    anyMonitored
  );
  const isMissingAbsoluteNumber =
    game.gameType === 'anime' &&
    platformNumber > 0 &&
    !allAbsoluteRomNumbers;

  const handleExpandPress = useCallback(() => {
    setIsExpanded((state) => !state);
  }, []);

  if (isExpanded) {
    return (
      <div>
        {events.map((event) => {
          return (
            <CalendarEvent
              key={event.id}
              romId={event.id}
              {...event}
              onEventModalOpenToggle={onEventModalOpenToggle}
            />
          );
        })}

        <Link
          className={styles.collapseContainer}
          component="div"
          onPress={handleExpandPress}
        >
          <Icon name={icons.COLLAPSE} />
        </Link>
      </div>
    );
  }

  return (
    <div
      className={classNames(
        styles.eventGroup,
        styles[statusStyle],
        enableColorImpairedMode && 'colorImpaired',
        fullColorEvents && 'fullColor'
      )}
    >
      <div className={styles.info}>
        <div className={styles.gameTitle}>{game.title}</div>

        <div
          className={classNames(
            styles.statusContainer,
            fullColorEvents && 'fullColor'
          )}
        >
          {isMissingAbsoluteNumber ? (
            <Icon
              containerClassName={styles.statusIcon}
              name={icons.WARNING}
              title={translate('EpisodeMissingAbsoluteNumber')}
            />
          ) : null}

          {anyDownloading ? (
            <Icon
              containerClassName={styles.statusIcon}
              name={icons.DOWNLOADING}
              title={translate('AnEpisodeIsDownloading')}
            />
          ) : null}

          {firstEpisode.romNumber === 1 && platformNumber > 0 ? (
            <Icon
              containerClassName={styles.statusIcon}
              name={icons.PREMIERE}
              kind={kinds.INFO}
              title={
                platformNumber === 1
                  ? translate('SeriesPremiere')
                  : translate('SeasonPremiere')
              }
            />
          ) : null}

          {showFinaleIcon && lastEpisode.finaleType ? (
            <Icon
              containerClassName={styles.statusIcon}
              name={
                lastEpisode.finaleType === 'game'
                  ? icons.FINALE_SERIES
                  : icons.FINALE_SEASON
              }
              kind={
                lastEpisode.finaleType === 'game'
                  ? kinds.DANGER
                  : kinds.WARNING
              }
              title={getFinaleTypeName(lastEpisode.finaleType)}
            />
          ) : null}
        </div>
      </div>

      <div className={styles.airingInfo}>
        <div className={styles.airTime}>
          {formatTime(airDateUtc, timeFormat, { timeZone })} -{' '}
          {formatTime(endTime.toISOString(), timeFormat, {
            includeMinuteZero: true,
            timeZone,
          })}
        </div>

        {showRomInformation ? (
          <div className={styles.romInfo}>
            {platformNumber}x{padNumber(firstEpisode.romNumber, 2)}-
            {padNumber(lastEpisode.romNumber, 2)}
            {game.gameType === 'anime' &&
            firstEpisode.absoluteRomNumber &&
            lastEpisode.absoluteRomNumber ? (
              <span className={styles.absoluteRomNumber}>
                ({firstEpisode.absoluteRomNumber}-
                {lastEpisode.absoluteRomNumber})
              </span>
            ) : null}
          </div>
        ) : (
          <Link
            className={styles.expandContainerInline}
            component="div"
            onPress={handleExpandPress}
          >
            <Icon name={icons.EXPAND} />
          </Link>
        )}
      </div>

      {showRomInformation ? (
        <Link
          className={styles.expandContainer}
          component="div"
          onPress={handleExpandPress}
        >
          &nbsp;
          <Icon name={icons.EXPAND} />
          &nbsp;
        </Link>
      ) : null}
    </div>
  );
}

export default CalendarEventGroup;
