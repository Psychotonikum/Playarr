import React, { useCallback, useMemo } from 'react';
import AddGame from 'AddGame/AddGame';
import {
  AddGameOptions,
  setAddGameOption,
  useAddGameOptions,
} from 'AddGame/addGameOptionsStore';
import GameMonitoringOptionsPopoverContent from 'AddGame/GameMonitoringOptionsPopoverContent';
import { useAppDimension } from 'App/appStore';
import CheckInput from 'Components/Form/CheckInput';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import { getValidationFailures } from 'Helpers/Hooks/useApiMutation';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import GamePoster from 'Game/GamePoster';
import selectSettings from 'Store/Selectors/selectSettings';
import { useIsWindows } from 'System/Status/useSystemStatus';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { useAddGame } from './useAddGame';
import styles from './AddNewGameModalContent.css';

export interface AddNewGameModalContentProps {
  game: AddGame;
  onModalClose: () => void;
}

function AddNewGameModalContent({
  game,
  onModalClose,
}: AddNewGameModalContentProps) {
  const { title, year, overview, images, folder, platforms = [] } = game;
  const options = useAddGameOptions();
  const isSmallScreen = useAppDimension('isSmallScreen');
  const isWindows = useIsWindows();

  const { isAdding, addError, addGame } = useAddGame();

  const { settings, validationErrors, validationWarnings } = useMemo(() => {
    return {
      ...selectSettings(options, {}),
      ...getValidationFailures(addError),
    };
  }, [options, addError]);

  const {
    monitor,
    rootFolderPath,
    searchForMissingRoms,
    tags,
    preferredRegions,
    preferredLanguageIds,
    preferredReleaseTypes,
    preferredModifications,
  } = settings;

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged<string | number | boolean | number[]>) => {
      setAddGameOption(name as keyof AddGameOptions, value);
    },
    []
  );

  const handleAddGamePress = useCallback(() => {
    addGame({
      ...game,
      rootFolderPath: rootFolderPath.value,
      addOptions: {
        monitor: monitor.value,
        searchForMissingRoms: searchForMissingRoms.value,
      },
      tags: tags.value,
      preferredRegions: preferredRegions.value,
      preferredLanguageIds: preferredLanguageIds.value,
      preferredReleaseTypes: preferredReleaseTypes.value,
      preferredModifications: preferredModifications.value,
    });
  }, [
    game,
    rootFolderPath,
    monitor,
    searchForMissingRoms,
    tags,
    preferredRegions,
    preferredLanguageIds,
    preferredReleaseTypes,
    preferredModifications,
    addGame,
  ]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {title}

        {!title.includes(String(year)) && year ? (
          <span className={styles.year}>({year})</span>
        ) : null}
      </ModalHeader>

      <ModalBody>
        <div className={styles.container}>
          {isSmallScreen ? null : (
            <div className={styles.poster}>
              <GamePoster
                className={styles.poster}
                images={images}
                size={250}
                title={title}
              />
            </div>
          )}

          <div className={styles.info}>
            {overview ? (
              <div className={styles.overview}>{overview}</div>
            ) : null}

            <Form
              validationErrors={validationErrors}
              validationWarnings={validationWarnings}
            >
              <FormGroup>
                <FormLabel>{translate('RootFolder')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.ROOT_FOLDER_SELECT}
                  name="rootFolderPath"
                  valueOptions={{
                    seriesFolder: folder,
                    isWindows,
                  }}
                  selectedValueOptions={{
                    seriesFolder: folder,
                    isWindows,
                  }}
                  helpText={translate('AddNewGameRootFolderHelpText', {
                    folder,
                  })}
                  onChange={handleInputChange}
                  {...rootFolderPath}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  {translate('Monitor')}

                  <Popover
                    anchor={
                      <Icon className={styles.labelIcon} name={icons.INFO} />
                    }
                    title={translate('MonitoringOptions')}
                    body={<GameMonitoringOptionsPopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.MONITOR_EPISODES_SELECT}
                  name="monitor"
                  onChange={handleInputChange}
                  {...monitor}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('Language')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.LANGUAGE_SELECT}
                  name="language"
                  value={0}
                  onChange={handleInputChange as any}
                  helpText={translate('LanguageHelpText')}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  {translate('GamePlatform')}
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="gamePlatform"
                  value="all"
                  values={[
                    { key: 'all', value: translate('All') },
                    ...platforms.map((p) => ({
                      key: String(p.platformNumber),
                      value: p.title || `Platform ${p.platformNumber}`,
                    })),
                  ]}
                  onChange={handleInputChange}
                  helpText={translate('GamePlatformHelpText')}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('PreferredRegions')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="preferredRegions"
                  value={preferredRegions.value as unknown as number[]}
                  values={[
                    { key: 'USA', value: 'USA' },
                    { key: 'Europe', value: 'Europe' },
                    { key: 'Japan', value: 'Japan' },
                    { key: 'World', value: 'World' },
                    { key: 'Asia', value: 'Asia' },
                    { key: 'Australia', value: 'Australia' },
                    { key: 'Korea', value: 'Korea' },
                    { key: 'Brazil', value: 'Brazil' },
                  ]}
                  helpText={translate('PreferredRegionsHelpText')}
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('PreferredReleaseTypes')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="preferredReleaseTypes"
                  value={preferredReleaseTypes.value as unknown as number[]}
                  values={[
                    { key: 'Retail', value: 'Retail' },
                    { key: 'Prototype', value: 'Prototype' },
                    { key: 'Beta', value: 'Beta' },
                    { key: 'Demo', value: 'Demo' },
                    { key: 'Sample', value: 'Sample' },
                    { key: 'Promo', value: 'Promo' },
                    { key: 'Update', value: 'Update' },
                    { key: 'Dlc', value: 'DLC' },
                  ]}
                  helpText={translate('PreferredReleaseTypesHelpText')}
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('PreferredModifications')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="preferredModifications"
                  value={preferredModifications.value as unknown as number[]}
                  values={[
                    { key: 'Original', value: 'Original' },
                    { key: 'Hack', value: 'Hack' },
                    { key: 'Translation', value: 'Translation' },
                    { key: 'Homebrew', value: 'Homebrew' },
                    { key: 'Unlicensed', value: 'Unlicensed' },
                  ]}
                  helpText={translate('PreferredModificationsHelpText')}
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('Tags')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  onChange={handleInputChange}
                  {...tags}
                />
              </FormGroup>
            </Form>
          </div>
        </div>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div>
          <label className={styles.searchLabelContainer}>
            <span className={styles.searchLabel}>
              {translate('AddNewGameSearchForMissingEpisodes')}
            </span>

            <CheckInput
              containerClassName={styles.searchInputContainer}
              className={styles.searchInput}
              name="searchForMissingRoms"
              onChange={handleInputChange}
              {...searchForMissingRoms}
            />
          </label>
        </div>

        <SpinnerButton
          className={styles.addButton}
          kind={kinds.SUCCESS}
          isSpinning={isAdding}
          onPress={handleAddGamePress}
        >
          {translate('AddGameWithTitle', { title })}
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddNewGameModalContent;
