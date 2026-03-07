import migrateAddGameDefaults from './migrateAddGameDefaults';

export default function migrate(persistedState) {
  migrateAddGameDefaults(persistedState);
}
