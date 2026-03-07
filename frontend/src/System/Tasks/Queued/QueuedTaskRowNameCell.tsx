import React from 'react';
import { CommandBody } from 'Commands/Command';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { useMultipleSeries } from 'Game/useGame';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import styles from './QueuedTaskRowNameCell.css';

function formatTitles(titles: string[]) {
  if (!titles) {
    return null;
  }

  if (titles.length > 11) {
    return (
      <span title={titles.join(', ')}>
        {titles.slice(0, 10).join(', ')}, {titles.length - 10} more
      </span>
    );
  }

  return <span>{titles.join(', ')}</span>;
}

export interface QueuedTaskRowNameCellProps {
  commandName: string;
  body: CommandBody;
  clientUserAgent?: string;
}

export default function QueuedTaskRowNameCell(
  props: QueuedTaskRowNameCellProps
) {
  const { commandName, body, clientUserAgent } = props;
  const gameIds = 'gameIds' in body ? [...body.gameIds] : [];

  if ('gameId' in body && body.gameId) {
    gameIds.push(body.gameId);
  }

  const game = useMultipleSeries(gameIds);
  const sortedSeries = game.sort(sortByProp('sortTitle'));

  return (
    <TableRowCell>
      <span className={styles.commandName}>
        {commandName}
        {sortedSeries.length ? (
          <span> - {formatTitles(sortedSeries.map((s) => s.title))}</span>
        ) : null}
        {'platformNumber' in body && body.platformNumber ? (
          <span>
            {' '}
            {translate('PlatformNumberToken', {
              platformNumber: body.platformNumber,
            })}
          </span>
        ) : null}
      </span>

      {clientUserAgent ? (
        <span
          className={styles.userAgent}
          title={translate('TaskUserAgentTooltip')}
        >
          {translate('From')}: {clientUserAgent}
        </span>
      ) : null}
    </TableRowCell>
  );
}
