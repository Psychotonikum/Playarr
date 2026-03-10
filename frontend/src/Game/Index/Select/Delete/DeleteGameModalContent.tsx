import { orderBy } from 'lodash';
import React, { useCallback, useMemo, useState } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Game from 'Game/Game';
import {
  setGameDeleteOptions,
  useGameDeleteOptions,
} from 'Game/gameOptionsStore';
import useGame, { useBulkDeleteGame } from 'Game/useGame';
import { inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './DeleteGameModalContent.css';

export interface DeleteGameModalContentProps {
  onModalClose(): void;
}

function DeleteGameModalContent({ onModalClose }: DeleteGameModalContentProps) {
  const { addImportListExclusion } = useGameDeleteOptions();
  const { data: allGames } = useGame();
  const { bulkDeleteGame } = useBulkDeleteGame();
  const [deleteFiles, setDeleteFiles] = useState(false);
  const { useSelectedIds } = useSelect<Game>();
  const gameIds = useSelectedIds();

  const game = useMemo((): Game[] => {
    const gameList = gameIds.map((id) => {
      return allGames.find((s) => s.id === id);
    }) as Game[];

    return orderBy(gameList, ['sortTitle']);
  }, [allGames, gameIds]);

  const onDeleteFilesChange = useCallback(
    ({ value }: InputChanged<boolean>) => {
      setDeleteFiles(value);
    },
    [setDeleteFiles]
  );

  const onDeleteOptionChange = useCallback(
    ({ name, value }: { name: string; value: boolean }) => {
      setGameDeleteOptions({
        [name]: value,
      });
    },
    []
  );

  const onDeleteGameConfirmed = useCallback(() => {
    setDeleteFiles(false);

    bulkDeleteGame({
      gameIds,
      deleteFiles,
      addImportListExclusion,
    });

    onModalClose();
  }, [
    deleteFiles,
    addImportListExclusion,
    setDeleteFiles,
    gameIds,
    bulkDeleteGame,
    onModalClose,
  ]);

  const { totalRomFileCount, totalSizeOnDisk } = useMemo(() => {
    return game.reduce(
      (acc, { statistics = {} }) => {
        const { romFileCount = 0, sizeOnDisk = 0 } = statistics;

        acc.totalRomFileCount += romFileCount;
        acc.totalSizeOnDisk += sizeOnDisk;

        return acc;
      },
      {
        totalRomFileCount: 0,
        totalSizeOnDisk: 0,
      }
    );
  }, [game]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('DeleteSelectedSeries')}</ModalHeader>

      <ModalBody>
        <div>
          <FormGroup>
            <FormLabel>{translate('AddListExclusion')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportListExclusion"
              value={addImportListExclusion}
              helpText={translate('AddListExclusionSeriesHelpText')}
              onChange={onDeleteOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>
              {game.length > 1
                ? translate('DeleteGameFolders')
                : translate('DeleteGameFolder')}
            </FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={
                game.length > 1
                  ? translate('DeleteGameFoldersHelpText')
                  : translate('DeleteGameFolderHelpText')
              }
              kind="danger"
              onChange={onDeleteFilesChange}
            />
          </FormGroup>
        </div>

        <div className={styles.message}>
          {deleteFiles
            ? translate('DeleteGameFolderCountWithFilesConfirmation', {
                count: game.length,
              })
            : translate('DeleteGameFolderCountConfirmation', {
                count: game.length,
              })}
        </div>

        <ul>
          {game.map(({ title, path, statistics = {} }) => {
            const { romFileCount = 0, sizeOnDisk = 0 } = statistics;

            return (
              <li key={title}>
                <span>{title}</span>

                {deleteFiles && (
                  <span>
                    <span className={styles.pathContainer}>
                      -<span className={styles.path}>{path}</span>
                    </span>

                    {!!romFileCount && (
                      <span className={styles.statistics}>
                        (
                        {translate('DeleteGameFolderRomCount', {
                          romFileCount,
                          size: formatBytes(sizeOnDisk),
                        })}
                        )
                      </span>
                    )}
                  </span>
                )}
              </li>
            );
          })}
        </ul>

        {deleteFiles && !!totalRomFileCount ? (
          <div className={styles.deleteFilesMessage}>
            {translate('DeleteGameFolderRomCount', {
              romFileCount: totalRomFileCount,
              size: formatBytes(totalSizeOnDisk),
            })}
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.DANGER} onPress={onDeleteGameConfirmed}>
          {translate('Delete')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default DeleteGameModalContent;
