import React, {
  PropsWithChildren,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react';
import QueueDetailsProvider from 'Activity/Queue/Details/QueueDetailsProvider';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Rom from 'Rom/Rom';
import RomFileProvider from 'RomFile/RomFileProvider';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import useMeasure from 'Helpers/Hooks/useMeasure';
import { align, icons } from 'Helpers/Props';
import NoGame from 'Game/NoGame';
import { useHasSeries } from 'Game/useGame';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import translate from 'Utilities/String/translate';
import Calendar from './Calendar';
import CalendarFilterModal from './CalendarFilterModal';
import CalendarMissingRomSearchButton from './CalendarMissingRomSearchButton';
import { setCalendarOption, useCalendarOption } from './calendarOptionsStore';
import CalendarLinkModal from './iCal/CalendarLinkModal';
import Legend from './Legend/Legend';
import CalendarOptionsModal from './Options/CalendarOptionsModal';
import useCalendar, {
  FILTERS,
  setCalendarDayCount,
  useCalendarPage,
} from './useCalendar';
import styles from './CalendarPage.css';

const MINIMUM_DAY_WIDTH = 120;

function CalendarPage() {
  const executeCommand = useExecuteCommand();

  const selectedFilterKey = useCalendarOption('selectedFilterKey');
  const { data } = useCalendar();

  useCalendarPage();

  const isRssSyncExecuting = useCommandExecuting(CommandNames.RssSync);
  const customFilters = useCustomFiltersList('calendar');
  const hasSeries = useHasSeries();

  const [pageContentRef, { width }] = useMeasure();
  const [isCalendarLinkModalOpen, setIsCalendarLinkModalOpen] = useState(false);
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);

  const isMeasured = width > 0;
  const PageComponent = hasSeries ? Calendar : NoGame;

  const handleGetCalendarLinkPress = useCallback(() => {
    setIsCalendarLinkModalOpen(true);
  }, []);

  const handleGetCalendarLinkModalClose = useCallback(() => {
    setIsCalendarLinkModalOpen(false);
  }, []);

  const handleOptionsPress = useCallback(() => {
    setIsOptionsModalOpen(true);
  }, []);

  const handleOptionsModalClose = useCallback(() => {
    setIsOptionsModalOpen(false);
  }, []);

  const handleRssSyncPress = useCallback(() => {
    executeCommand({
      name: CommandNames.RssSync,
    });
  }, [executeCommand]);

  const handleFilterSelect = useCallback((key: string | number) => {
    setCalendarOption('selectedFilterKey', key);
  }, []);

  const romIds = useMemo(() => {
    return selectUniqueIds<Rom, number>(data, 'id');
  }, [data]);

  const romFileIds = useMemo(() => {
    return selectUniqueIds<Rom, number>(data, 'romFileId');
  }, [data]);

  useEffect(() => {
    if (width === 0) {
      return;
    }

    const dayCount = Math.max(
      3,
      Math.min(7, Math.floor(width / MINIMUM_DAY_WIDTH))
    );

    setCalendarDayCount(dayCount);
  }, [width]);

  return (
    <CalendarPageProvider
      romIds={romIds}
      romFileIds={romFileIds}
    >
      <PageContent title={translate('Calendar')}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('ICalLink')}
              iconName={icons.CALENDAR}
              onPress={handleGetCalendarLinkPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('RssSync')}
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              onPress={handleRssSyncPress}
            />

            <CalendarMissingRomSearchButton />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <PageToolbarButton
              label={translate('Options')}
              iconName={icons.POSTER}
              onPress={handleOptionsPress}
            />

            <FilterMenu
              alignMenu={align.RIGHT}
              isDisabled={!hasSeries}
              selectedFilterKey={selectedFilterKey}
              filters={FILTERS}
              customFilters={customFilters}
              filterModalConnectorComponent={CalendarFilterModal}
              onFilterSelect={handleFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody
          ref={pageContentRef}
          className={styles.calendarPageBody}
          innerClassName={styles.calendarInnerPageBody}
        >
          {isMeasured ? <PageComponent totalItems={0} /> : <div />}
          {hasSeries && <Legend />}
        </PageContentBody>

        <CalendarLinkModal
          isOpen={isCalendarLinkModalOpen}
          onModalClose={handleGetCalendarLinkModalClose}
        />

        <CalendarOptionsModal
          isOpen={isOptionsModalOpen}
          onModalClose={handleOptionsModalClose}
        />
      </PageContent>
    </CalendarPageProvider>
  );
}

export default CalendarPage;

function CalendarPageProvider({
  romIds,
  romFileIds,
  children,
}: PropsWithChildren<{ romIds: number[]; romFileIds: number[] }>) {
  return (
    <QueueDetailsProvider romIds={romIds}>
      <RomFileProvider romFileIds={romFileIds}>
        {children}
      </RomFileProvider>
    </QueueDetailsProvider>
  );
}
