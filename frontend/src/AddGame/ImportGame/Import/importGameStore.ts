import { useEffect } from 'react';
import { create } from 'zustand';
import { useShallow } from 'zustand/react/shallow';
import { useAddGameOptions } from 'AddGame/addGameOptionsStore';
import { UnmappedFolder } from 'RootFolder/useRootFolders';
import Game, { GameMonitor } from 'Game/Game';

export interface UnamppedFolderItem extends UnmappedFolder {
  id: string;
}

export interface ImportGameItem {
  id: string;
  monitor: GameMonitor;
  path: string;
  relativePath: string;
  selectedSeries?: Game;
  name: string;
  hasSearched: boolean;
}

interface ImportGameState {
  items: Record<string, ImportGameItem>;
  lookupQueue: string[];
  isProcessing: boolean;
}

const defaultState: ImportGameState = {
  items: {},
  lookupQueue: [],
  isProcessing: false,
};

const importGameStore = create<ImportGameState>()(() => defaultState);

export const useEnsureImportGameItems = (
  unmappedFolders: UnamppedFolderItem[]
) => {
  const { monitor } =
    useAddGameOptions();

  useEffect(() => {
    unmappedFolders.forEach((unmappedFolder) => {
      const existingItem =
        importGameStore.getState().items[unmappedFolder.id];

      if (existingItem) {
        return;
      }

      const newItem: ImportGameItem = {
        ...unmappedFolder,
        monitor,
        hasSearched: false,
      };

      importGameStore.setState((state) => ({
        items: {
          ...state.items,
          [unmappedFolder.id]: newItem,
        },
      }));
    });
  }, [unmappedFolders, monitor]);
};

export const updateImportGameItem = (
  itemData: Partial<ImportGameItem> & Pick<ImportGameItem, 'id'>
) => {
  importGameStore.setState((state) => {
    const existingItem = state.items[itemData.id];

    if (existingItem) {
      return {
        items: {
          ...state.items,
          [itemData.id]: {
            ...existingItem,
            ...itemData,
          },
        },
      };
    }

    return state;
  });
};

export const removeImportGameItemByPath = (path: string) => {
  importGameStore.setState((state) => {
    const item = Object.values(state.items).find((i) => i.path === path);

    if (!item) {
      return state;
    }

    const { [item.id]: removed, ...items } = state.items;

    return { items };
  });
};

export const clearImportGame = () => {
  importGameStore.setState(defaultState);
};

export const startProcessing = () => {
  importGameStore.setState((state) => {
    const items = Object.values(state.items).reduce<string[]>((acc, item) => {
      if (!item.hasSearched) {
        acc.push(item.id);
      }

      return acc;
    }, []);

    return { isProcessing: true, lookupQueue: items };
  });
};

export const stopProcessing = () => {
  importGameStore.setState({ isProcessing: false, lookupQueue: [] });
};

export const addToLookupQueue = (id: string) => {
  importGameStore.setState((state) => ({
    lookupQueue: [...state.lookupQueue, id],
  }));
};

export const removeFromLookupQueue = (id: string) => {
  importGameStore.setState((state) => ({
    lookupQueue: state.lookupQueue.filter((queuedId) => queuedId !== id),
  }));
};

export const useIsCurrentLookupQueueItem = (id: string) => {
  return importGameStore((state) => state.lookupQueue[0] === id);
};

export const useIsCurrentedItemQueued = (id: string) => {
  return importGameStore((state) => state.lookupQueue.includes(id));
};

export const useLookupQueueHasItems = () => {
  return importGameStore((state) => state.lookupQueue.length > 0);
};

export const useImportGameItem = (id: string) => {
  return importGameStore((state) => state.items[id]);
};

export const useImportGameItems = () => {
  return importGameStore(useShallow((state) => Object.values(state.items)));
};

export const getImportGameItems = (ids: string[]) => {
  const state = importGameStore.getState();

  return ids.reduce<ImportGameItem[]>((acc, id) => {
    const item = state.items[id];

    if (item != null) {
      acc.push(item);
    }

    return acc;
  }, []);
};
