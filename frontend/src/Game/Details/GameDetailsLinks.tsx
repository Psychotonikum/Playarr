import React, { useMemo } from 'react';
import Label from 'Components/Label';
import ClipboardButton from 'Components/Link/ClipboardButton';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import Game from 'Game/Game';
import translate from 'Utilities/String/translate';
import styles from './GameDetailsLinks.css';

type GameDetailsLinksProps = Pick<
  Game,
  'igdbId' | 'rawgId'
>;

interface GameDetailsLink {
  externalId: string | number;
  name: string;
  url: string;
}

function GameDetailsLinks(props: GameDetailsLinksProps) {
  const { igdbId, rawgId } = props;

  const links = useMemo(() => {
    const validLinks: GameDetailsLink[] = [];

    if (igdbId) {
      validLinks.push(
        {
          externalId: igdbId,
          name: 'IGDB',
          url: `https://www.igdb.com/games/${igdbId}`,
        }
      );
    }

    if (rawgId) {
      validLinks.push({
        externalId: rawgId,
        name: 'RAWG',
        url: `https://rawg.io/games/${rawgId}`,
      });
    }

    return validLinks;
  }, [igdbId, rawgId]);

  return (
    <div className={styles.links}>
      {links.map((link) => (
        <div key={link.name} className={styles.linkBlock}>
          <Link className={styles.link} to={link.url}>
            <Label
              className={styles.linkLabel}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              {link.name}
            </Label>
          </Link>

          <ClipboardButton
            value={`${link.externalId}`}
            title={translate('CopyToClipboard')}
            kind={kinds.DEFAULT}
            size={sizes.SMALL}
            label={link.externalId}
          />
        </div>
      ))}
    </div>
  );
}

export default GameDetailsLinks;
