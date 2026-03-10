import React from 'react';
import FieldSet from 'Components/FieldSet';
import GameTitleLink from 'Game/GameTitleLink';
import RomFormats from 'Rom/RomFormats';
import translate from 'Utilities/String/translate';
import { ParseModel } from './ParseModel';
import ParseResultItem from './ParseResultItem';
import styles from './ParseResult.css';

interface ParseResultProps {
  item: ParseModel;
}

function ParseResult(props: ParseResultProps) {
  const { item } = props;
  const {
    customFormats,
    customFormatScore,
    roms,
    languages,
    parsedRomInfo,
    game,
  } = item;

  const {
    releaseTitle,
    gameTitle,
    gameTitleInfo,
    releaseGroup,
    releaseHash,
    platformNumber,
    romNumbers,
    absoluteRomNumbers,
    special,
    fullSeason,
    isMultiSeason,
    isPartialSeason,
    isDaily,
    airDate,
    quality,
  } = parsedRomInfo;

  const finalLanguages = languages ?? parsedRomInfo.languages;

  return (
    <div>
      <FieldSet legend={translate('Release')}>
        <ParseResultItem
          title={translate('ReleaseTitle')}
          data={releaseTitle}
        />

        <ParseResultItem title={translate('GameTitle')} data={gameTitle} />

        <ParseResultItem
          title={translate('Year')}
          data={gameTitleInfo.year > 0 ? gameTitleInfo.year : '-'}
        />

        <ParseResultItem
          title={translate('AllTitles')}
          data={
            gameTitleInfo.allTitles?.length > 0
              ? gameTitleInfo.allTitles.join(', ')
              : '-'
          }
        />

        <ParseResultItem
          title={translate('ReleaseGroup')}
          data={releaseGroup ?? '-'}
        />

        <ParseResultItem
          title={translate('ReleaseHash')}
          data={releaseHash ? releaseHash : '-'}
        />
      </FieldSet>

      <FieldSet legend={translate('RomInfo')}>
        <div className={styles.container}>
          <div className={styles.column}>
            <ParseResultItem
              title={translate('PlatformNumber')}
              data={
                platformNumber === 0 && absoluteRomNumbers.length
                  ? '-'
                  : platformNumber
              }
            />

            <ParseResultItem
              title={translate('RomNumbers')}
              data={romNumbers.join(', ') || '-'}
            />

            <ParseResultItem
              title={translate('AbsoluteRomNumbers')}
              data={
                absoluteRomNumbers.length ? absoluteRomNumbers.join(', ') : '-'
              }
            />

            <ParseResultItem
              title={translate('Daily')}
              data={isDaily ? 'True' : 'False'}
            />

            <ParseResultItem
              title={translate('AirDate')}
              data={airDate ?? '-'}
            />
          </div>

          <div className={styles.column}>
            <ParseResultItem
              title={translate('Special')}
              data={special ? translate('True') : translate('False')}
            />

            <ParseResultItem
              title={translate('FullSeason')}
              data={fullSeason ? translate('True') : translate('False')}
            />

            <ParseResultItem
              title={translate('MultiSeason')}
              data={isMultiSeason ? translate('True') : translate('False')}
            />

            <ParseResultItem
              title={translate('PartialSeason')}
              data={isPartialSeason ? translate('True') : translate('False')}
            />
          </div>
        </div>
      </FieldSet>

      <FieldSet legend={translate('Quality')}>
        <div className={styles.container}>
          <div className={styles.column}>
            <ParseResultItem
              title={translate('Quality')}
              data={quality.quality.name}
            />
            <ParseResultItem
              title={translate('Proper')}
              data={
                quality.revision.version > 1 && !quality.revision.isRepack
                  ? translate('True')
                  : '-'
              }
            />

            <ParseResultItem
              title={translate('Repack')}
              data={quality.revision.isRepack ? translate('True') : '-'}
            />
          </div>

          <div className={styles.column}>
            <ParseResultItem
              title={translate('Version')}
              data={
                quality.revision.version > 1 ? quality.revision.version : '-'
              }
            />

            <ParseResultItem
              title={translate('Real')}
              data={quality.revision.real ? translate('True') : '-'}
            />
          </div>
        </div>
      </FieldSet>

      <FieldSet legend={translate('Languages')}>
        <ParseResultItem
          title={translate('Languages')}
          data={finalLanguages.map((l) => l.name).join(', ')}
        />
      </FieldSet>

      <FieldSet legend={translate('Details')}>
        <ParseResultItem
          title={translate('MatchedToSeries')}
          data={
            game ? (
              <GameTitleLink titleSlug={game.titleSlug} title={game.title} />
            ) : (
              '-'
            )
          }
        />

        <ParseResultItem
          title={translate('MatchedToSeason')}
          data={roms.length ? roms[0].platformNumber : '-'}
        />

        <ParseResultItem
          title={translate('MatchedToEpisodes')}
          data={
            roms.length ? (
              <div>
                {roms.map((e) => {
                  return (
                    <div key={e.id}>
                      {e.romNumber}
                      {game?.gameType === 'anime' && e.absoluteRomNumber
                        ? ` (${e.absoluteRomNumber})`
                        : ''}{' '}
                      {` - ${e.title}`}
                    </div>
                  );
                })}
              </div>
            ) : (
              '-'
            )
          }
        />

        <ParseResultItem
          title={translate('CustomFormats')}
          data={
            customFormats?.length ? <RomFormats formats={customFormats} /> : '-'
          }
        />

        <ParseResultItem
          title={translate('CustomFormatScore')}
          data={customFormatScore}
        />
      </FieldSet>
    </div>
  );
}

export default ParseResult;
