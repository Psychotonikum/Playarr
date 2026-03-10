import { createSelector, createSelectorCreator, defaultMemoize } from 'reselect';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';

function createUnoptimizedSelector(uiSection) {
  return createSelector(
    createClientSideCollectionSelector('game', uiSection),
    (game) => {
      const items = game.items.map((s) => {
        const {
          id,
          sortTitle
        } = s;

        return {
          id,
          sortTitle
        };
      });

      return {
        ...game,
        items
      };
    }
  );
}

function gameListEqual(a, b) {
  return hasDifferentItemsOrOrder(a, b);
}

const createSeriesEqualSelector = createSelectorCreator(
  defaultMemoize,
  gameListEqual
);

function createGameClientSideCollectionItemsSelector(uiSection) {
  return createSeriesEqualSelector(
    createUnoptimizedSelector(uiSection),
    (game) => game
  );
}

export default createGameClientSideCollectionItemsSelector;
