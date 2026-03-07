import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import RomLanguages from 'Rom/RomLanguages';
import RomQuality from 'Rom/RomQuality';
import usePrevious from 'Helpers/Hooks/usePrevious';
import SelectRomModal from 'InteractiveImport/Rom/SelectRomModal';
import { SelectedEpisode } from 'InteractiveImport/Rom/SelectRomModalContent';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectPlatformModal from 'InteractiveImport/Platform/SelectPlatformModal';
import SelectGameModal from 'InteractiveImport/Game/SelectGameModal';
import { ReleaseEpisode, useGrabRelease } from 'InteractiveSearch/useReleases';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Game from 'Game/Game';
import { useSingleGame } from 'Game/useGame';
import { fetchDownloadClients } from 'Store/Actions/settingsActions';
import createEnabledDownloadClientsSelector from 'Store/Selectors/createEnabledDownloadClientsSelector';
import translate from 'Utilities/String/translate';
import SelectDownloadClientModal from './DownloadClient/SelectDownloadClientModal';
import OverrideMatchData from './OverrideMatchData';
import styles from './OverrideMatchModalContent.css';

type SelectType =
  | 'select'
  | 'game'
  | 'platform'
  | 'rom'
  | 'quality'
  | 'language'
  | 'downloadClient';

export interface OverrideMatchModalContentProps {
  indexerId: number;
  title: string;
  guid: string;
  gameId?: number;
  platformNumber?: number;
  roms: ReleaseEpisode[];
  languages: Language[];
  quality: QualityModel;
  protocol: DownloadProtocol;
  isGrabbing: boolean;
  grabError?: string;
  grabRelease: ReturnType<typeof useGrabRelease>['grabRelease'];
  onModalClose(): void;
}

