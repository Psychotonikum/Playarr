import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import translate from 'Utilities/String/translate';

function GameMonitoringOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title={translate('MonitorAllEpisodes')}
        data={translate('MonitorAllEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorFutureEpisodes')}
        data={translate('MonitorFutureEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorMissingEpisodes')}
        data={translate('MonitorMissingEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorSpecialEpisodes')}
        data={translate('MonitorSpecialEpisodesDescription')}
      />
    </DescriptionList>
  );
}

export default GameMonitoringOptionsPopoverContent;
