import { orderBy } from 'lodash';
import React, { useCallback, useMemo } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, kinds } from 'Helpers/Props';
import Game from 'Game/Game';
import useGame from 'Game/useGame';
import translate from 'Utilities/String/translate';
import styles from './OrganizeGameModalContent.css';

export interface OrganizeGameModalContentProps {
  onModalClose: () => void;
}

function OrganizeGameModalContent({
  onModalClose,
}: OrganizeGameModalContentProps) {
  const { data: allGames } = useGame();
  const executeCommand = useExecuteCommand();
  const { useSelectedIds } = useSelect<Game>();
  const gameIds = useSelectedIds();

  const gameTitles = useMemo(() => {
    const game = gameIds.reduce((acc: Game[], id) => {
      const s = allGames.find((s) => s.id === id);

      if (s) {
        acc.push(s);
      }

      return acc;
    }, []);

    const sorted = orderBy(game, ['sortTitle']);

    return sorted.map((s) => s.title);
  }, [allGames, gameIds]);

  const onOrganizePress = useCallback(() => {
    executeCommand({
      name: CommandNames.RenameSeries,
      gameIds,
    });

    onModalClose();
  }, [gameIds, onModalClose, executeCommand]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('OrganizeSelectedSeriesModalHeader')}
      </ModalHeader>

      <ModalBody>
        <Alert>
          {translate('OrganizeSelectedSeriesModalAlert')}
          <Icon className={styles.renameIcon} name={icons.ORGANIZE} />
        </Alert>

        <div className={styles.message}>
          {translate('OrganizeSelectedSeriesModalConfirmation', {
            count: gameTitles.length,
          })}
        </div>

        <ul>
          {gameTitles.map((title) => {
            return <li key={title}>{title}</li>;
          })}
        </ul>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.DANGER} onPress={onOrganizePress}>
          {translate('Organize')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default OrganizeGameModalContent;
