import React, { PropsWithChildren } from 'react';
import QueueDetailsProvider from 'Activity/Queue/Details/QueueDetailsProvider';
import { RomFileContext } from 'RomFile/RomFileProvider';
import useRomFiles from 'RomFile/useRomFiles';

function GameDetailsProvider({
  gameId,
  children,
}: PropsWithChildren<{ gameId: number }>) {
  const { data } = useRomFiles({ gameId });

  return (
    <QueueDetailsProvider gameId={gameId}>
      <RomFileContext.Provider value={data}>{children}</RomFileContext.Provider>
    </QueueDetailsProvider>
  );
}

export default GameDetailsProvider;
