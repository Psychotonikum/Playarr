import translate from 'Utilities/String/translate';

export default function getFinaleTypeName(finaleType?: string): string | null {
  switch (finaleType) {
    case 'game':
      return translate('SeriesFinale');
    case 'platform':
      return translate('SeasonFinale');
    case 'midseason':
      return translate('MidseasonFinale');
    default:
      return null;
  }
}
