import React, { useCallback, useState } from 'react';
import AddGame from 'AddGame/AddGame';
import { useAppDimension } from 'App/appStore';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import MetadataAttribution from 'Components/MetadataAttribution';
import { icons, kinds, sizes } from 'Helpers/Props';
import { Statistics } from 'Game/Game';
import GameGenres from 'Game/GameGenres';
import GamePoster from 'Game/GamePoster';
import useExistingGame from 'Game/useExistingGame';
import translate from 'Utilities/String/translate';
import AddNewGameModal from './AddNewGameModal';
import styles from './AddNewGameSearchResult.css';

interface AddNewGameSearchResultProps {
  game: AddGame;
}

function AddNewGameSearchResult({ game }: AddNewGameSearchResultProps) {
  const {
    igdbId,
    titleSlug,
    title,
    year,
    network,
    originalLanguage,
    genres = [],
    status,
    statistics = {} as Statistics,
    ratings,
    overview,
    gameType,
    images,
    isExcluded,
  } = game;

  const isExistingSeries = useExistingGame(igdbId);
  const isSmallScreen = useAppDimension('isSmallScreen');
  const [isNewAddGameModalOpen, setIsNewAddGameModalOpen] = useState(false);

  const platformCount = statistics.platformCount;
  const handlePress = useCallback(() => {
    setIsNewAddGameModalOpen(true);
  }, []);

  const handleAddGameModalClose = useCallback(() => {
    setIsNewAddGameModalOpen(false);
  }, []);

  const handleTvdbLinkPress = useCallback((event: React.SyntheticEvent) => {
    event.stopPropagation();
  }, []);

  const linkProps = isExistingSeries
    ? { to: `/game/${titleSlug}` }
    : { onPress: handlePress };
  let platforms = translate('OneSeason');

  if (platformCount > 1) {
    platforms = translate('CountSeasons', { count: platformCount });
  }

  return (
    <div className={styles.searchResult}>
      <Link className={styles.underlay} {...linkProps} />

      <div className={styles.overlay}>
        {isSmallScreen ? null : (
          <GamePoster
            className={styles.poster}
            images={images}
            size={250}
            overflow={true}
            lazy={false}
            title={title}
          />
        )}

        <div className={styles.content}>
          <div className={styles.titleRow}>
            <div className={styles.titleContainer}>
              <div className={styles.title}>
                {title}

                {!title.includes(String(year)) && year ? (
                  <span className={styles.year}>({year})</span>
                ) : null}
              </div>
            </div>

            <div className={styles.icons}>
              {isExistingSeries ? (
                <Icon
                  className={styles.alreadyExistsIcon}
                  name={icons.CHECK_CIRCLE}
                  size={36}
                  title={translate('AlreadyInYourLibrary')}
                />
              ) : null}

              {isExcluded ? (
                <Icon
                  className={styles.excludedIcon}
                  name={icons.DANGER}
                  size={36}
                  title={translate('SeriesInImportListExclusions')}
                />
              ) : null}

              <Link
                className={styles.tvdbLink}
                to={`https://www.thetvdb.com/?tab=game&id=${igdbId}`}
                onPress={handleTvdbLinkPress}
              >
                <Icon
                  className={styles.tvdbLinkIcon}
                  name={icons.EXTERNAL_LINK}
                  size={28}
                />
              </Link>
            </div>
          </div>

          <div>
            <Label size={sizes.LARGE}>
              <HeartRating
                rating={ratings.value}
                votes={ratings.votes}
                iconSize={13}
              />
            </Label>

            {originalLanguage?.name ? (
              <Label size={sizes.LARGE}>
                <Icon name={icons.LANGUAGE} size={13} />

                <span className={styles.originalLanguageName}>
                  {originalLanguage.name}
                </span>
              </Label>
            ) : null}

            {network ? (
              <Label size={sizes.LARGE}>
                <Icon name={icons.NETWORK} size={13} />

                <span className={styles.network}>{network}</span>
              </Label>
            ) : null}

            {genres.length > 0 ? (
              <Label size={sizes.LARGE}>
                <Icon name={icons.GENRE} size={13} />
                <GameGenres className={styles.genres} genres={genres} />
              </Label>
            ) : null}

            {platformCount ? <Label size={sizes.LARGE}>{platforms}</Label> : null}

            {status === 'ended' ? (
              <Label kind={kinds.DANGER} size={sizes.LARGE}>
                {translate('Ended')}
              </Label>
            ) : null}

            {status === 'upcoming' ? (
              <Label kind={kinds.INFO} size={sizes.LARGE}>
                {translate('Upcoming')}
              </Label>
            ) : null}
          </div>

          <div className={styles.overview}>{overview}</div>

          <MetadataAttribution />
        </div>
      </div>

      <AddNewGameModal
        isOpen={isNewAddGameModalOpen && !isExistingSeries}
        game={game}
        initialGameType={gameType}
        onModalClose={handleAddGameModalClose}
      />
    </div>
  );
}

export default AddNewGameSearchResult;
