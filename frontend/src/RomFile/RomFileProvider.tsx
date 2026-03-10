import React, {
  createContext,
  PropsWithChildren,
  useContext,
  useMemo,
} from 'react';
import { RomFile } from './RomFile';
import useRomFiles, { RomFileFilter } from './useRomFiles';

export const RomFileContext = createContext<RomFile[] | undefined>(undefined);

export default function RomFileProvider({
  children,
  ...filter
}: PropsWithChildren<RomFileFilter>) {
  const { data } = useRomFiles(filter);

  return (
    <RomFileContext.Provider value={data}>{children}</RomFileContext.Provider>
  );
}

export function useRomFile(id: number | undefined) {
  const romFiles = useContext(RomFileContext);

  return useMemo(() => {
    if (id === undefined) {
      return undefined;
    }

    return romFiles?.find((item) => item.id === id);
  }, [id, romFiles]);
}

export interface SeriesRomFile {
  count: number;
  episodesWithFiles: number;
}
