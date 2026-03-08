import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbar from 'Settings/SettingsToolbar';
import GameSystems from './GameSystems';

function GameSystemSettingsPage() {
  return (
    <PageContent title="Game Systems">
      <SettingsToolbar showSave={false} />

      <PageContentBody>
        <GameSystems />
      </PageContentBody>
    </PageContent>
  );
}

export default GameSystemSettingsPage;
