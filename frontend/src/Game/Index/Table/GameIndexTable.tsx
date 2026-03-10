import React, { RefObject, useEffect, useMemo, useRef } from 'react';
import { FixedSizeList, ListChildComponentProps } from 'react-window';
import Column from 'Components/Table/Column';
import VirtualTable from 'Components/Table/VirtualTable';
import Game from 'Game/Game';
import { useGameOption, useGameTableOptions } from 'Game/gameOptionsStore';
import { SortDirection } from 'Helpers/Props/sortDirections';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import GameIndexRow from './GameIndexRow';
import GameIndexTableHeader from './GameIndexTableHeader';
import styles from './GameIndexTable.css';

interface RowItemData {
  items: Game[];
  sortKey: string;
  columns: Column[];
  isSelectMode: boolean;
}

interface GameIndexTableProps {
  items: Game[];
  sortKey: string;
  sortDirection?: SortDirection;
  jumpToCharacter?: string;
  scrollTop?: number;
  scrollerRef: RefObject<HTMLElement>;
  isSelectMode: boolean;
  isSmallScreen: boolean;
}

function Row({ index, style, data }: ListChildComponentProps<RowItemData>) {
  const { items, sortKey, columns, isSelectMode } = data;

  if (index >= items.length) {
    return null;
  }

  const game = items[index];

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'space-between',
        ...style,
      }}
      className={styles.row}
    >
      <GameIndexRow
        gameId={game.id}
        sortKey={sortKey}
        columns={columns}
        isSelectMode={isSelectMode}
      />
    </div>
  );
}

function GameIndexTable({
  items,
  sortKey,
  sortDirection,
  jumpToCharacter,
  isSelectMode,
  isSmallScreen,
  scrollerRef,
}: GameIndexTableProps) {
  const columns = useGameOption('columns');
  const { showBanners } = useGameTableOptions();
  const listRef = useRef<FixedSizeList<RowItemData>>(null);

  const rowHeight = useMemo(() => {
    return showBanners ? 70 : 38;
  }, [showBanners]);

  useEffect(() => {
    if (jumpToCharacter) {
      const index = getIndexOfFirstCharacter(items, jumpToCharacter);

      if (index != null) {
        let scrollTop = index * rowHeight;

        // If the offset is zero go to the top, otherwise offset
        // by the approximate size of the header + padding (37 + 20).
        if (scrollTop > 0) {
          const offset = 57;

          scrollTop += offset;
        }

        listRef.current?.scrollTo(scrollTop);
        scrollerRef?.current?.scrollTo(0, scrollTop);
      }
    }
  }, [jumpToCharacter, rowHeight, items, scrollerRef, listRef]);

  return (
    <VirtualTable
      Header={
        <GameIndexTableHeader
          showBanners={showBanners}
          columns={columns}
          sortKey={sortKey}
          sortDirection={sortDirection}
          isSelectMode={isSelectMode}
        />
      }
      itemCount={items.length}
      itemData={{
        items,
        sortKey,
        columns,
        isSelectMode,
      }}
      isSmallScreen={isSmallScreen}
      listRef={listRef}
      rowHeight={rowHeight}
      Row={Row}
      scrollerRef={scrollerRef}
    />
  );
}

export default GameIndexTable;
