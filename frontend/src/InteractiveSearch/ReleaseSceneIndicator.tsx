import classNames from 'classnames';
import _ from 'lodash';
import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { icons, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ReleaseSceneIndicator.css';

function formatReleaseNumber(
  platformNumber: number | undefined,
  romNumbers: number[] | undefined,
  absoluteRomNumbers: number[] | undefined
) {
  if (romNumbers && romNumbers.length) {
    if (romNumbers.length > 1) {
      return `${platformNumber}x${romNumbers[0]}-${
        romNumbers[romNumbers.length - 1]
      }`;
    }
    return `${platformNumber}x${romNumbers[0]}`;
  }

  if (absoluteRomNumbers && absoluteRomNumbers.length) {
    if (absoluteRomNumbers.length > 1) {
      return `${absoluteRomNumbers[0]}-${
        absoluteRomNumbers[absoluteRomNumbers.length - 1]
      }`;
    }
    return absoluteRomNumbers[0];
  }

  if (platformNumber !== undefined) {
    return translate('PlatformNumberToken', { platformNumber });
  }

  return null;
}

interface ReleaseSceneIndicatorProps {
  className: string;
  platformNumber?: number;
  romNumbers?: number[];
  absoluteRomNumbers?: number[];
  scenePlatformNumber?: number;
  sceneRomNumbers?: number[];
  sceneAbsoluteRomNumbers?: number[];
  sceneMapping?: {
    sceneOrigin?: string;
    title?: string;
    comment?: string;
  };
  episodeRequested: boolean;
  isDaily: boolean;
}

function ReleaseSceneIndicator(props: ReleaseSceneIndicatorProps) {
  const {
    className,
    platformNumber,
    romNumbers,
    absoluteRomNumbers,
    scenePlatformNumber,
    sceneRomNumbers,
    sceneAbsoluteRomNumbers,
    sceneMapping = {},
    episodeRequested,
    isDaily,
  } = props;

  const { sceneOrigin, title, comment } = sceneMapping;

  if (isDaily) {
    return null;
  }

  let mappingDifferent =
    scenePlatformNumber !== undefined && platformNumber !== scenePlatformNumber;

  if (sceneRomNumbers !== undefined) {
    mappingDifferent =
      mappingDifferent || !_.isEqual(sceneRomNumbers, romNumbers);
  } else if (sceneAbsoluteRomNumbers !== undefined) {
    mappingDifferent =
      mappingDifferent ||
      !_.isEqual(sceneAbsoluteRomNumbers, absoluteRomNumbers);
  }

  if (!sceneMapping && !mappingDifferent) {
    return null;
  }

  const releaseNumber = formatReleaseNumber(
    scenePlatformNumber,
    sceneRomNumbers,
    sceneAbsoluteRomNumbers
  );
  const mappedNumber = formatReleaseNumber(
    platformNumber,
    romNumbers,
    absoluteRomNumbers
  );
  const messages = [];

  const isMixed = sceneOrigin === 'mixed';
  const isUnknown = sceneOrigin === 'unknown' || sceneOrigin === 'unknown:igdb';

  let level = styles.levelNone;

  if (isMixed) {
    level = styles.levelMixed;
    messages.push(
      <div key="source">
        {translate('ReleaseSceneIndicatorSourceMessage', {
          message: comment ?? 'Source',
        })}
      </div>
    );
  } else if (isUnknown) {
    level = styles.levelUnknown;
    messages.push(
      <div key="unknown">
        {translate('ReleaseSceneIndicatorUnknownMessage')}
      </div>
    );
    if (sceneOrigin === 'unknown') {
      messages.push(
        <div key="origin">
          {translate('ReleaseSceneIndicatorAssumingScene')}.
        </div>
      );
    } else if (sceneOrigin === 'unknown:igdb') {
      messages.push(
        <div key="origin">{translate('ReleaseSceneIndicatorAssumingIgdb')}</div>
      );
    }
  } else if (mappingDifferent) {
    level = styles.levelMapped;
  } else if (sceneOrigin) {
    level = styles.levelNormal;
  }

  if (!episodeRequested) {
    if (!isMixed && !isUnknown) {
      level = styles.levelNotRequested;
    }
    if (mappedNumber) {
      messages.push(
        <div key="not-requested">
          {translate('ReleaseSceneIndicatorMappedNotRequested')}
        </div>
      );
    } else {
      messages.push(
        <div key="unknown-game">
          {translate('ReleaseSceneIndicatorUnknownSeries')}
        </div>
      );
    }
  }

  const table = (
    <DescriptionList className={styles.descriptionList}>
      {comment !== undefined && (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Mapping')}
          data={comment}
        />
      )}

      {title !== undefined && (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Title')}
          data={title}
        />
      )}

      {releaseNumber !== undefined && (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Release')}
          data={releaseNumber ?? 'unknown'}
        />
      )}

      {releaseNumber !== undefined && (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('TheIgdb')}
          data={mappedNumber ?? 'unknown'}
        />
      )}
    </DescriptionList>
  );

  return (
    <Popover
      anchor={
        <div className={classNames(level, styles.container, className)}>
          <Icon name={icons.SCENE_MAPPING} />
        </div>
      }
      title={translate('SceneInfo')}
      body={
        <div>
          {table}
          {(messages.length && (
            <div className={styles.messages}>{messages}</div>
          )) ||
            null}
        </div>
      }
      position={tooltipPositions.RIGHT}
    />
  );
}

export default ReleaseSceneIndicator;
