import createAjaxRequest from 'Utilities/createAjaxRequest';
import updateEpisodes from 'Utilities/Rom/updateRoms';
import getSectionState from 'Utilities/State/getSectionState';

function createBatchToggleEpisodeMonitoredHandler(section, fetchHandler) {
  return function(getState, payload, dispatch) {
    const {
      romIds,
      monitored
    } = payload;

    const state = getSectionState(getState(), section, true);

    dispatch(updateEpisodes(section, state.items, romIds, {
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/rom/monitor',
      method: 'PUT',
      data: JSON.stringify({ romIds, monitored }),
      dataType: 'json'
    }).request;

    promise.done(() => {
      dispatch(updateEpisodes(section, state.items, romIds, {
        isSaving: false,
        monitored
      }));

      dispatch(fetchHandler());
    });

    promise.fail(() => {
      dispatch(updateEpisodes(section, state.items, romIds, {
        isSaving: false
      }));
    });
  };
}

export default createBatchToggleEpisodeMonitoredHandler;
