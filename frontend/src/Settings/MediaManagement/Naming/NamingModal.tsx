import React, { useCallback, useState } from 'react';
import FieldSet from 'Components/FieldSet';
import SelectInput, { SelectInputOption } from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import NamingOption from './NamingOption';
import TokenCase from './TokenCase';
import TokenSeparator from './TokenSeparator';
import { NamingSettingsModel } from './useNamingSettings';
import styles from './NamingModal.css';

type SeparatorInputOption = Omit<SelectInputOption, 'key'> & {
  key: TokenSeparator;
};

type CaseInputOption = Omit<SelectInputOption, 'key'> & {
  key: TokenCase;
};

const separatorOptions: SeparatorInputOption[] = [
  {
    key: ' ',
    get value() {
      return `${translate('Space')} ( )`;
    },
  },
  {
    key: '.',
    get value() {
      return `${translate('Period')} (.)`;
    },
  },
  {
    key: '_',
    get value() {
      return `${translate('Underscore')} (_)`;
    },
  },
  {
    key: '-',
    get value() {
      return `${translate('Dash')} (-)`;
    },
  },
];

const caseOptions: CaseInputOption[] = [
  {
    key: 'title',
    get value() {
      return translate('DefaultCase');
    },
  },
  {
    key: 'lower',
    get value() {
      return translate('Lowercase');
    },
  },
  {
    key: 'upper',
    get value() {
      return translate('Uppercase');
    },
  },
];

const fileNameTokens = [
  {
    token:
      '{Game TitleYear} - S{platform:00}E{rom:00} - {Rom CleanTitle} {Quality Full}',
    example: "The Game Title's! (2010) - S01E01 - Rom Title WEBDL-1080p Proper",
  },
  {
    token:
      '{Game TitleYear} - {platform:0}x{rom:00} - {Rom CleanTitle} {Quality Full}',
    example: "The Game Title's! (2010) - 1x01 - Rom Title WEBDL-1080p Proper",
  },
  {
    token:
      '{Game.CleanTitleYear}.S{platform:00}E{rom:00}.{Rom.CleanTitle}.{Quality.Full}',
    example: "The.Game.Title's!.2010.S01E01.Rom.Title.WEBDL-1080p.Proper",
  },
];

const fileNameDailyTokens = [
  {
    token: '{Game TitleYear} - {Air-Date} - {Rom CleanTitle} {Quality Full}',
    example:
      "The Game Title's! (2010) - 2013-10-30 - Rom Title WEBDL-1080p Proper",
  },
  {
    token: '{Game.CleanTitleYear}.{Air.Date}.{Rom.CleanTitle}.{Quality.Full}',
    example: "The.Game.Title's!.2010.2013.10.30.Rom.Title.WEBDL-1080p.Proper",
  },
];

const fileNameAnimeTokens = [
  {
    token:
      '{Game TitleYear} - S{platform:00}E{rom:00} - {absolute:000} - {Rom CleanTitle} {Quality Full}',
    example:
      "The Game Title's! (2010) - S01E01 - 001- Rom Title WEBDL-1080p Proper",
  },
  {
    token:
      '{Game TitleYear} - {platform:0}x{rom:00} - {absolute:000} - {Rom CleanTitle} {Quality Full}',
    example:
      "The Game Title's! (2010) - 1x01 - 001 - Rom Title WEBDL-1080p Proper",
  },
  {
    token:
      '{Game.CleanTitleYear}.S{platform:00}E{rom:00}.{absolute:000}.{Rom.CleanTitle}.{Quality.Full}',
    example: "The.Game.Title's!.2010.S01E01.001.Rom.Title.WEBDL-1080p.Proper",
  },
];

