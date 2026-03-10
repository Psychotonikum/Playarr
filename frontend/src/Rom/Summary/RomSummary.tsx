import { useQueryClient } from '@tanstack/react-query';
import React, { useCallback, useEffect } from 'react';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import Game from 'Game/Game';
import { useSingleGame } from 'Game/useGame';
import { icons, kinds, sizes } from 'Helpers/Props';
import Rom from 'Rom/Rom';
import useRom, { RomEntity } from 'Rom/useRom';
import { useRomFile } from 'RomFile/RomFileProvider';
import { useDeleteRomFile } from 'RomFile/useRomFiles';
import QualityProfileName from 'Settings/Profiles/Quality/QualityProfileName';
import translate from 'Utilities/String/translate';
import RomAiring from './RomAiring';
import RomFileRow from './RomFileRow';
import styles from './RomSummary.css';

const COLUMNS: Column[] = [
  {
    name: 'path',
    label: () => translate('Path'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'customFormats',
    label: () => translate('Formats'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: () => translate('CustomFormatScore'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isSortable: false,
    isVisible: true,
  },
];

interface RomSummaryProps {
  gameId: number;
  romId: number;
  romEntity: RomEntity;
  romFileId?: number;
}

function RomSummary({ gameId, romId, romEntity, romFileId }: RomSummaryProps) {
  const queryClient = useQueryClient();
  const { qualityProfileId, network } = useSingleGame(gameId) as Game;

  const { airDateUtc, overview } = useRom(romId, romEntity) as Rom;

  const {
    path,
    mediaInfo,
    size,
    languages,
    quality,
    qualityCutoffNotMet,
    customFormats,
    customFormatScore,
  } = useRomFile(romFileId) ?? {};

  const { deleteRomFile } = useDeleteRomFile(romFileId!, romEntity);

  const handleDeleteRomFile = useCallback(() => {
    deleteRomFile();
  }, [deleteRomFile]);

  useEffect(() => {
    if (romFileId && !path) {
      queryClient.invalidateQueries({ queryKey: ['/romFile'] });
    }
  }, [romFileId, path, queryClient]);

  const hasOverview = !!overview;

  return (
    <div>
      <div>
        <span className={styles.infoTitle}>{translate('Airs')}</span>

        <RomAiring airDateUtc={airDateUtc} network={network} />
      </div>

      <div>
        <span className={styles.infoTitle}>{translate('QualityProfile')}</span>

        <Label kind={kinds.PRIMARY} size={sizes.MEDIUM}>
          <QualityProfileName qualityProfileId={qualityProfileId} />
        </Label>
      </div>

      <div className={styles.overview}>
        {hasOverview ? overview : translate('NoEpisodeOverview')}
      </div>

      {path ? (
        <Table columns={COLUMNS}>
          <TableBody>
            <RomFileRow
              path={path}
              size={size!}
              languages={languages!}
              quality={quality!}
              qualityCutoffNotMet={qualityCutoffNotMet!}
              customFormats={customFormats!}
              customFormatScore={customFormatScore!}
              mediaInfo={mediaInfo!}
              columns={COLUMNS}
              onDeleteRomFile={handleDeleteRomFile}
            />
          </TableBody>
        </Table>
      ) : null}
    </div>
  );
}

export default RomSummary;
