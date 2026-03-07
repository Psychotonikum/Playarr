import React, { useCallback, useState } from 'react';
import { Tab, TabList, TabPanel, Tabs } from 'react-tabs';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import Rom from 'Rom/Rom';
import RomDetailsTab from 'Rom/RomDetailsTab';
import romEntities from 'Rom/romEntities';
import useRom, {
  RomEntity,
  getQueryKey,
  useToggleEpisodesMonitored,
} from 'Rom/useRom';
import Game from 'Game/Game';
import { useSingleGame } from 'Game/useGame';
import translate from 'Utilities/String/translate';
import RomHistory from './History/RomHistory';
import EpisodeSearch from './Search/RomSearch';
import PlatformRomNumber from './PlatformRomNumber';
import RomSummary from './Summary/RomSummary';
import styles from './RomDetailsModalContent.css';

const TABS: RomDetailsTab[] = ['details', 'history', 'search'];

export interface RomDetailsModalContentProps {
  romId: number;
  romEntity: RomEntity;
  gameId: number;
  romTitle: string;
  showOpenSeriesButton?: boolean;
  selectedTab?: RomDetailsTab;
  startInteractiveSearch?: boolean;
  onTabChange(isSearch: boolean): void;
  onModalClose(): void;
}

function RomDetailsModalContent({
  romId,
  romEntity = romEntities.ROMS,
  gameId,
  romTitle,
  showOpenSeriesButton = false,
  startInteractiveSearch = false,
  selectedTab = 'details',
  onTabChange,
  onModalClose,
}: RomDetailsModalContentProps) {
  const [currentlySelectedTab, setCurrentlySelectedTab] = useState(selectedTab);

  const {
    title: gameTitle,
    titleSlug,
    monitored: seriesMonitored,
    gameType,
  } = useSingleGame(gameId) as Game;

  const {
    romFileId,
    platformNumber,
    romNumber,
    absoluteRomNumber,
    airDate,
    monitored,
  } = useRom(romId, romEntity) as Rom;

  const { toggleEpisodesMonitored, isToggling } = useToggleEpisodesMonitored(
    getQueryKey(romEntity)!
  );

  const handleTabSelect = useCallback(
    (selectedIndex: number) => {
      const tab = TABS[selectedIndex];
      onTabChange(tab === 'search');
      setCurrentlySelectedTab(tab);
    },
    [onTabChange]
  );

  const handleMonitorEpisodePress = useCallback(
    (monitored: boolean) => {
      toggleEpisodesMonitored({
        romIds: [romId],
        monitored,
      });
    },
    [romId, toggleEpisodesMonitored]
  );

  const seriesLink = `/game/${titleSlug}`;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        <MonitorToggleButton
          monitored={monitored}
          size={18}
          isDisabled={!seriesMonitored}
          isSaving={isToggling}
          onPress={handleMonitorEpisodePress}
        />

        <span className={styles.gameTitle}>{gameTitle}</span>

        <span className={styles.separator}>-</span>

        <PlatformRomNumber
          platformNumber={platformNumber}
          romNumber={romNumber}
          absoluteRomNumber={absoluteRomNumber}
          airDate={airDate}
          gameType={gameType}
        />

        <span className={styles.separator}>-</span>

        {romTitle}
      </ModalHeader>

      <ModalBody>
        <Tabs
          className={styles.tabs}
          selectedIndex={TABS.indexOf(currentlySelectedTab)}
          onSelect={handleTabSelect}
        >
          <TabList className={styles.tabList}>
            <Tab className={styles.tab} selectedClassName={styles.selectedTab}>
              {translate('Details')}
            </Tab>

            <Tab className={styles.tab} selectedClassName={styles.selectedTab}>
              {translate('History')}
            </Tab>

            <Tab className={styles.tab} selectedClassName={styles.selectedTab}>
              {translate('Search')}
            </Tab>
          </TabList>

          <TabPanel>
            <div className={styles.tabContent}>
              <RomSummary
                romId={romId}
                romEntity={romEntity}
                romFileId={romFileId}
                gameId={gameId}
              />
            </div>
          </TabPanel>

          <TabPanel>
            <div className={styles.tabContent}>
              <RomHistory romId={romId} />
            </div>
          </TabPanel>

          <TabPanel>
            {/* Don't wrap in tabContent so we not have a top margin */}
            <EpisodeSearch
              romId={romId}
              startInteractiveSearch={startInteractiveSearch}
              onModalClose={onModalClose}
            />
          </TabPanel>
        </Tabs>
      </ModalBody>

      <ModalFooter>
        {showOpenSeriesButton && (
          <Button
            className={styles.openGameButton}
            to={seriesLink}
            onPress={onModalClose}
          >
            {translate('OpenSeries')}
          </Button>
        )}

        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default RomDetailsModalContent;
