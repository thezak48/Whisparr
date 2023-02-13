import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createExistingSeriesSelector from 'Store/Selectors/createExistingSeriesSelector';
import AddNewSeriesSearchResult from './AddNewSeriesSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingSeriesSelector(),
    createDimensionsSelector(),
    (state) => state.settings.safeForWorkMode,
    (isExistingSeries, dimensions, safeForWork) => {
      return {
        isExistingSeries,
        isSmallScreen: dimensions.isSmallScreen,
        safeForWork
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewSeriesSearchResult);
