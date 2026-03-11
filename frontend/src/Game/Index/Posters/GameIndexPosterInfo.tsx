import React from 'react';
import GameTagList from 'Components/GameTagList';
import HeartRating from 'Components/HeartRating';
import { Ratings } from 'Game/Game';
import Language from 'Language/Language';
import { QualityProfileModel } from 'Settings/Profiles/Quality/useQualityProfiles';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './GameIndexPosterInfo.css';

interface GameIndexPosterInfoProps {
  originalLanguage?: Language;
  showQualityProfile: boolean;
  qualityProfile?: QualityProfileModel;
  added?: string;
  platformCount: number;
  path: string;
  sizeOnDisk?: number;
  ratings: Ratings;
  tags: number[];
  sortKey: string;
  showRelativeDates: boolean;
  shortDateFormat: string;
  longDateFormat: string;
  timeFormat: string;
  showTags: boolean;
}

function GameIndexPosterInfo(props: GameIndexPosterInfoProps) {
  const {
    originalLanguage,
    qualityProfile,
    showQualityProfile,
    added,
    platformCount,
    path,
    sizeOnDisk = 0,
    ratings,
    tags,
    sortKey,
    showRelativeDates,
    shortDateFormat,
    longDateFormat,
    timeFormat,
    showTags,
  } = props;

  if (sortKey === 'originalLanguage' && !!originalLanguage?.name) {
    return (
      <div className={styles.info} title={translate('OriginalLanguage')}>
        {originalLanguage.name}
      </div>
    );
  }

  if (
    sortKey === 'qualityProfileId' &&
    !showQualityProfile &&
    !!qualityProfile?.name
  ) {
    return (
      <div className={styles.info} title={translate('QualityProfile')}>
        {qualityProfile.name}
      </div>
    );
  }

  if (sortKey === 'added' && added) {
    const addedDate = getRelativeDate({
      date: added,
      shortDateFormat,
      showRelativeDates,
      timeFormat,
      timeForToday: false,
    });

    return (
      <div
        className={styles.info}
        title={formatDateTime(added, longDateFormat, timeFormat)}
      >
        {translate('Added')}: {addedDate}
      </div>
    );
  }

  if (sortKey === 'platformCount') {
    let platforms = translate('OneSeason');

    if (platformCount === 0) {
      platforms = translate('NoSeasons');
    } else if (platformCount > 1) {
      platforms = translate('CountSeasons', { count: platformCount });
    }

    return <div className={styles.info}>{platforms}</div>;
  }

  if (!showTags && sortKey === 'tags' && tags.length) {
    return (
      <div className={styles.tags}>
        <div className={styles.tagsList}>
          <GameTagList tags={tags} />
        </div>
      </div>
    );
  }

  if (sortKey === 'path') {
    return (
      <div className={styles.info} title={translate('Path')}>
        {path}
      </div>
    );
  }

  if (sortKey === 'sizeOnDisk') {
    return (
      <div className={styles.info} title={translate('SizeOnDisk')}>
        {formatBytes(sizeOnDisk)}
      </div>
    );
  }

  if (sortKey === 'ratings' && ratings.value) {
    return (
      <div className={styles.info} title={translate('Rating')}>
        <HeartRating rating={ratings.value} votes={ratings.votes} />
      </div>
    );
  }

  return null;
}

export default GameIndexPosterInfo;
