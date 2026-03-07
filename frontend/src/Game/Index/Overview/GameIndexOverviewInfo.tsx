import React, { useMemo } from 'react';
import { IconName } from 'Components/Icon';
import { icons } from 'Helpers/Props';
import { QualityProfileModel } from 'Settings/Profiles/Quality/useQualityProfiles';
import {
  UiSettingsModel,
  useUiSettingsValues,
} from 'Settings/UI/useUiSettings';
import dimensions from 'Styles/Variables/dimensions';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import GameIndexOverviewInfoRow from './GameIndexOverviewInfoRow';
import styles from './GameIndexOverviewInfo.css';

interface RowProps {
  name: string;
  showProp: string;
  valueProp: string;
}

interface RowInfoProps {
  title: string;
  iconName: IconName;
  label: string;
}

interface GameIndexOverviewInfoProps {
  height: number;
  showNetwork: boolean;
  showMonitored: boolean;
  showQualityProfile: boolean;
  showPreviousAiring: boolean;
  showAdded: boolean;
  showPlatformCount: boolean;
  showPath: boolean;
  showSizeOnDisk: boolean;
  monitored: boolean;
  nextAiring?: string;
  network?: string;
  qualityProfile?: QualityProfileModel;
  previousAiring?: string;
  added?: string;
  platformCount: number;
  path: string;
  sizeOnDisk?: number;
  sortKey: string;
}

const infoRowHeight = parseInt(dimensions.seriesIndexOverviewInfoRowHeight);

const rows = [
  {
    name: 'monitored',
    showProp: 'showMonitored',
    valueProp: 'monitored',
  },
  {
    name: 'network',
    showProp: 'showNetwork',
    valueProp: 'network',
  },
  {
    name: 'qualityProfileId',
    showProp: 'showQualityProfile',
    valueProp: 'qualityProfile',
  },
  {
    name: 'previousAiring',
    showProp: 'showPreviousAiring',
    valueProp: 'previousAiring',
  },
  {
    name: 'added',
    showProp: 'showAdded',
    valueProp: 'added',
  },
  {
    name: 'platformCount',
    showProp: 'showPlatformCount',
    valueProp: 'platformCount',
  },
  {
    name: 'path',
    showProp: 'showPath',
    valueProp: 'path',
  },
  {
    name: 'sizeOnDisk',
    showProp: 'showSizeOnDisk',
    valueProp: 'sizeOnDisk',
  },
];

function getInfoRowProps(
  row: RowProps,
  props: GameIndexOverviewInfoProps,
  uiSettings: UiSettingsModel
): RowInfoProps | null {
  const { name } = row;

  if (name === 'monitored') {
    const monitoredText = props.monitored
      ? translate('Monitored')
      : translate('Unmonitored');

    return {
      title: monitoredText,
      iconName: props.monitored ? icons.MONITORED : icons.UNMONITORED,
      label: monitoredText,
    };
  }

  if (name === 'network') {
    return {
      title: translate('Network'),
      iconName: icons.NETWORK,
      label: props.network ?? '',
    };
  }

  if (name === 'qualityProfileId' && !!props.qualityProfile?.name) {
    return {
      title: translate('QualityProfile'),
      iconName: icons.PROFILE,
      label: props.qualityProfile.name,
    };
  }

  if (name === 'previousAiring') {
    const previousAiring = props.previousAiring;
    const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
      uiSettings;

    return {
      title: translate('PreviousAiringDate', {
        date: formatDateTime(previousAiring, longDateFormat, timeFormat),
      }),
      iconName: icons.CALENDAR,
      label: getRelativeDate({
        date: previousAiring,
        shortDateFormat,
        showRelativeDates,
        timeFormat,
        timeForToday: true,
      }),
    };
  }

  if (name === 'added') {
    const added = props.added;
    const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
      uiSettings;

    return {
      title: translate('AddedDate', {
        date: formatDateTime(added, longDateFormat, timeFormat),
      }),
      iconName: icons.ADD,
      label:
        getRelativeDate({
          date: added,
          shortDateFormat,
          showRelativeDates,
          timeFormat,
          timeForToday: true,
        }) ?? '',
    };
  }

  if (name === 'platformCount') {
    const { platformCount } = props;
    let platforms = translate('OneSeason');

    if (platformCount === 0) {
      platforms = translate('NoSeasons');
    } else if (platformCount > 1) {
      platforms = translate('CountSeasons', { count: platformCount });
    }

    return {
      title: translate('PlatformCount'),
      iconName: icons.CIRCLE,
      label: platforms,
    };
  }

  if (name === 'path') {
    return {
      title: translate('Path'),
      iconName: icons.FOLDER,
      label: props.path,
    };
  }

  if (name === 'sizeOnDisk') {
    const { sizeOnDisk = 0 } = props;

    return {
      title: translate('SizeOnDisk'),
      iconName: icons.DRIVE,
      label: formatBytes(sizeOnDisk),
    };
  }

  return null;
}

function GameIndexOverviewInfo(props: GameIndexOverviewInfoProps) {
  const { height, nextAiring } = props;

  const uiSettings = useUiSettingsValues();
  const { shortDateFormat, showRelativeDates, longDateFormat, timeFormat } =
    uiSettings;

  let shownRows = 1;
  const maxRows = Math.floor(height / (infoRowHeight + 4));

  const rowInfo = useMemo(() => {
    return rows.map((row) => {
      const { name, showProp, valueProp } = row;

      const isVisible =
        // eslint-disable-next-line @typescript-eslint/ban-ts-comment
        // @ts-ignore ts(7053)
        props[valueProp] != null && (props[showProp] || props.sortKey === name);

      return {
        ...row,
        isVisible,
      };
    });
  }, [props]);

  return (
    <div className={styles.infos}>
      {!!nextAiring && (
        <GameIndexOverviewInfoRow
          title={translate('NextAiringDate', {
            date: formatDateTime(nextAiring, longDateFormat, timeFormat),
          })}
          iconName={icons.SCHEDULED}
          label={getRelativeDate({
            date: nextAiring,
            shortDateFormat,
            showRelativeDates,
            timeFormat,
            timeForToday: true,
          })}
        />
      )}

      {rowInfo.map((row) => {
        if (!row.isVisible) {
          return null;
        }

        if (shownRows >= maxRows) {
          return null;
        }

        shownRows++;

        const infoRowProps = getInfoRowProps(row, props, uiSettings);

        if (infoRowProps == null) {
          return null;
        }

        return <GameIndexOverviewInfoRow key={row.name} {...infoRowProps} />;
      })}
    </div>
  );
}

export default GameIndexOverviewInfo;