function OverrideMatchModalContent(props: OverrideMatchModalContentProps) {
  const modalTitle = translate('ManualGrab');
  const {
    indexerId,
    title,
    guid,
    protocol,
    isGrabbing,
    grabError,
    grabRelease,
    onModalClose,
  } = props;

  const [gameId, setGameId] = useState(props.gameId);
  const [platformNumber, setPlatformNumber] = useState(props.platformNumber);
  const [roms, setEpisodes] = useState(props.roms);
  const [languages, setLanguages] = useState(props.languages);
  const [quality, setQuality] = useState(props.quality);
  const [downloadClientId, setDownloadClientId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [selectModalOpen, setSelectModalOpen] = useState<SelectType | null>(
    null
  );
  const previousIsGrabbing = usePrevious(isGrabbing);

  const dispatch = useDispatch();
  const game: Game | undefined = useSingleGame(gameId);
  const { items: downloadClients } = useSelector(
    createEnabledDownloadClientsSelector(protocol)
  );

  const romInfo = useMemo(() => {
    return roms.map((rom) => {
      return (
        <div key={rom.id}>
          {rom.romNumber}

          {game?.gameType === 'anime' &&
          rom.absoluteRomNumber != null
            ? ` (${rom.absoluteRomNumber})`
            : ''}

          {` - ${rom.title}`}
        </div>
      );
    });
  }, [game, roms]);

  const onSelectModalClose = useCallback(() => {
    setSelectModalOpen(null);
  }, [setSelectModalOpen]);

  const onSelectSeriesPress = useCallback(() => {
    setSelectModalOpen('game');
  }, [setSelectModalOpen]);

  const onSeriesSelect = useCallback(
    (s: Game) => {
      setGameId(s.id);
      setPlatformNumber(undefined);
      setEpisodes([]);
      setSelectModalOpen(null);
    },
    [setGameId, setPlatformNumber, setEpisodes, setSelectModalOpen]
  );

  const onSelectSeasonPress = useCallback(() => {
    setSelectModalOpen('platform');
  }, [setSelectModalOpen]);

  const onSeasonSelect = useCallback(
    (s: number) => {
      setPlatformNumber(s);
      setEpisodes([]);
      setSelectModalOpen(null);
    },
    [setPlatformNumber, setEpisodes, setSelectModalOpen]
  );

  const onSelectEpisodePress = useCallback(() => {
    setSelectModalOpen('rom');
  }, [setSelectModalOpen]);

  const onEpisodesSelect = useCallback(
    (episodeMap: SelectedEpisode[]) => {
      setEpisodes(episodeMap[0].roms);
      setSelectModalOpen(null);
    },
    [setEpisodes, setSelectModalOpen]
  );

  const onSelectQualityPress = useCallback(() => {
    setSelectModalOpen('quality');
  }, [setSelectModalOpen]);

  const onQualitySelect = useCallback(
    (quality: QualityModel) => {
      setQuality(quality);
      setSelectModalOpen(null);
    },
    [setQuality, setSelectModalOpen]
  );

  const onSelectLanguagesPress = useCallback(() => {
    setSelectModalOpen('language');
  }, [setSelectModalOpen]);

  const onLanguagesSelect = useCallback(
    (languages: Language[]) => {
      setLanguages(languages);
      setSelectModalOpen(null);
    },
    [setLanguages, setSelectModalOpen]
  );

  const onSelectDownloadClientPress = useCallback(() => {
    setSelectModalOpen('downloadClient');
  }, [setSelectModalOpen]);

  const onDownloadClientSelect = useCallback(
    (downloadClientId: number) => {
      setDownloadClientId(downloadClientId);
      setSelectModalOpen(null);
    },
    [setDownloadClientId, setSelectModalOpen]
  );

  const onGrabPress = useCallback(() => {
    if (!gameId) {
      setError(translate('OverrideGrabNoGame'));
      return;
    } else if (!roms.length) {
      setError(translate('OverrideGrabNoEpisode'));
      return;
    } else if (!quality) {
      setError(translate('OverrideGrabNoQuality'));
      return;
    } else if (!languages.length) {
      setError(translate('OverrideGrabNoLanguage'));
      return;
    }

    grabRelease({
      indexerId,
      guid,
      override: {
        gameId,
        romIds: roms.map((e) => e.id),
        quality,
        languages,
        downloadClientId,
      },
    });
  }, [
    indexerId,
    guid,
    gameId,
    roms,
    quality,
    languages,
    downloadClientId,
    setError,
    grabRelease,
  ]);

  useEffect(() => {
    if (!isGrabbing && previousIsGrabbing) {
      onModalClose();
    }
  }, [isGrabbing, previousIsGrabbing, onModalClose]);

  useEffect(
    () => {
      dispatch(fetchDownloadClients());
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('OverrideGrabModalTitle', { title })}
      </ModalHeader>

      <ModalBody>
        <DescriptionList>
          <DescriptionListItem
            className={styles.item}
            title={translate('Game')}
            data={
              <OverrideMatchData
                value={game?.title}
                onPress={onSelectSeriesPress}
              />
            }
          />

          <DescriptionListItem
            className={styles.item}
            title={translate('PlatformNumber')}
            data={
              <OverrideMatchData
                value={platformNumber}
                isDisabled={!game}
                onPress={onSelectSeasonPress}
              />
            }
          />

          <DescriptionListItem
            className={styles.item}
            title={translate('Roms')}
            data={
              <OverrideMatchData
                value={romInfo}
                isDisabled={!game || isNaN(Number(platformNumber))}
                onPress={onSelectEpisodePress}
              />
            }
          />

          <DescriptionListItem
            className={styles.item}
            title={translate('Quality')}
            data={
              <OverrideMatchData
                value={
                  <RomQuality className={styles.label} quality={quality} />
                }
                onPress={onSelectQualityPress}
              />
            }
          />

          <DescriptionListItem
            className={styles.item}
            title={translate('Languages')}
            data={
              <OverrideMatchData
                value={
                  <RomLanguages
                    className={styles.label}
                    languages={languages}
                  />
                }
                onPress={onSelectLanguagesPress}
              />
            }
          />

          {downloadClients.length > 1 ? (
            <DescriptionListItem
              className={styles.item}
              title={translate('DownloadClient')}
              data={
                <OverrideMatchData
                  value={
                    downloadClients.find(
                      (downloadClient) => downloadClient.id === downloadClientId
                    )?.name ?? translate('Default')
                  }
                  onPress={onSelectDownloadClientPress}
                />
              }
            />
          ) : null}
        </DescriptionList>
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <div className={styles.error}>{error || grabError}</div>

        <div className={styles.buttons}>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <SpinnerErrorButton
            isSpinning={isGrabbing}
            error={grabError}
            onPress={onGrabPress}
          >
            {translate('GrabRelease')}
          </SpinnerErrorButton>
        </div>
      </ModalFooter>

      <SelectGameModal
        isOpen={selectModalOpen === 'game'}
        modalTitle={modalTitle}
        onSeriesSelect={onSeriesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectPlatformModal
        isOpen={selectModalOpen === 'platform'}
        modalTitle={modalTitle}
        gameId={gameId}
        onSeasonSelect={onSeasonSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectRomModal
        isOpen={selectModalOpen === 'rom'}
        selectedIds={[guid]}
        gameId={gameId}
        isAnime={game?.gameType === 'anime'}
        platformNumber={platformNumber}
        selectedDetails={title}
        modalTitle={modalTitle}
        onEpisodesSelect={onEpisodesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectQualityModal
        isOpen={selectModalOpen === 'quality'}
        qualityId={quality ? quality.quality.id : 0}
        proper={quality ? quality.revision.version > 1 : false}
        real={quality ? quality.revision.real > 0 : false}
        modalTitle={modalTitle}
        onQualitySelect={onQualitySelect}
        onModalClose={onSelectModalClose}
      />

      <SelectLanguageModal
        isOpen={selectModalOpen === 'language'}
        languageIds={languages ? languages.map((l) => l.id) : []}
        modalTitle={modalTitle}
        onLanguagesSelect={onLanguagesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectDownloadClientModal
        isOpen={selectModalOpen === 'downloadClient'}
        protocol={protocol}
        modalTitle={modalTitle}
        onDownloadClientSelect={onDownloadClientSelect}
        onModalClose={onSelectModalClose}
      />
    </ModalContent>
  );
}

export default OverrideMatchModalContent;