const seriesTokens = [
  { token: '{Game Title}', example: "The Game Title's!", footNotes: '1' },
  {
    token: '{Game CleanTitle}',
    example: "The Game Title's!",
    footNotes: '1',
  },
  {
    token: '{Game TitleYear}',
    example: "The Game Title's! (2010)",
    footNotes: '1',
  },
  {
    token: '{Game CleanTitleYear}',
    example: "The Game Title's! 2010",
    footNotes: '1',
  },
  {
    token: '{Game TitleWithoutYear}',
    example: "The Game Title's!",
    footNotes: '1',
  },
  {
    token: '{Game CleanTitleWithoutYear}',
    example: "The Game Title's!",
    footNotes: '1',
  },
  {
    token: '{Game TitleThe}',
    example: "Game Title's!, The",
    footNotes: '1',
  },
  {
    token: '{Game CleanTitleThe}',
    example: "Game Title's!, The",
    footNotes: '1',
  },
  {
    token: '{Game TitleTheYear}',
    example: "Game Title's!, The (2010)",
    footNotes: '1',
  },
  {
    token: '{Game CleanTitleTheYear}',
    example: "Game Title's!, The 2010",
    footNotes: '1',
  },
  {
    token: '{Game TitleTheWithoutYear}',
    example: "Game Title's!, The",
    footNotes: '1',
  },
  {
    token: '{Game CleanTitleTheWithoutYear}',
    example: "Game Title's!, The",
    footNotes: '1',
  },
  { token: '{Game TitleFirstCharacter}', example: 'S', footNotes: '1' },
  { token: '{Game Year}', example: '2010' },
];

const gameIdTokens = [
  { token: '{ImdbId}', example: 'tt12345' },
  { token: '{IgdbId}', example: '12345' },
  { token: '{TmdbId}', example: '11223' },
  { token: '{RawgId}', example: '54321' },
];

const seasonTokens = [
  { token: '{platform:0}', example: '1' },
  { token: '{platform:00}', example: '01' },
];

const episodeTokens = [
  { token: '{rom:0}', example: '1' },
  { token: '{rom:00}', example: '01' },
];

const airDateTokens = [
  { token: '{Air-Date}', example: '2016-03-20' },
  { token: '{Air Date}', example: '2016 03 20' },
];

const absoluteTokens = [
  { token: '{absolute:0}', example: '1' },
  { token: '{absolute:00}', example: '01' },
  { token: '{absolute:000}', example: '001' },
];

const romTitleTokens = [
  { token: '{Rom Title}', example: "Rom's Title", footNotes: '1' },
  { token: '{Rom CleanTitle}', example: 'Roms Title', footNotes: '1' },
];

const qualityTokens = [
  { token: '{Quality Full}', example: 'WEBDL-1080p Proper' },
  { token: '{Quality Title}', example: 'WEBDL-1080p' },
];

const mediaInfoTokens = [
  { token: '{MediaInfo Simple}', example: 'x264 DTS' },
  { token: '{MediaInfo Full}', example: 'x264 DTS [EN+DE]', footNotes: '1' },

  { token: '{MediaInfo AudioCodec}', example: 'DTS' },
  { token: '{MediaInfo AudioChannels}', example: '5.1' },
  {
    token: '{MediaInfo AudioLanguages}',
    example: '[EN+DE]',
    footNotes: '1,2',
  },
  {
    token: '{MediaInfo AudioLanguagesAll}',
    example: '[EN]',
    footNotes: '1',
  },
  { token: '{MediaInfo SubtitleLanguages}', example: '[DE]', footNotes: '1' },

  { token: '{MediaInfo VideoCodec}', example: 'x264' },
  { token: '{MediaInfo VideoBitDepth}', example: '10' },
  { token: '{MediaInfo VideoDynamicRange}', example: 'HDR' },
  { token: '{MediaInfo VideoDynamicRangeType}', example: 'DV HDR10' },
];

const otherTokens = [
  { token: '{Release Group}', example: 'Rls Grp', footNotes: '1' },
  { token: '{Custom Formats}', example: 'iNTERNAL' },
  { token: '{Custom Format:FormatName}', example: 'AMZN' },
];

const otherAnimeTokens = [{ token: '{Release Hash}', example: 'ABCDEFGH' }];

const originalTokens = [
  {
    token: '{Original Title}',
    example: "The.Game.Title's!.S01E01.WEBDL.1080p.x264-EVOLVE",
  },
  {
    token: '{Original Filename}',
    example: "the.game.title's!.s01e01.webdl.1080p.x264-EVOLVE",
  },
];

