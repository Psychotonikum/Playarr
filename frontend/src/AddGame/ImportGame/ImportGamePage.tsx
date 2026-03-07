import React from 'react';
import { Route } from 'react-router-dom';
import Switch from 'Components/Router/Switch';
import ImportGame from './Import/ImportGame';
import ImportGameSelectFolder from './SelectFolder/ImportGameSelectFolder';

function ImportGamePage() {
  return (
    <Switch>
      <Route
        exact={true}
        path="/add/import"
        component={ImportGameSelectFolder}
      />

      <Route path="/add/import/:rootFolderId" component={ImportGame} />
    </Switch>
  );
}

export default ImportGamePage;
