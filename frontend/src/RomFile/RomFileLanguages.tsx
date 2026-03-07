import React from 'react';
import RomLanguages from 'Rom/RomLanguages';
import { useRomFile } from './RomFileProvider';

interface RomFileLanguagesProps {
  romFileId: number | undefined;
}

function RomFileLanguages({ romFileId }: RomFileLanguagesProps) {
  const romFile = useRomFile(romFileId);

  return <RomLanguages languages={romFile?.languages ?? []} />;
}

export default RomFileLanguages;
