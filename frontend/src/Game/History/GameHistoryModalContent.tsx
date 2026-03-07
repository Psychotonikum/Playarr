import React from 'react';
import useGameHistory from 'Activity/History/useGameHistory';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds } from 'Helpers/Props';
import formatPlatform from 'Platform/formatPlatform';
import translate from 'Utilities/String/translate';
import GameHistoryRow from './GameHistoryRow';

const columns: Column[] = [
  {
    name: 'eventType',
    label: '',
    isVisible: true,
  },
  {
    name: 'rom',
    label: () => translate('Rom'),
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

export interface GameHistoryModalContentProps {
  gameId: number;
  platformNumber?: number;
  onModalClose: () => void;
}

function GameHistoryModalContent({
  gameId,
  platformNumber,
  onModalClose,
}: GameHistoryModalContentProps) {
  const { isFetching, isFetched, error, data } = useGameHistory(
    gameId,
    platformNumber
  );

  const fullSeries = platformNumber == null;
  const hasItems = !!data.length;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {platformNumber == null
          ? translate('History')
          : translate('HistoryModalHeaderSeason', {
              platform: formatPlatform(platformNumber)!,
            })}
      </ModalHeader>

      <ModalBody>
        {isFetching && !isFetched ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <Alert kind={kinds.DANGER}>{translate('HistoryLoadError')}</Alert>
        ) : null}

        {isFetched && !hasItems && !error ? (
          <div>{translate('NoHistory')}</div>
        ) : null}

        {isFetched && hasItems && !error ? (
          <Table columns={columns}>
            <TableBody>
              {data.map((item) => {
                return (
                  <GameHistoryRow
                    key={item.id}
                    fullSeries={fullSeries}
                    {...item}
                  />
                );
              })}
            </TableBody>
          </Table>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default GameHistoryModalContent;
