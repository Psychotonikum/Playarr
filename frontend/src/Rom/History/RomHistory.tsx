import React from 'react';
import useRomHistory from 'Activity/History/useRomHistory';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import RomHistoryRow from './RomHistoryRow';

const columns: Column[] = [
  {
    name: 'eventType',
    label: '',
    isVisible: true,
  },
  {
    name: 'sourceTitle',
    label: () => translate('SourceTitle'),
    isVisible: true,
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isVisible: true,
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isVisible: true,
  },
  {
    name: 'customFormats',
    label: () => translate('CustomFormats'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: () => translate('CustomFormatScore'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'date',
    label: () => translate('Date'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

interface RomHistoryProps {
  romId: number;
}

function RomHistory({ romId }: RomHistoryProps) {
  const { data, isFetching, isFetched, error } = useRomHistory(romId);

  const hasItems = !!data.length;

  if (isFetching) {
    return <LoadingIndicator />;
  }

  if (!isFetching && !!error) {
    return (
      <Alert kind={kinds.DANGER}>{translate('RomHistoryLoadError')}</Alert>
    );
  }

  if (isFetched && !hasItems && !error) {
    return <Alert kind={kinds.INFO}>{translate('NoRomHistory')}</Alert>;
  }

  if (isFetched && hasItems && !error) {
    return (
      <Table columns={columns}>
        <TableBody>
          {data.map((item) => {
            return <RomHistoryRow key={item.id} {...item} />;
          })}
        </TableBody>
      </Table>
    );
  }

  return null;
}

export default RomHistory;
