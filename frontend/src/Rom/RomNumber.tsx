import React from 'react';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { AlternateTitle, GameType } from 'Game/Game';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import filterAlternateTitles from 'Utilities/Game/filterAlternateTitles';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import SceneInfo from './SceneInfo';
import styles from './RomNumber.css';

function getWarningMessage(
  unverifiedSceneNumbering: boolean,
  gameType: GameType | undefined,
  absoluteRomNumber: number | undefined
) {
  const messages = [];

  if (unverifiedSceneNumbering) {
    messages.push(translate('SceneNumberNotVerified'));
  }

  if (gameType === 'anime' && !absoluteRomNumber) {
    messages.push(translate('EpisodeMissingAbsoluteNumber'));
  }

  return messages.join('\n');
}

export interface RomNumberProps {
  platformNumber: number;
  romNumber: number;
  absoluteRomNumber?: number;
  scenePlatformNumber?: number;
  sceneRomNumber?: number;
  sceneAbsoluteRomNumber?: number;
  useSceneNumbering?: boolean;
  unverifiedSceneNumbering?: boolean;
  alternateTitles?: AlternateTitle[];
  gameType?: GameType;
  showPlatformNumber?: boolean;
}

function RomNumber(props: RomNumberProps) {
  const {
    platformNumber,
    romNumber,
    absoluteRomNumber,
    scenePlatformNumber,
    sceneRomNumber,
    sceneAbsoluteRomNumber,
    useSceneNumbering = false,
    unverifiedSceneNumbering = false,
    alternateTitles: seriesAlternateTitles = [],
    gameType,
    showPlatformNumber = false,
  } = props;

  const alternateTitles = filterAlternateTitles(
    seriesAlternateTitles,
    null,
    useSceneNumbering,
    platformNumber,
    scenePlatformNumber
  );

  const hasSceneInformation =
    scenePlatformNumber !== undefined ||
    sceneRomNumber !== undefined ||
    (gameType === 'anime' && sceneAbsoluteRomNumber !== undefined) ||
    !!alternateTitles.length;

  const warningMessage = getWarningMessage(
    unverifiedSceneNumbering,
    gameType,
    absoluteRomNumber
  );

  return (
    <span>
      {hasSceneInformation ? (
        <Popover
          anchor={
            <span>
              {showPlatformNumber && platformNumber != null && (
                <>{platformNumber}x</>
              )}

              {showPlatformNumber ? padNumber(romNumber, 2) : romNumber}

              {gameType === 'anime' && !!absoluteRomNumber && (
                <span className={styles.absoluteRomNumber}>
                  ({absoluteRomNumber})
                </span>
              )}
            </span>
          }
          title={translate('SceneInformation')}
          body={
            <SceneInfo
              platformNumber={platformNumber}
              romNumber={romNumber}
              scenePlatformNumber={scenePlatformNumber}
              sceneRomNumber={sceneRomNumber}
              sceneAbsoluteRomNumber={sceneAbsoluteRomNumber}
              alternateTitles={alternateTitles}
              gameType={gameType}
            />
          }
          position={tooltipPositions.RIGHT}
        />
      ) : (
        <span>
          {showPlatformNumber && platformNumber != null && (
            <>{platformNumber}x</>
          )}

          {showPlatformNumber ? padNumber(romNumber, 2) : romNumber}

          {gameType === 'anime' && !!absoluteRomNumber && (
            <span className={styles.absoluteRomNumber}>
              ({absoluteRomNumber})
            </span>
          )}
        </span>
      )}

      {warningMessage ? (
        <Icon
          className={styles.warning}
          name={icons.WARNING}
          kind={kinds.WARNING}
          title={warningMessage}
        />
      ) : null}
    </span>
  );
}

export default RomNumber;
