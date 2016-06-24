

var Dashboard = React.createClass({

    filterByGrainId: function (event) {
        this.setState({ grainIdFilter: event.target.value });
        //$.each(orniscient.data.nodes.get(), function (index, grainData) {

        //    grainData.hidden = grainData.label.indexOf(event.target.value) === -1;
        //    orniscient.data.nodes.update(grainData);

        //    //need to remove all the edges where this grain was the from grain.
        //    var affectedEdges = orniscient.data.edges.get({
        //        filter: function (item) {
        //            return item.from === grainData.id || item.to === grainData.id;
        //        }
        //    });

        //    $.each(affectedEdges, function (index, edge) {
        //        edge.hidden = grainData.hidden;
        //    });
        //    orniscient.data.edges.update(affectedEdges);
        //});
    },
    siloSelected(val) {
        console.log("Selected Silo: " + val);
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

        if (this.state.selectedTypes != undefined) {
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

        //TODO : Add the silo filter here as well.
        orniscient.getServerData(filter);
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
        console.log('orniscientUpdated was called, need to re-render the form now......');
        this.setState({ typeCounts: typeCounts.detail });
    },
    componentDidMount: function () {
        console.log('componentDidMount is called.');
        window.addEventListener('orniscientUpdated', this.orniscientUpdated);
        orniscient.init();
    },

    render: function () {
        return (
            <div className="container bigContainer ">
                <div className="row">

                    <div className="col-md-3 padding20">
                        <div className="row">
                             <div className="form-group">
                                    <label for="silo">Cluster</label>
                                     <select className="form-control width100" id="silo">
                                         <option>United Kingdom</option>
                                         <option>South Africa</option>
                                     </select>
                             </div>
                            <button type="submit" className="btn btn-default pull-right btn-success">Go</button>
                        </div>
                        <div className="row">
                            <h3>Filter options</h3>
                            <hr />
                            <form>
                                 <div className="form-group">
                                    <label for="silo">Silo</label>
                                    <Select name="form-field-name" options={this.state.silos} multi={true} onChange={this.siloSelected} disabled={false} value={ this.state.selectedSilos } />
                                 </div>
                                 <div className="form-group">
                                    <label for="grainType">Grain Type</label>
                                    <Select name="form-field-name" options={this.state.availableTypes} multi={true} onChange={this.getFilters} disabled={false} value={ this.state.selectedTypes } />
                                 </div>
                                <div className="form-group">
                                    <label for="grainid">Grain Id</label>
                                    <input type="text" className="form-control width100" id="grainid" placeholder="Grain Id" onChange={this.filterByGrainId} />
                                </div>

                                <DashboardTypeFilterList data={this.state.availableFilters} filterSelected={this.filterSelected} />
                                <button type="submit" className="btn btn-default  btn-success" onClick={this.searchClicked}>Search</button>

                                <DashboardTypeCounts data={this.state.typeCounts} />
                            </form>

                        </div>
                    </div>
                    <div className="col-md-9" id="mynetwork">

                    </div>
                </div>
            </div>
        );
    }
});

ReactDOM.render(
        <Dashboard />,
        document.getElementById('dashboard')
    )