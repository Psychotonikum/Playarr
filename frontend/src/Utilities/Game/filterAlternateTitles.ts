import { AlternateTitle } from 'Game/Game';

function filterAlternateTitles(
  alternateTitles: AlternateTitle[],
  gameTitle: string | null,
  useSceneNumbering: boolean,
  platformNumber?: number,
  scenePlatformNumber?: number
) {
  const globalTitles: AlternateTitle[] = [];
  const seasonTitles: AlternateTitle[] = [];

  if (alternateTitles) {
    alternateTitles.forEach((alternateTitle) => {
      if (
        alternateTitle.sceneOrigin === 'unknown' ||
        alternateTitle.sceneOrigin === 'unknown:igdb'
      ) {
        return;
      }

      if (alternateTitle.sceneOrigin === 'mixed') {
        // For now filter out 'mixed' from the UI, the user will get an rejection during manual search.
        return;
      }

      const hasAltPlatformNumber =
        alternateTitle.platformNumber !== -1 &&
        alternateTitle.platformNumber !== undefined;
      const hasAltScenePlatformNumber =
        alternateTitle.scenePlatformNumber !== -1 &&
        alternateTitle.scenePlatformNumber !== undefined;

      // Global alias that should be displayed global
      if (
        !hasAltPlatformNumber &&
        !hasAltScenePlatformNumber &&
        alternateTitle.title !== gameTitle &&
        (!alternateTitle.sceneOrigin || !useSceneNumbering)
      ) {
        globalTitles.push(alternateTitle);
        return;
      }

      // Global alias that should be displayed per rom
      if (
        !hasAltPlatformNumber &&
        !hasAltScenePlatformNumber &&
        alternateTitle.sceneOrigin &&
        useSceneNumbering
      ) {
        seasonTitles.push(alternateTitle);
        return;
      }

      // Apply the alternative mapping (release to scene)
      const mappedAltPlatformNumber = hasAltPlatformNumber
        ? alternateTitle.platformNumber
        : alternateTitle.scenePlatformNumber;
      // Select scene or igdb on the rom
      const mappedPlatformNumber =
        alternateTitle.sceneOrigin === 'igdb'
          ? platformNumber
          : scenePlatformNumber;

      if (
        mappedPlatformNumber !== undefined &&
        mappedPlatformNumber === mappedAltPlatformNumber
      ) {
        seasonTitles.push(alternateTitle);
        return;
      }
    });
  }

  if (platformNumber === undefined) {
    return globalTitles;
  }

  return seasonTitles;
}

export default filterAlternateTitles;
