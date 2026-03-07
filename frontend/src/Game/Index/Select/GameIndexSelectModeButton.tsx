import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import PageToolbarButton, {
  PageToolbarButtonProps,
} from 'Components/Page/Toolbar/PageToolbarButton';

interface GameIndexSelectModeButtonProps extends PageToolbarButtonProps {
  isSelectMode: boolean;
  onPress: () => void;
}

function GameIndexSelectModeButton(props: GameIndexSelectModeButtonProps) {
  const { label, iconName, isSelectMode, overflowComponent, onPress } = props;
  const { reset } = useSelect();

  const onPressWrapper = useCallback(() => {
    if (isSelectMode) {
      reset();
    }

    onPress();
  }, [isSelectMode, onPress, reset]);

  return (
    <PageToolbarButton
      label={label}
      iconName={iconName}
      overflowComponent={overflowComponent}
      onPress={onPressWrapper}
    />
  );
}

export default GameIndexSelectModeButton;
