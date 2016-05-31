/*
    This is the filter grouping for the type.
*/
var DashboardTypeFilter = React.createClass({
    render: function () {
        var filters = this.props.data.Filters.map(function (filter) {

            //need to get the values into the correct format....
            var filterValues = orniscientutils.stringArrToSelectOptions(filter.Values);
            return (
                <div className="form-group" key={filter.FilterName}>
                    <label>{filter.FilterName}</label>
                    <Select name="form-field-name" options={filterValues} multi={true} onChange={this.typeSelected} />
                </div>
            );
        });

        return (
            <div>
                <h4>{this.props.data.TypeName}</h4>
                <hr />
                {filters}
            </div>
        );
    }

})