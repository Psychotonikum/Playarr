import React, { useMemo } from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import { AlternateTitle } from 'Game/Game';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import styles from './SceneInfo.css';

interface SceneInfoProps {
  platformNumber?: number;
  romNumber?: number;
  scenePlatformNumber?: number;
  sceneRomNumber?: number;
  sceneAbsoluteRomNumber?: number;
  alternateTitles: AlternateTitle[];
  gameType?: string;
}

function SceneInfo(props: SceneInfoProps) {
  const {
    platformNumber,
    romNumber,
    scenePlatformNumber,
    sceneRomNumber,
    sceneAbsoluteRomNumber,
    alternateTitles,
    gameType,
  } = props;

  const groupedAlternateTitles = useMemo(() => {
    const reducedAlternateTitles = alternateTitles.map((alternateTitle) => {
      let suffix = '';

      const altScenePlatformNumber =
        scenePlatformNumber === undefined
          ? platformNumber
          : scenePlatformNumber;
      const altSceneRomNumber =
        sceneRomNumber === undefined ? romNumber : sceneRomNumber;

      const mappingPlatformNumber =
        alternateTitle.sceneOrigin === 'igdb'
          ? platformNumber
          : altScenePlatformNumber;
      const altPlatformNumber =
        alternateTitle.scenePlatformNumber !== -1 &&
        alternateTitle.scenePlatformNumber !== undefined
          ? alternateTitle.scenePlatformNumber
          : mappingPlatformNumber;
      const altRomNumber =
        alternateTitle.sceneOrigin === 'igdb' ? romNumber : altSceneRomNumber;

      if (altRomNumber !== altSceneRomNumber) {
        suffix = `S${padNumber(altPlatformNumber as number, 2)}E${padNumber(
          altRomNumber as number,
          2
        )}`;
      } else if (altPlatformNumber !== altScenePlatformNumber) {
        suffix = `S${padNumber(altPlatformNumber as number, 2)}`;
      }

      return {
        alternateTitle,
        title: alternateTitle.title,
        suffix,
        comment: alternateTitle.comment,
      };
    });

    return Object.values(
      reducedAlternateTitles.reduce(
        (
          acc: Record<
            string,
            { title: string; suffix: string; comment: string }
          >,
          alternateTitle
        ) => {
          const key = alternateTitle.suffix
            ? `${alternateTitle.title} ${alternateTitle.suffix}`
            : alternateTitle.title;
          const item = acc[key];

          if (item) {
            item.comment = alternateTitle.comment
              ? `${item.comment}/${alternateTitle.comment}`
              : item.comment;
          } else {
            acc[key] = {
              title: alternateTitle.title,
              suffix: alternateTitle.suffix,
              comment: alternateTitle.comment ?? '',
            };
          }

          return acc;
        },
        {}
      )
    );
  }, [
    alternateTitles,
    platformNumber,
    romNumber,
    scenePlatformNumber,
    sceneRomNumber,
  ]);

  return (
    <DescriptionList className={styles.descriptionList}>
      {scenePlatformNumber === undefined ? null : (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Platform')}
          data={scenePlatformNumber}
        />
      )}

      {sceneRomNumber === undefined ? null : (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Rom')}
          data={sceneRomNumber}
        />
      )}

      {gameType === 'anime' && sceneAbsoluteRomNumber !== undefined ? (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Absolute')}
          data={sceneAbsoluteRomNumber}
        />
      ) : null}

      {alternateTitles.length ? (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={
            groupedAlternateTitles.length === 1
              ? translate('Title')
              : translate('Titles')
          }
          data={
            <div>
              {groupedAlternateTitles.map(({ title, suffix, comment }) => {
                return (
                  <div key={`${title} ${suffix}`}>
                    {title}
                    {suffix && <span> ({suffix})</span>}
                    {comment ? (
                      <span className={styles.comment}> {comment}</span>
                    ) : null}
                  </div>
                );
              })}
            </div>
          }
        />
      ) : null}
    </DescriptionList>
  );
}

export default SceneInfo;
