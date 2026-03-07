import moment from 'moment';
import React, { useCallback } from 'react';
import { useQueueDetails } from 'Activity/Queue/Details/QueueDetailsProvider';
import { useCommands } from 'Commands/useCommands';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import { isCommandExecuting } from 'Utilities/Command';
import isBefore from 'Utilities/Date/isBefore';
import translate from 'Utilities/String/translate';
import useCalendar, {
  useCalendarRange,
  useCalendarSearchMissingCommandId,
} from './useCalendar';

function useIsSearching(searchMissingCommandId: number | undefined) {
  const { data: commands } = useCommands();

  if (searchMissingCommandId == null) {
    return false;
  }

  return isCommandExecuting(
    commands.find((command) => {
      return command.id === searchMissingCommandId;
    })
  );
}

const useMissingRomIdsSelector = () => {
  const { start, end } = useCalendarRange();
  const { data } = useCalendar();
  const queueDetails = useQueueDetails();

  return data.reduce<number[]>((acc, rom) => {
    const airDateUtc = rom.airDateUtc;

    if (
      !rom.romFileId &&
      moment(airDateUtc).isAfter(start) &&
      moment(airDateUtc).isBefore(end) &&
      isBefore(rom.airDateUtc) &&
      !queueDetails.some(
        (details) => !!details.rom && details.rom.id === rom.id
      )
    ) {
      acc.push(rom.id);
    }

    return acc;
  }, []);
};

export default function CalendarMissingRomSearchButton() {
  const searchMissingCommandId = useCalendarSearchMissingCommandId();
  const missingRomIds = useMissingRomIdsSelector();
  const isSearchingForMissing = useIsSearching(searchMissingCommandId);

  const handlePress = useCallback(() => {}, []);

  return (
    <PageToolbarButton
      label={translate('SearchForMissing')}
      iconName={icons.SEARCH}
      isDisabled={!missingRomIds.length}
      isSpinning={isSearchingForMissing}
      onPress={handlePress}
    />
  );
}
