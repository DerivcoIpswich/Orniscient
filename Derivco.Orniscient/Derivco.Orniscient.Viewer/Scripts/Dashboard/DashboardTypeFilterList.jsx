var DashboardTypeFilterList = React.createClass({
    propTypes: {
        filterSelected: React.PropTypes.func
    },
    filterSelected: function (type,filterName, selected) {
        console.log('DashboardTypeFilterList filterselected');
        this.props.filterSelected(type,filterName, selected);
    },
    render: function () {
        var filterNodes = this.props.data.map(function (filter) {
            //console.log(filter);
            return (<DashboardTypeFilter data={filter} key={filter.TypeNameShort} filterSelected={this.filterSelected } />);
        },this);
        return (
        <div className="">
                {filterNodes}
        </div>);
    }
});