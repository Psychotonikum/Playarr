import Rom from 'Rom/Rom';
import { update } from 'Store/Actions/baseActions';

function updateEpisodes(
  section: string,
  roms: Rom[],
  romIds: number[],
  options: Partial<Rom>
) {
  const data = roms.reduce<Rom[]>((result, item) => {
    if (romIds.indexOf(item.id) > -1) {
      result.push({
        ...item,
        ...options,
      });
    } else {
      result.push(item);
    }

    return result;
  }, []);

  return update({ section, data });
}

export default updateEpisodes;
