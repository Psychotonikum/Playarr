import { get } from 'lodash';
import monitorOptions from 'Utilities/Game/monitorOptions';

export default function migrateAddGameDefaults(persistedState) {
  const monitor = get(persistedState, 'addGame.defaults.monitor');

  if (!monitor) {
    return;
  }

  if (!monitorOptions.find((option) => option.key === monitor)) {
    persistedState.addGame.defaults.monitor = monitorOptions[0].key;
  }
}
