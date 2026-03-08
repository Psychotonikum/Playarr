import React from 'react';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import translate from 'Utilities/String/translate';
import styles from './TheIgdb.css';

function TheIgdb() {
  return (
    <div className={styles.container}>
      <img
        className={styles.image}
        src={`${window.Playarr.urlBase}/Content/Images/theigdb.png`}
      />

      <div className={styles.info}>
        <div className={styles.title}>{translate('TheIgdb')}</div>
        <InlineMarkdown
          data={translate('SeriesAndRomInformationIsProvidedByTheIGDB', {
            url: 'https://www.theigdb.com/subscribe',
          })}
        />
      </div>
    </div>
  );
}

export default TheIgdb;
