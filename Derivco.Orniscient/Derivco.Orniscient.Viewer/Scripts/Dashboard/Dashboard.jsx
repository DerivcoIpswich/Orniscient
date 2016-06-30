

var Dashboard = React.createClass({
    filterByGrainId: function (event) {
        this.setState({ grainIdFilter: event.target.value });
    },
    siloSelected(val) {
        //console.log("Selected Silo: " + val);
        this.setState({ selectedSilos: val });
    },
    getFilters: function (selectedTypes) {

        var requestData = {
            Types: selectedTypes != null && selectedTypes.length > 0 ? selectedTypes.map(function (a) { return a.value; }) : {}
        };

        var xhr = new XMLHttpRequest();
        xhr.open('post', orniscienturls.getFilters, true);
        xhr.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
        xhr.onload = function () {
            var filters = [];
            if (xhr.responseText != null && xhr.responseText !== "") {
                filters = JSON.parse(xhr.responseText);
            }

            //TODO :  delete all the types from the selectedFilters so that state is not maintained
            this.setState({
                availableFilters: filters,
                selectedTypes: selectedTypes
            });
        }.bind(this);
        xhr.send(JSON.stringify(requestData));
    },
    filterSelected: function (type, filterName, selected) {

        var selectedFilters = this.state.selectedFilters;
        var filterid = filterName.replace(/[^\w]/gi, '.'); //remove special characters

        if (selectedFilters[type] === undefined) {
            selectedFilters[type] = {};
        }

        if (selectedFilters[type][filterid] === undefined) {
            selectedFilters[type][filterid] = {};
        }

        selectedFilters[type][filterid] = selected;
        this.setState({ selectedFilters: selectedFilters });
    },
    searchClicked: function (e) {
        e.preventDefault();

        //build the filter here.
        var selectedFilters = this.state.selectedFilters;
        var filter = {
            GrainId: this.state.grainIdFilter
            
        }

        if (this.state.selectedSilos != null) {
            filter.SelectedSilos= this.state.selectedSilos.map(function(silo) { return silo.value });
        }

        if (this.state.selectedTypes != null) {
            filter.TypeFilters = this.state.selectedTypes.map(function (type) {
                var selectedValues = {};
                var selectedValuesForType = selectedFilters[type.value];
                for (var key in selectedValuesForType) {
                    selectedValues[key] = selectedValuesForType[key].map(function (i) { return i.value });
                }
                return {
                    TypeName: type.value,
                    SelectedValues: selectedValues
                };
            });
        }

        orniscient.getServerData(filter);
    },
    clearFiltersClicked :function(e) {
        e.preventDefault();
        //clear everything that was set with this.setState({})
        this.setState({
            grainIdFilter: '',
            selectedSilos: null,            
            selectedTypes: null,
            availableFilters: [],
            selectedFilters: {}
    });

        orniscient.getServerData();
    },
    //React methods
    childContextTypes: {
        selectedFilters: React.PropTypes.object
    },
    getChildContext: function () {
        return { selectedFilters: this.state.selectedFilters };
    },
    getInitialState: function () {
        return {
            grainIdFilter: '',
            selectedSilos: null,
            selectedTypes: null,
            silos: [],
            availableTypes: [],
            availableFilters: [],
            selectedFilters: {},
            typeCounts: []
        };
    },
    componentWillMount: function () {
        console.log('componentWillMount is callled');
        var xhr = new XMLHttpRequest();
        xhr.open('get', orniscienturls.dashboardInfo, true);
        xhr.onload = function () {
            var data = JSON.parse(xhr.responseText);
            this.setState({
                silos: orniscientutils.stringArrToSelectOptions(data.Silos),
                availableTypes: orniscientutils.stringArrToFilterNames(data.AvailableTypes)
            });
        }.bind(this);
        xhr.send();
    },
    orniscientUpdated: function (typeCounts, a, b) {
        //console.log('orniscientUpdated was called, need to re-render the form now......');
        this.setState({ typeCounts: typeCounts.detail });
    },
    componentDidMount: function () {
        //console.log('componentDidMount is called.');
        window.addEventListener('orniscientUpdated', this.orniscientUpdated);
        console.log('Init orniscient now');
        orniscient.init();
    },

    render: function () {
        return (
            <div id="filterwrap">
                    <div className="container bigContainer ">
                        <div className="row">
                            <div className="" id="mynetwork">
                            </div>
                        </div>
                    </div>
                    <div className="filterFlyout">
                        <div className="container">
                        <div className="floatButton">
                            <button className="btn btn-default togglefilter"><span className="glyphicon glyphicon-chevron-right"></span></button>
                        </div>
                         <div className="row">
                            <div className="col-md-12">
                            <h4>Filter options</h4>
                            <form>
                                <div className="form-group">
                                    <label for="grainid">Grain Id</label>
                                    <input type="text" className="form-control width100" id="grainid" placeholder="Grain Id" onChange={this.filterByGrainId} value={this.state.grainIdFilter}/>
                                </div>
                                <div className="form-group">
                                    <label for="silo">Silo</label>
                                    <Select name="form-field-name" options={this.state.silos} multi={true} onChange={this.siloSelected} disabled={false} value={ this.state.selectedSilos } />
                                </div>
                                <div className="form-group">
                                    <label for="grainType">Grain Type</label>
                                    <Select name="form-field-name" options={this.state.availableTypes} multi={true} onChange={this.getFilters} disabled={false} value={ this.state.selectedTypes } />
                                </div>
                                <DashboardTypeFilterList data={this.state.availableFilters} filterSelected={this.filterSelected} />
                                <div className="row">
                                    <div className="col-md-12">
                                        <button type="submit" className="btn btn-success pull-right" onClick={this.searchClicked}>Search</button>
                                    </div>
                                    <div className="col-md-12">
                                        <button type="submit" className="btn btn-danger pull-left" onClick={this.clearFiltersClicked}>Clear Filters</button>
                                     </div>
                                </div>
                                <DashboardTypeCounts data={this.state.typeCounts} />
                            </form>
                            </div>
                         </div>
                        </div>
                    </div>
            </div>
        );
    }
});

ReactDOM.render(
    <Dashboard />,
    document.getElementById('dashboard')
);


$(document).ready(function () {

    $(document).on('click', '.togglefilter', function (e) {
        e.preventDefault();

        var $filterLayout = $('.filterFlyout');
        $filterLayout.toggleClass('shown');

        if ($filterLayout.hasClass('shown')) {
            $filterLayout.find('.glyphicon').removeClass('glyphicon-chevron-right').addClass('glyphicon-chevron-left');
        } else {
            $filterLayout.find('.glyphicon').removeClass('glyphicon-chevron-left').addClass('glyphicon-chevron-right');
        }
    });
});