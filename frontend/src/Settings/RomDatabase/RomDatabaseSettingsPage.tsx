import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbar from 'Settings/SettingsToolbar';
import RomDatabaseSystems from './RomDatabaseSystems';

function RomDatabaseSettingsPage() {
  return (
    <PageContent title="ROM Databases">
      <SettingsToolbar showSave={false} />

      <PageContentBody>
        <RomDatabaseSystems />
      </PageContentBody>
    </PageContent>
  );
}

export default RomDatabaseSettingsPage;
