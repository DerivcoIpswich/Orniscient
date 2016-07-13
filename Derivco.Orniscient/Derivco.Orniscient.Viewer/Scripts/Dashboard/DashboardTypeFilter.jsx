/*
    This is the filter grouping for the type.
*/
var DashboardTypeFilter = React.createClass({
    propTypes: {
        filterSelected: React.PropTypes.func
    },
   
    filterSelected: function (filterName,val ) {
        console.log('dashboardTypeFilter filterselected');
        this.props.filterSelected(this.props.data.TypeName,filterName, val);
    },
    render: function() {
        var filters = this.props.data.Filters.map(function (filter) {
            return (<DashboardTypeFilterRow data={filter} filterSelected={this.filterSelected} key={filter.FilterName } typeName={this.props.data.TypeName}/>);
        },this);

        return (
            <div>
                <h5>{this.props.data.TypeNameShort}</h5>
                {filters}
            </div>
        );
    }

});

var DashboardTypeFilterRow = React.createClass({
    contextTypes: {
        selectedFilters: React.PropTypes.object
    },
    propTypes: {
        filterSelected: React.PropTypes.func
    },
    getselectedfilters() {

        var selectedFilters = this.context.selectedFilters;
        if (selectedFilters === undefined) {
            return null;
        }

        if (selectedFilters[this.props.typeName] === undefined) {
            return null;
        }
        var filterid = this.props.data.FilterName.replace(/[^\w]/gi, '.'); //remove special characters
        if (selectedFilters[this.props.typeName][filterid] === undefined) {
            return null;
        }

        return selectedFilters[this.props.typeName][filterid];
    },
    filterSelected : function(val) {
        this.props.filterSelected(this.props.data.FilterName,val);
    },
    getInitialState: function () {
        return {
            SelectedValue :[]
        }
    },
    render: function() {
        var filterValues = orniscientutils.stringArrToSelectOptions(this.props.data.Values);
        return (
            <div className="form-group" key={this.props.data.FilterName}>
                    <label>{this.props.data.FilterName}</label>
                    <Select name="form-field-name" options={filterValues} multi={true} onChange={this.filterSelected} value={this.getselectedfilters()} />
                </div>
            );
    }
});