interface NamingModalProps {
  isOpen: boolean;
  name: keyof Pick<
    NamingSettingsModel,
    | 'standardEpisodeFormat'
    | 'dailyEpisodeFormat'
    | 'animeEpisodeFormat'
    | 'seriesFolderFormat'
    | 'platformFolderFormat'
    | 'specialsFolderFormat'
  >;
  value: string;
  platform?: boolean;
  rom?: boolean;
  daily?: boolean;
  anime?: boolean;
  additional?: boolean;
  onInputChange: ({ name, value }: { name: string; value: string }) => void;
  onModalClose: () => void;
}

function NamingModal(props: NamingModalProps) {
  const {
    isOpen,
    name,
    value,
    platform = false,
    rom = false,
    daily = false,
    anime = false,
    additional = false,
    onInputChange,
    onModalClose,
  } = props;

  const [tokenSeparator, setTokenSeparator] = useState<TokenSeparator>(' ');
  const [tokenCase, setTokenCase] = useState<TokenCase>('title');
  const [selectionStart, setSelectionStart] = useState<number | null>(null);
  const [selectionEnd, setSelectionEnd] = useState<number | null>(null);

  const handleTokenSeparatorChange = useCallback(
    ({ value }: { value: TokenSeparator }) => {
      setTokenSeparator(value);
    },
    [setTokenSeparator]
  );

  const handleTokenCaseChange = useCallback(
    ({ value }: { value: TokenCase }) => {
      setTokenCase(value);
    },
    [setTokenCase]
  );

  const handleInputSelectionChange = useCallback(
    (selectionStart: number | null, selectionEnd: number | null) => {
      setSelectionStart(selectionStart);
      setSelectionEnd(selectionEnd);
    },
    [setSelectionStart, setSelectionEnd]
  );

  const handleOptionPress = useCallback(
    ({
      isFullFilename,
      tokenValue,
    }: {
      isFullFilename: boolean;
      tokenValue: string;
    }) => {
      if (isFullFilename) {
        onInputChange({ name, value: tokenValue });
      } else if (selectionStart == null || selectionEnd == null) {
        onInputChange({
          name,
          value: `${value}${tokenValue}`,
        });
      } else {
        const start = value.substring(0, selectionStart);
        const end = value.substring(selectionEnd);
        const newValue = `${start}${tokenValue}${end}`;

        onInputChange({ name, value: newValue });

        setSelectionStart(newValue.length - 1);
        setSelectionEnd(newValue.length - 1);
      }
    },
    [name, value, selectionEnd, selectionStart, onInputChange]
  );

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {rom ? translate('FileNameTokens') : translate('FolderNameTokens')}
        </ModalHeader>

        <ModalBody>
          <div className={styles.namingSelectContainer}>
            <SelectInput
              className={styles.namingSelect}
              name="separator"
              value={tokenSeparator}
              values={separatorOptions}
              onChange={handleTokenSeparatorChange}
            />

            <SelectInput
              className={styles.namingSelect}
              name="case"
              value={tokenCase}
              values={caseOptions}
              onChange={handleTokenCaseChange}
            />
          </div>

          {rom ? (
            <FieldSet legend={translate('FileNames')}>
              <div className={styles.groups}>
                {daily
                  ? fileNameDailyTokens.map(({ token, example }) => (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        isFullFilename={true}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        size={sizes.LARGE}
                        onPress={handleOptionPress}
                      />
                    ))
                  : null}

                {anime
                  ? fileNameAnimeTokens.map(({ token, example }) => (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        isFullFilename={true}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        size={sizes.LARGE}
                        onPress={handleOptionPress}
                      />
                    ))
                  : null}

                {fileNameTokens.map(({ token, example }) => (
                  <NamingOption
                    key={token}
                    token={token}
                    example={example}
                    isFullFilename={true}
                    tokenSeparator={tokenSeparator}
                    tokenCase={tokenCase}
                    size={sizes.LARGE}
                    onPress={handleOptionPress}
                  />
                ))}
              </div>
            </FieldSet>
          ) : null}

          <FieldSet legend={translate('Game')}>
            <div className={styles.groups}>
              {seriesTokens.map(({ token, example, footNotes }) => (
                <NamingOption
                  key={token}
                  token={token}
                  example={example}
                  footNotes={footNotes}
                  tokenSeparator={tokenSeparator}
                  tokenCase={tokenCase}
                  onPress={handleOptionPress}
                />
              ))}
            </div>

            <div className={styles.footNote}>
              <sup className={styles.identifier}>1</sup>
              <InlineMarkdown data={translate('SeriesFootNote')} />
            </div>
          </FieldSet>

          <FieldSet legend={translate('SeriesID')}>
            <div className={styles.groups}>
              {gameIdTokens.map(({ token, example }) => (
                <NamingOption
                  key={token}
                  token={token}
                  example={example}
                  tokenSeparator={tokenSeparator}
                  tokenCase={tokenCase}
                  onPress={handleOptionPress}
                />
              ))}
            </div>
          </FieldSet>

          {platform ? (
            <FieldSet legend={translate('Platform')}>
              <div className={styles.groups}>
                {seasonTokens.map(({ token, example }) => (
                  <NamingOption
                    key={token}
                    token={token}
                    example={example}
                    tokenSeparator={tokenSeparator}
                    tokenCase={tokenCase}
                    onPress={handleOptionPress}
                  />
                ))}
              </div>
            </FieldSet>
          ) : null}

          {rom ? (
            <div>
              <FieldSet legend={translate('Rom')}>
                <div className={styles.groups}>
                  {episodeTokens.map(({ token, example }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>
              </FieldSet>

              <FieldSet legend={translate('AirDate')}>
                <div className={styles.groups}>
                  {airDateTokens.map(({ token, example }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>
              </FieldSet>

              {anime ? (
                <FieldSet legend={translate('AbsoluteRomNumber')}>
                  <div className={styles.groups}>
                    {absoluteTokens.map(({ token, example }) => (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={handleOptionPress}
                      />
                    ))}
                  </div>
                </FieldSet>
              ) : null}
            </div>
          ) : null}

          {additional ? (
            <div>
              <FieldSet legend={translate('RomTitle')}>
                <div className={styles.groups}>
                  {romTitleTokens.map(({ token, example, footNotes }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      footNotes={footNotes}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>
                <div className={styles.footNote}>
                  <sup className={styles.identifier}>1</sup>
                  <InlineMarkdown data={translate('RomTitleFootNote')} />
                </div>
              </FieldSet>

              <FieldSet legend={translate('Quality')}>
                <div className={styles.groups}>
                  {qualityTokens.map(({ token, example }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>
              </FieldSet>

              <FieldSet legend={translate('MediaInfo')}>
                <div className={styles.groups}>
                  {mediaInfoTokens.map(({ token, example, footNotes }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      footNotes={footNotes}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>

                <div className={styles.footNote}>
                  <sup className={styles.identifier}>1</sup>
                  <InlineMarkdown data={translate('MediaInfoFootNote')} />
                </div>

                <div className={styles.footNote}>
                  <sup className={styles.identifier}>2</sup>
                  <InlineMarkdown data={translate('MediaInfoFootNote2')} />
                </div>
              </FieldSet>

              <FieldSet legend={translate('Other')}>
                <div className={styles.groups}>
                  {otherTokens.map(({ token, example, footNotes }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      footNotes={footNotes}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      onPress={handleOptionPress}
                    />
                  ))}

                  {anime
                    ? otherAnimeTokens.map(({ token, example }) => (
                        <NamingOption
                          key={token}
                          token={token}
                          example={example}
                          tokenSeparator={tokenSeparator}
                          tokenCase={tokenCase}
                          onPress={handleOptionPress}
                        />
                      ))
                    : null}
                </div>

                <div className={styles.footNote}>
                  <sup className={styles.identifier}>1</sup>
                  <InlineMarkdown data={translate('ReleaseGroupFootNote')} />
                </div>
              </FieldSet>

              <FieldSet legend={translate('Original')}>
                <div className={styles.groups}>
                  {originalTokens.map(({ token, example }) => (
                    <NamingOption
                      key={token}
                      token={token}
                      example={example}
                      tokenSeparator={tokenSeparator}
                      tokenCase={tokenCase}
                      size={sizes.LARGE}
                      onPress={handleOptionPress}
                    />
                  ))}
                </div>
              </FieldSet>
            </div>
          ) : null}
        </ModalBody>

        <ModalFooter>
          <TextInput
            name={name}
            value={value}
            onChange={onInputChange}
            onSelectionChange={handleInputSelectionChange}
          />

          <Button onPress={onModalClose}>{translate('Close')}</Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default NamingModal;
