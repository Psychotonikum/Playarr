import React, { RefObject, useCallback, useRef } from 'react';
import { FixedSizeList, ListChildComponentProps } from 'react-window';
import { useAppDimension } from 'App/appStore';
import { useSelect } from 'App/Select/SelectContext';
import VirtualTable from 'Components/Table/VirtualTable';
import { CheckInputChanged } from 'typings/inputs';
import ImportGameHeader from './ImportGameHeader';
import ImportGameRow from './ImportGameRow';
import {
  UnamppedFolderItem,
  useEnsureImportGameItems,
} from './importGameStore';
import styles from './ImportGameTable.css';

const ROW_HEIGHT = 52;

interface RowItemData {
  items: UnamppedFolderItem[];
}

interface ImportGameTableProps {
  items: UnamppedFolderItem[];
  scrollerRef: RefObject<HTMLElement>;
}

function Row({ index, style, data }: ListChildComponentProps<RowItemData>) {
  const { items } = data;

  if (index >= items.length) {
    return null;
  }

  const item = items[index];

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'space-between',
        ...style,
      }}
      className={styles.row}
    >
      <ImportGameRow key={item.id} unmappedFolder={item} />
    </div>
  );
}

function ImportGameTable({ items, scrollerRef }: ImportGameTableProps) {
  const isSmallScreen = useAppDimension('isSmallScreen');
  const { allSelected, allUnselected, selectAll, unselectAll, useHasItems } =
    useSelect();

  const listRef = useRef<FixedSizeList<RowItemData>>(null);

  const handleSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      if (value) {
        selectAll();
      } else {
        unselectAll();
      }
    },
    [selectAll, unselectAll]
  );

  const hasSelectItems = useHasItems();

  useEnsureImportGameItems(items);

  if (!items.length || !hasSelectItems) {
    return null;
  }

  return (
    <VirtualTable
      Header={
        <ImportGameHeader
          allSelected={allSelected}
          allUnselected={allUnselected}
          onSelectAllChange={handleSelectAllChange}
        />
      }
      itemCount={items.length}
      itemData={{
        items,
      }}
      isSmallScreen={isSmallScreen}
      listRef={listRef}
      rowHeight={ROW_HEIGHT}
      Row={Row}
      scrollerRef={scrollerRef}
    />
  );
}

export default ImportGameTable;
