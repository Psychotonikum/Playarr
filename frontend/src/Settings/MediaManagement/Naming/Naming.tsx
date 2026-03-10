import React, { useCallback, useEffect, useState } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useDebounce from 'Helpers/Hooks/useDebounce';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import NamingModal from './NamingModal';
import {
  NamingSettingsModel,
  useManageNamingSettings,
  useNamingExamples,
} from './useNamingSettings';
import styles from './Naming.css';

interface NamingModalOptions {
  name: keyof Pick<
    NamingSettingsModel,
    | 'standardEpisodeFormat'
    | 'dailyEpisodeFormat'
    | 'animeEpisodeFormat'
    | 'seriesFolderFormat'
    | 'platformFolderFormat'
    | 'specialsFolderFormat'
  >;
  platform?: boolean;
  rom?: boolean;
  daily?: boolean;
  anime?: boolean;
  additional?: boolean;
}

interface NamingProps {
  setChildSave: (saveCallback: () => void) => void;
  onChildStateChange: (state: {
    isSaving: boolean;
    hasPendingChanges: boolean;
  }) => void;
}

function Naming({ setChildSave, onChildStateChange }: NamingProps) {
  const advancedSettings = useShowAdvancedSettings();
  const {
    settings,
    updateSetting,
    isFetching,
    error,
    hasSettings,
    hasPendingChanges,
    isSaving,
    saveSettings,
  } = useManageNamingSettings();

  const debouncedSettings = useDebounce(settings, 300);
  const { examples } = useNamingExamples(debouncedSettings);
  const examplesPopulated = !!examples;

  const [isNamingModalOpen, setNamingModalOpen, setNamingModalClosed] =
    useModalOpenState(false);
  const [namingModalOptions, setNamingModalOptions] =
    useState<NamingModalOptions | null>(null);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      const key = change.name as keyof NamingSettingsModel;

      updateSetting(key, change.value as NamingSettingsModel[typeof key]);
    },
    [updateSetting]
  );

  const handleStandardNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'standardEpisodeFormat',
      platform: true,
      rom: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handleDailyNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'dailyEpisodeFormat',
      platform: true,
      rom: true,
      daily: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handleAnimeNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'animeEpisodeFormat',
      platform: true,
      rom: true,
      anime: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handleGameFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'seriesFolderFormat',
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handlePlatformFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'platformFolderFormat',
      platform: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handleSpecialsFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'specialsFolderFormat',
      platform: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const renameRoms = hasSettings && settings.renameRoms.value;
  const replaceIllegalCharacters =
    hasSettings && settings.replaceIllegalCharacters.value;

  const multiEpisodeStyleOptions: EnhancedSelectInputValue<number>[] = [
    { key: 0, value: translate('Extend'), hint: 'S01E01-02-03' },
    { key: 1, value: translate('Duplicate'), hint: 'S01E01.S01E02' },
    { key: 2, value: translate('Repeat'), hint: 'S01E01E02E03' },
    { key: 3, value: translate('Scene'), hint: 'S01E01-E02-E03' },
    { key: 4, value: translate('Range'), hint: 'S01E01-03' },
    { key: 5, value: translate('PrefixedRange'), hint: 'S01E01-E03' },
  ];

  const colonReplacementOptions: EnhancedSelectInputValue<number>[] = [
    { key: 0, value: translate('Delete') },
    { key: 1, value: translate('ReplaceWithDash') },
    { key: 2, value: translate('ReplaceWithSpaceDash') },
    { key: 3, value: translate('ReplaceWithSpaceDashSpace') },
    {
      key: 4,
      value: translate('SmartReplace'),
      hint: translate('SmartReplaceHint'),
    },
    {
      key: 5,
      value: translate('Custom'),
      hint: translate('CustomColonReplacementFormatHint'),
    },
  ];

  const standardEpisodeFormatHelpTexts = [];
  const standardEpisodeFormatErrors = [];
  const dailyEpisodeFormatHelpTexts = [];
  const dailyEpisodeFormatErrors = [];
  const animeEpisodeFormatHelpTexts = [];
  const animeEpisodeFormatErrors = [];
  const seriesFolderFormatHelpTexts = [];
  const seriesFolderFormatErrors = [];
  const platformFolderFormatHelpTexts = [];
  const platformFolderFormatErrors = [];
  const specialsFolderFormatHelpTexts = [];
  const specialsFolderFormatErrors = [];

  if (examplesPopulated) {
    if (examples.singleEpisodeExample) {
      standardEpisodeFormatHelpTexts.push(
        `${translate('SingleEpisode')}: ${examples.singleEpisodeExample}`
      );
    } else {
      standardEpisodeFormatErrors.push({
        message: translate('SingleEpisodeInvalidFormat'),
      });
    }

    if (examples.multiEpisodeExample) {
      standardEpisodeFormatHelpTexts.push(
        `${translate('MultiEpisode')}: ${examples.multiEpisodeExample}`
      );
    } else {
      standardEpisodeFormatErrors.push({
        message: translate('MultiEpisodeInvalidFormat'),
      });
    }

    if (examples.dailyEpisodeExample) {
      dailyEpisodeFormatHelpTexts.push(
        `${translate('Example')}: ${examples.dailyEpisodeExample}`
      );
    } else {
      dailyEpisodeFormatErrors.push({ message: translate('InvalidFormat') });
    }

    if (examples.animeEpisodeExample) {
      animeEpisodeFormatHelpTexts.push(
        `${translate('SingleEpisode')}: ${examples.animeEpisodeExample}`
      );
    } else {
      animeEpisodeFormatErrors.push({
        message: translate('SingleEpisodeInvalidFormat'),
      });
    }

    if (examples.animeMultiEpisodeExample) {
      animeEpisodeFormatHelpTexts.push(
        `${translate('MultiEpisode')}: ${examples.animeMultiEpisodeExample}`
      );
    } else {
      animeEpisodeFormatErrors.push({
        message: translate('MultiEpisodeInvalidFormat'),
      });
    }

    if (examples.seriesFolderExample) {
      seriesFolderFormatHelpTexts.push(
        `${translate('Example')}: ${examples.seriesFolderExample}`
      );
    } else {
      seriesFolderFormatErrors.push({ message: translate('InvalidFormat') });
    }

    if (examples.platformFolderExample) {
      platformFolderFormatHelpTexts.push(
        `${translate('Example')}: ${examples.platformFolderExample}`
      );
    } else {
      platformFolderFormatErrors.push({ message: translate('InvalidFormat') });
    }

    if (examples.specialsFolderExample) {
      specialsFolderFormatHelpTexts.push(
        `${translate('Example')}: ${examples.specialsFolderExample}`
      );
    } else {
      specialsFolderFormatErrors.push({ message: translate('InvalidFormat') });
    }
  }

  useEffect(() => {
    onChildStateChange({
      hasPendingChanges,
      isSaving,
    });
  }, [hasPendingChanges, isSaving, onChildStateChange]);

  useEffect(() => {
    setChildSave(saveSettings);
  }, [setChildSave, saveSettings]);

  return (
    <FieldSet legend={translate('EpisodeNaming')}>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('NamingSettingsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && !isFetching && !error ? (
        <Form>
          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('RenameRoms')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="renameRoms"
              helpText={translate('RenameRomsHelpText')}
              onChange={handleInputChange}
              {...settings.renameRoms}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('ReplaceIllegalCharacters')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="replaceIllegalCharacters"
              helpText={translate('ReplaceIllegalCharactersHelpText')}
              onChange={handleInputChange}
              {...settings.replaceIllegalCharacters}
            />
          </FormGroup>

          {replaceIllegalCharacters ? (
            <FormGroup size={sizes.MEDIUM}>
              <FormLabel>{translate('ColonReplacement')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="colonReplacementFormat"
                values={colonReplacementOptions}
                helpText={translate('ColonReplacementFormatHelpText')}
                onChange={handleInputChange}
                {...settings.colonReplacementFormat}
              />
            </FormGroup>
          ) : null}

          {replaceIllegalCharacters &&
          settings.colonReplacementFormat.value === 5 ? (
            <FormGroup size={sizes.MEDIUM}>
              <FormLabel>{translate('CustomColonReplacement')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="customColonReplacementFormat"
                helpText={translate('CustomColonReplacementFormatHelpText')}
                onChange={handleInputChange}
                {...settings.customColonReplacementFormat}
              />
            </FormGroup>
          ) : null}

          {renameRoms ? (
            <>
              <FormGroup size={sizes.LARGE}>
                <FormLabel>{translate('StandardEpisodeFormat')}</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="standardEpisodeFormat"
                  buttons={
                    <FormInputButton
                      onPress={handleStandardNamingModalOpenClick}
                    >
                      ?
                    </FormInputButton>
                  }
                  onChange={handleInputChange}
                  {...settings.standardEpisodeFormat}
                  helpTexts={standardEpisodeFormatHelpTexts}
                  errors={[
                    ...standardEpisodeFormatErrors,
                    ...settings.standardEpisodeFormat.errors,
                  ]}
                />
              </FormGroup>

              <FormGroup size={sizes.LARGE}>
                <FormLabel>{translate('DailyEpisodeFormat')}</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="dailyEpisodeFormat"
                  buttons={
                    <FormInputButton onPress={handleDailyNamingModalOpenClick}>
                      ?
                    </FormInputButton>
                  }
                  onChange={handleInputChange}
                  {...settings.dailyEpisodeFormat}
                  helpTexts={dailyEpisodeFormatHelpTexts}
                  errors={[
                    ...dailyEpisodeFormatErrors,
                    ...settings.dailyEpisodeFormat.errors,
                  ]}
                />
              </FormGroup>

              <FormGroup size={sizes.LARGE}>
                <FormLabel>{translate('AnimeEpisodeFormat')}</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="animeEpisodeFormat"
                  buttons={
                    <FormInputButton onPress={handleAnimeNamingModalOpenClick}>
                      ?
                    </FormInputButton>
                  }
                  onChange={handleInputChange}
                  {...settings.animeEpisodeFormat}
                  helpTexts={animeEpisodeFormatHelpTexts}
                  errors={[
                    ...animeEpisodeFormatErrors,
                    ...settings.animeEpisodeFormat.errors,
                  ]}
                />
              </FormGroup>
            </>
          ) : null}

          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>{translate('GameFolderFormat')}</FormLabel>

            <FormInputGroup
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="seriesFolderFormat"
              buttons={
                <FormInputButton onPress={handleGameFolderNamingModalOpenClick}>
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.seriesFolderFormat}
              helpTexts={[
                translate('GameFolderFormatHelpText'),
                ...seriesFolderFormatHelpTexts,
              ]}
              errors={[
                ...seriesFolderFormatErrors,
                ...settings.seriesFolderFormat.errors,
              ]}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('PlatformFolderFormat')}</FormLabel>

            <FormInputGroup
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="platformFolderFormat"
              buttons={
                <FormInputButton
                  onPress={handlePlatformFolderNamingModalOpenClick}
                >
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.platformFolderFormat}
              helpTexts={platformFolderFormatHelpTexts}
              errors={[
                ...platformFolderFormatErrors,
                ...settings.platformFolderFormat.errors,
              ]}
            />
          </FormGroup>

          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>{translate('SpecialsFolderFormat')}</FormLabel>

            <FormInputGroup
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="specialsFolderFormat"
              buttons={
                <FormInputButton
                  onPress={handleSpecialsFolderNamingModalOpenClick}
                >
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.specialsFolderFormat}
              helpTexts={specialsFolderFormatHelpTexts}
              errors={[
                ...specialsFolderFormatErrors,
                ...settings.specialsFolderFormat.errors,
              ]}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('MultiEpisodeStyle')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="multiEpisodeStyle"
              values={multiEpisodeStyleOptions}
              onChange={handleInputChange}
              {...settings.multiEpisodeStyle}
            />
          </FormGroup>

          {namingModalOptions ? (
            <NamingModal
              isOpen={isNamingModalOpen}
              {...namingModalOptions}
              value={settings[namingModalOptions.name].value}
              onInputChange={handleInputChange}
              onModalClose={setNamingModalClosed}
            />
          ) : null}
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default Naming;
