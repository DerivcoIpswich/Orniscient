var DashboardTypeFilterList = React.createClass({
    render: function () {
        var filterNodes = this.props.data.map(function (filter) {
            console.log(filter);
            return (<DashboardTypeFilter data={filter} key={filter.TypeName } />);
        });
        return (
        <div className="">
                {filterNodes}
        </div>);
    }
});