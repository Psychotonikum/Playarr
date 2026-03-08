import React, { useCallback, useEffect, useState } from 'react';
import Alert from 'Components/Alert';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import useDebounce from 'Helpers/Hooks/useDebounce';
import useQueryParams from 'Helpers/Hooks/useQueryParams';
import { icons, kinds } from 'Helpers/Props';
import { useHasSeries } from 'Game/useGame';
import { InputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import AddNewGameSearchResult from './AddNewGameSearchResult';
import { useLookupSeries } from './useAddGame';
import styles from './AddNewGame.css';

function AddNewGame() {
  const { term: initialTerm = '' } = useQueryParams<{ term: string }>();
  const hasSeries = useHasSeries();
  const [term, setTerm] = useState(initialTerm);
  const [isFetching, setIsFetching] = useState(false);
  const query = useDebounce(term, term ? 300 : 0);

  const handleSearchInputChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setTerm(value);
      setIsFetching(!!value.trim());
    },
    []
  );

  const handleClearGameLookupPress = useCallback(() => {
    setTerm('');
    setIsFetching(false);
  }, []);

  const { isFetching: isFetchingApi, error, data } = useLookupSeries(query);

  useEffect(() => {
    setIsFetching(isFetchingApi);
  }, [isFetchingApi]);

  useEffect(() => {
    setTerm(initialTerm);
  }, [initialTerm]);

  return (
    <PageContent title={translate('AddNewGame')}>
      <PageContentBody>
        <div className={styles.searchContainer}>
          <div className={styles.searchIconContainer}>
            <Icon name={icons.SEARCH} size={20} />
          </div>

          <TextInput
            className={styles.searchInput}
            name="seriesLookup"
            value={term}
            placeholder="eg. Breaking Bad, igdb:####"
            autoFocus={true}
            onChange={handleSearchInputChange}
          />

          <Button
            className={styles.clearLookupButton}
            onPress={handleClearGameLookupPress}
          >
            <Icon name={icons.REMOVE} size={20} />
          </Button>
        </div>

        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <div className={styles.message}>
            <div className={styles.helpText}>
              {translate('AddNewGameError')}
            </div>

            <Alert kind={kinds.DANGER}>{getErrorMessage(error)}</Alert>
          </div>
        ) : null}

        {!isFetching && !error && !!data.length ? (
          <div className={styles.searchResults}>
            {data.map((item) => {
              return (
                <AddNewGameSearchResult key={item.igdbId} game={item} />
              );
            })}
          </div>
        ) : null}

        {!isFetching && !error && !data.length && term ? (
          <div className={styles.message}>
            <div className={styles.noResults}>
              {translate('CouldNotFindResults', { term })}
            </div>
            <div>{translate('SearchByIgdbId')}</div>
            <div>
              <Link to="https://wiki.servarr.com/playarr/faq#why-cant-i-add-a-new-game-when-i-know-the-igdb-id">
                {translate('WhyCantIFindMyShow')}
              </Link>
            </div>
          </div>
        ) : null}

        {term ? null : (
          <div className={styles.message}>
            <div className={styles.helpText}>
              {translate('AddNewGameHelpText')}
            </div>
            <div>{translate('SearchByIgdbId')}</div>
          </div>
        )}

        {!term && !hasSeries ? (
          <div className={styles.message}>
            <div className={styles.noGameText}>
              {translate('NoGameHaveBeenAdded')}
            </div>
            <div>
              <Button to="/add/import" kind={kinds.PRIMARY}>
                {translate('ImportExistingSeries')}
              </Button>
            </div>
          </div>
        ) : null}

        <div />
      </PageContentBody>
    </PageContent>
  );
}

export default AddNewGame;
