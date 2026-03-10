import classNames from 'classnames';
import React from 'react';
import { ColorImpairedConsumer } from 'App/ColorImpairedContext';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import useGame from 'Game/useGame';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './GameIndexFooter.css';

export default function GameIndexFooter() {
  const { data: game } = useGame();
  const count = game.length;
  let roms = 0;
  let romFiles = 0;
  let ended = 0;
  let continuing = 0;
  let monitored = 0;
  let totalFileSize = 0;

  game.forEach((s) => {
    const { statistics = { romCount: 0, romFileCount: 0, sizeOnDisk: 0 } } = s;

    const { romCount = 0, romFileCount = 0, sizeOnDisk = 0 } = statistics;

    roms += romCount;
    romFiles += romFileCount;

    if (s.status === 'ended') {
      ended++;
    } else {
      continuing++;
    }

    if (s.monitored) {
      monitored++;
    }

    totalFileSize += sizeOnDisk;
  });

  return (
    <ColorImpairedConsumer>
      {(enableColorImpairedMode) => {
        return (
          <div className={styles.footer}>
            <div>
              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.continuing,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('GameIndexFooterContinuing')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.ended,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('GameIndexFooterEnded')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.missingMonitored,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('GameIndexFooterMissingMonitored')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.missingUnmonitored,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('GameIndexFooterMissingUnmonitored')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.downloading,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('GameIndexFooterDownloading')}</div>
              </div>
            </div>

            <div className={styles.statistics}>
              <DescriptionList>
                <DescriptionListItem title={translate('Game')} data={count} />

                <DescriptionListItem title={translate('Ended')} data={ended} />

                <DescriptionListItem
                  title={translate('Continuing')}
                  data={continuing}
                />
              </DescriptionList>

              <DescriptionList>
                <DescriptionListItem
                  title={translate('Monitored')}
                  data={monitored}
                />

                <DescriptionListItem
                  title={translate('Unmonitored')}
                  data={count - monitored}
                />
              </DescriptionList>

              <DescriptionList>
                <DescriptionListItem title={translate('Roms')} data={roms} />

                <DescriptionListItem
                  title={translate('Files')}
                  data={romFiles}
                />
              </DescriptionList>

              <DescriptionList>
                <DescriptionListItem
                  title={translate('TotalFileSize')}
                  data={formatBytes(totalFileSize)}
                />
              </DescriptionList>
            </div>
          </div>
        );
      }}
    </ColorImpairedConsumer>
  );
}
