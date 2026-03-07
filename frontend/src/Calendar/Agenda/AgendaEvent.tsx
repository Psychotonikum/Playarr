import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { useQueueItemForEpisode } from 'Activity/Queue/Details/QueueDetailsProvider';
import { useCalendarOptions } from 'Calendar/calendarOptionsStore';
import CalendarEventQueueDetails from 'Calendar/Events/CalendarEventQueueDetails';
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
import styles from './AgendaEvent.css';

interface AgendaEventProps {
  id: number;
  gameId: number;
  romFileId: number;
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
  showDate: boolean;
}

function AgendaEvent(props: AgendaEventProps) {
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
    showDate,
  } = props;

  const game = useSingleGame(gameId)!;
  const romFile = useRomFile(romFileId);
  const queueItem = useQueueItemForEpisode(id);
  const { timeFormat, longDateFormat, enableColorImpairedMode, timeZone } =
    useUiSettingsValues();

  const {
    showRomInformation,
    showFinaleIcon,
    showSpecialIcon,
    showCutoffUnmetIcon,
  } = useCalendarOptions();

  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);

  const startTime = convertToTimezone(airDateUtc, timeZone);
  const endTime = convertToTimezone(airDateUtc, timeZone).add(
    game.runtime,
    'minutes'
  );
  const downloading = !!(queueItem || grabbed);
  const isMonitored = game.monitored && monitored;
  const statusStyle = getStatusStyle(
    hasFile,
    downloading,
    startTime,
    endTime,
    isMonitored
  );
  const missingAbsoluteNumber =
    game.gameType === 'anime' && platformNumber > 0 && !absoluteRomNumber;

  const handlePress = useCallback(() => {
    setIsDetailsModalOpen(true);
  }, []);

  const handleDetailsModalClose = useCallback(() => {
    setIsDetailsModalOpen(false);
  }, []);

  return (
    <div className={styles.event}>
      <Link className={styles.underlay} onPress={handlePress} />

      <div className={styles.overlay}>
        <div className={styles.date}>
          {showDate && startTime.format(longDateFormat)}
        </div>

        <div
          className={classNames(
            styles.eventWrapper,
            styles[statusStyle],
            enableColorImpairedMode && 'colorImpaired'
          )}
        >
          <div className={styles.time}>
            {formatTime(airDateUtc, timeFormat, { timeZone })} -{' '}
            {formatTime(endTime.toISOString(), timeFormat, {
              includeMinuteZero: true,
              timeZone,
            })}
          </div>

          <div className={styles.gameTitle}>{game.title}</div>

          {showRomInformation ? (
            <div className={styles.seasonRomNumber}>
              {platformNumber}x{padNumber(romNumber, 2)}
              {game.gameType === 'anime' && absoluteRomNumber && (
                <span className={styles.absoluteRomNumber}>
                  ({absoluteRomNumber})
                </span>
              )}
              <div className={styles.episodeSeparator}> - </div>
            </div>
          ) : null}

          <div className={styles.romTitle}>
            {showRomInformation ? title : null}
          </div>

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
              <CalendarEventQueueDetails
                platformNumber={platformNumber}
                {...queueItem}
              />
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
          romFile &&
          romFile.qualityCutoffNotMet ? (
            <Icon
              className={styles.statusIcon}
              name={icons.ROM_FILE}
              kind={kinds.WARNING}
              title={translate('QualityCutoffNotMet')}
            />
          ) : null}

          {romNumber === 1 && platformNumber > 0 && (
            <Icon
              className={styles.statusIcon}
              name={icons.INFO}
              kind={kinds.INFO}
              title={
                platformNumber === 1
                  ? translate('SeriesPremiere')
                  : translate('SeasonPremiere')
              }
            />
          )}

          {showFinaleIcon && finaleType ? (
            <Icon
              className={styles.statusIcon}
              name={icons.INFO}
              kind={kinds.WARNING}
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

export default AgendaEvent;
