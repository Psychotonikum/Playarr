import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';

export default function formatPlatform(
  platformNumber: number,
  shortFormat?: boolean
) {
  if (platformNumber === 0) {
    return translate('Specials');
  }

  if (platformNumber > 0) {
    return shortFormat
      ? `S${padNumber(platformNumber, 2)}`
      : translate('PlatformNumberToken', { platformNumber });
  }

  return null;
}
