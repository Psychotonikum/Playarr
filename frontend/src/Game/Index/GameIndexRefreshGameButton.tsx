import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import Game from 'Game/Game';
import { useGameIndex } from 'Game/useGame';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface GameIndexRefreshGameButtonProps {
  isSelectMode: boolean;
  selectedFilterKey: string | number;
}

function GameIndexRefreshGameButton(props: GameIndexRefreshGameButtonProps) {
  const isRefreshing = useCommandExecuting(CommandNames.RefreshSeries);
  const { data, totalItems } = useGameIndex();

  const executeCommand = useExecuteCommand();
  const { isSelectMode, selectedFilterKey } = props;
  const { anySelected, getSelectedIds } = useSelect<Game>();

  let refreshLabel = translate('UpdateAll');

  if (anySelected) {
    refreshLabel = translate('UpdateSelected');
  } else if (selectedFilterKey !== 'all') {
    refreshLabel = translate('UpdateFiltered');
  }

  const onPress = useCallback(() => {
    const seriesToRefresh =
      isSelectMode && anySelected ? getSelectedIds() : data.map((m) => m.id);

    executeCommand({
      name: CommandNames.RefreshSeries,
      gameIds: seriesToRefresh,
    });
  }, [executeCommand, anySelected, isSelectMode, data, getSelectedIds]);

  return (
    <PageToolbarButton
      label={refreshLabel}
      isSpinning={isRefreshing}
      isDisabled={!totalItems}
      iconName={icons.REFRESH}
      onPress={onPress}
    />
  );
}

export default GameIndexRefreshGameButton;
