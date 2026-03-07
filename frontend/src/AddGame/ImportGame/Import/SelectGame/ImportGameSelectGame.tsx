import {
  autoUpdate,
  flip,
  FloatingPortal,
  useClick,
  useDismiss,
  useFloating,
  useInteractions,
} from '@floating-ui/react';
import React, { useCallback, useEffect, useState } from 'react';
import { useLookupSeries } from 'AddGame/AddNewGame/useAddGame';
import FormInputButton from 'Components/Form/FormInputButton';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useDebounce from 'Helpers/Hooks/useDebounce';
import { icons, kinds } from 'Helpers/Props';
import useExistingGame from 'Game/useExistingGame';
import { InputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import {
  addToLookupQueue,
  removeFromLookupQueue,
  updateImportGameItem,
  useImportGameItem,
  useIsCurrentedItemQueued,
  useIsCurrentLookupQueueItem,
} from '../importGameStore';
import ImportGameSearchResult from './ImportGameSearchResult';
import ImportGameTitle from './ImportGameTitle';
import styles from './ImportGameSelectGame.css';

interface ImportGameSelectGameProps {
  id: string;
  onInputChange: (input: InputChanged) => void;
}

function ImportGameSelectGame({
  id,
  onInputChange,
}: ImportGameSelectGameProps) {
  const importSeriesItem = useImportGameItem(id);
  const { selectedSeries, name } = importSeriesItem ?? {};
  const isExistingSeries = useExistingGame(selectedSeries?.igdbId);

  const [term, setTerm] = useState(name);
  const [isOpen, setIsOpen] = useState(false);
  const query = useDebounce(term, term ? 300 : 0);
  const isCurrentLookupQueueItem = useIsCurrentLookupQueueItem(id);
  const isQueued = useIsCurrentedItemQueued(id);

  const { isFetching, isFetched, error, data, refetch } = useLookupSeries(
    query,
    isCurrentLookupQueueItem
  );

  const errorMessage = getErrorMessage(error);
  const isLookingUpSeries = isFetching || isQueued;

  const handlePress = useCallback(() => {
    setIsOpen((prevIsOpen) => !prevIsOpen);
  }, []);

  const handleSearchInputChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setTerm(value);
      addToLookupQueue(id);
    },
    [id]
  );

  const handleRefreshPress = useCallback(() => {
    refetch();
  }, [refetch]);

  const handleSeriesSelect = useCallback(
    (igdbId: number) => {
      setIsOpen(false);

      const selectedSeries = data.find((item) => item.igdbId === igdbId)!;

      updateImportGameItem({
        id,
        selectedSeries,
      });

      if (selectedSeries.gameType !== 'standard') {
        onInputChange({
          name: 'gameType',
          value: selectedSeries.gameType,
        });
      }
    },
    [id, data, onInputChange]
  );

  useEffect(() => {
    if (isFetched) {
      updateImportGameItem({
        id,
        hasSearched: isFetched,
        selectedSeries: data[0],
      });

      removeFromLookupQueue(id);
    }
  }, [id, isFetched, data]);

  useEffect(() => {
    setTerm(name);
  }, [name]);

  const { refs, context, floatingStyles } = useFloating({
    middleware: [
      flip({
        crossAxis: false,
        mainAxis: true,
      }),
    ],
    open: isOpen,
    placement: 'bottom',
    whileElementsMounted: autoUpdate,
    onOpenChange: setIsOpen,
  });

  const click = useClick(context);
  const dismiss = useDismiss(context);

  const { getReferenceProps, getFloatingProps } = useInteractions([
    click,
    dismiss,
  ]);

  return (
    <>
      <div ref={refs.setReference} {...getReferenceProps()}>
        <Link className={styles.button} component="div" onPress={handlePress}>
          {isLookingUpSeries && isQueued && !isFetched ? (
            <LoadingIndicator className={styles.loading} size={20} />
          ) : null}

          {isFetched && selectedSeries && isExistingSeries ? (
            <Icon
              className={styles.warningIcon}
              name={icons.WARNING}
              kind={kinds.WARNING}
            />
          ) : null}

          {isFetched && selectedSeries ? (
            <ImportGameTitle
              title={selectedSeries.title}
              year={selectedSeries.year}
              network={selectedSeries.network}
              isExistingSeries={isExistingSeries}
            />
          ) : null}

          {isFetched && !selectedSeries ? (
            <div>
              <Icon
                className={styles.warningIcon}
                name={icons.WARNING}
                kind={kinds.WARNING}
              />

              {translate('NoMatchFound')}
            </div>
          ) : null}

          {!isFetching && !!error ? (
            <div>
              <Icon
                className={styles.warningIcon}
                title={errorMessage}
                name={icons.WARNING}
                kind={kinds.WARNING}
              />

              {translate('SearchFailedError')}
            </div>
          ) : null}

          <div className={styles.dropdownArrowContainer}>
            <Icon name={icons.CARET_DOWN} />
          </div>
        </Link>
      </div>

      {isOpen ? (
        <FloatingPortal id="portal-root">
          <div
            ref={refs.setFloating}
            className={styles.contentContainer}
            style={floatingStyles}
            {...getFloatingProps()}
          >
            {isOpen ? (
              <div className={styles.content}>
                <div className={styles.searchContainer}>
                  <div className={styles.searchIconContainer}>
                    <Icon name={icons.SEARCH} />
                  </div>

                  <TextInput
                    className={styles.searchInput}
                    name={`${name}_textInput`}
                    value={term}
                    onChange={handleSearchInputChange}
                  />

                  <FormInputButton
                    kind={kinds.DEFAULT}
                    spinnerIcon={icons.REFRESH}
                    canSpin={true}
                    isSpinning={isFetching}
                    onPress={handleRefreshPress}
                  >
                    <Icon name={icons.REFRESH} />
                  </FormInputButton>
                </div>

                <div className={styles.results}>
                  {data.map((item) => {
                    return (
                      <ImportGameSearchResult
                        key={item.igdbId}
                        igdbId={item.igdbId}
                        title={item.title}
                        year={item.year}
                        network={item.network}
                        onPress={handleSeriesSelect}
                      />
                    );
                  })}
                </div>
              </div>
            ) : null}
          </div>
        </FloatingPortal>
      ) : null}
    </>
  );
}

export default ImportGameSelectGame;
