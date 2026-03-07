import React, { useCallback, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import { icons, kinds, sizes } from 'Helpers/Props';
import InteractiveSearch from 'InteractiveSearch/InteractiveSearch';
import useReleases from 'InteractiveSearch/useReleases';
import translate from 'Utilities/String/translate';
import styles from './RomSearch.css';

interface EpisodeSearchProps {
  romId: number;
  startInteractiveSearch: boolean;
  onModalClose: () => void;
}

function EpisodeSearch({
  romId,
  startInteractiveSearch,
  onModalClose,
}: EpisodeSearchProps) {
  const executeCommand = useExecuteCommand();
  const { isFetched } = useReleases({ romId });

  const [isInteractiveSearchOpen, setIsInteractiveSearchOpen] = useState(
    startInteractiveSearch || isFetched
  );

  const handleQuickSearchPress = useCallback(() => {
    executeCommand({
      name: CommandNames.EpisodeSearch,
      romIds: [romId],
    });

    onModalClose();
  }, [romId, executeCommand, onModalClose]);

  const handleInteractiveSearchPress = useCallback(() => {
    setIsInteractiveSearchOpen(true);
  }, []);

  if (isInteractiveSearchOpen) {
    return <InteractiveSearch type="rom" searchPayload={{ romId }} />;
  }

  return (
    <div>
      <div className={styles.buttonContainer}>
        <Button
          className={styles.button}
          size={sizes.LARGE}
          onPress={handleQuickSearchPress}
        >
          <Icon className={styles.buttonIcon} name={icons.QUICK} />

          {translate('QuickSearch')}
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          className={styles.button}
          kind={kinds.PRIMARY}
          size={sizes.LARGE}
          onPress={handleInteractiveSearchPress}
        >
          <Icon className={styles.buttonIcon} name={icons.INTERACTIVE} />

          {translate('InteractiveSearch')}
        </Button>
      </div>
    </div>
  );
}

export default EpisodeSearch;
