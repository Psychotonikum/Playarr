import { keepPreviousData } from '@tanstack/react-query';
import { useEffect } from 'react';
import { Filter } from 'Filters/Filter';
import usePage from 'Helpers/Hooks/usePage';
import usePagedApiQuery from 'Helpers/Hooks/usePagedApiQuery';
import Rom from 'Rom/Rom';
import { setRomQueryKey } from 'Rom/useRom';
import translate from 'Utilities/String/translate';
import { useMissingOptions } from './missingOptionsStore';

export const FILTERS: Filter[] = [
  {
    key: 'monitored',
    label: () => translate('Monitored'),
    filters: [
      {
        key: 'monitored',
        value: [true],
        type: 'equal',
      },
    ],
  },
  {
    key: 'unmonitored',
    label: () => translate('Unmonitored'),
    filters: [
      {
        key: 'monitored',
        value: [false],
        type: 'equal',
      },
    ],
  },
];

const useMissing = () => {
  const { page, goToPage } = usePage('missing');
  const { pageSize, selectedFilterKey, sortKey, sortDirection } =
    useMissingOptions();

  const { isPlaceholderData, queryKey, ...query } = usePagedApiQuery<Rom>({
    path: '/wanted/missing',
    page,
    pageSize,
    queryParams: {
      monitored: selectedFilterKey === 'monitored',
    },
    sortKey,
    sortDirection,
    queryOptions: {
      placeholderData: keepPreviousData,
    },
  });

  useEffect(() => {
    if (!isPlaceholderData) {
      setRomQueryKey('wanted.missing', queryKey);
    }
  }, [isPlaceholderData, queryKey]);

  return {
    ...query,
    goToPage,
    isPlaceholderData,
    page,
  };
};

export default useMissing;
