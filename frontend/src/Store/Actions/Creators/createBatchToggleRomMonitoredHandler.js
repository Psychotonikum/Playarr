import createAjaxRequest from 'Utilities/createAjaxRequest';
import updateRoms from 'Utilities/Rom/updateRoms';
import getSectionState from 'Utilities/State/getSectionState';

function createBatchToggleRomMonitoredHandler(section, fetchHandler) {
  return function(getState, payload, dispatch) {
    const {
      romIds,
      monitored
    } = payload;

    const state = getSectionState(getState(), section, true);

    dispatch(updateRoms(section, state.items, romIds, {
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/rom/monitor',
      method: 'PUT',
      data: JSON.stringify({ romIds, monitored }),
      dataType: 'json'
    }).request;

    promise.done(() => {
      dispatch(updateRoms(section, state.items, romIds, {
        isSaving: false,
        monitored
      }));

      dispatch(fetchHandler());
    });

    promise.fail(() => {
      dispatch(updateRoms(section, state.items, romIds, {
        isSaving: false
      }));
    });
  };
}

export default createBatchToggleRomMonitoredHandler;
