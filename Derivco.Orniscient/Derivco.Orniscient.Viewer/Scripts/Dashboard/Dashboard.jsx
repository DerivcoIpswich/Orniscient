

var Dashboard = React.createClass({
    filterByGrainId: function (event) {
        $.each(orniscient.data.nodes.get(), function (index, grainData) {

            grainData.hidden = grainData.label.indexOf(event.target.value) === -1;
            orniscient.data.nodes.update(grainData);

            //need to remove all the edges where this grain was the from grain.
            var affectedEdges = orniscient.data.edges.get({
                filter: function (item) {
                    return item.from === grainData.id || item.to === grainData.id;
                }
            });

            $.each(affectedEdges, function (index, edge) {
                edge.hidden = grainData.hidden;
            });
            orniscient.data.edges.update(affectedEdges);
        });
    },
    siloSelected(val) {
        console.log("Selected Silo: " + val);
        this.setState({ selectedSilos: val });
    },
    typeSelected(val) {
        this.setState({ selectedTypes: val });
        console.log("Selected Type : " + val);
    },
    stringArrToSelectOptions(arr) {
        return arr.map(function (obj) {
            return {
                value: obj,
                label: obj
            }
        });
    },
    getInitialState: function () {
        return {
            data: {
                Silos: [],
                AvailableTypes : []
            }
        };
    },
    componentWillMount: function () {
        var xhr = new XMLHttpRequest();
        xhr.open('get', this.props.url, true);
        xhr.onload = function () {
            var data = JSON.parse(xhr.responseText);
            this.setState({
                data: {
                    Silos: this.stringArrToSelectOptions(data.Silos),
                    AvailableTypes: this.stringArrToSelectOptions(data.AvailableTypes)
                }
            });
        }.bind(this);
        xhr.send();
    },
    componentDidMount: function () {
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
                                    <Select name="form-field-name" options={this.state.data.Silos} multi={true} onChange={this.siloSelected} disabled={false} value={ this.state.selectedSilos } />
                                 </div>
                                 <div className="form-group">
                                    <label for="grainType">Grain Type</label>
                                    <Select name="form-field-name" options={this.state.data.AvailableTypes} multi={true} onChange={this.typeSelected} disabled={false} value={ this.state.selectedTypes } />
                                 </div>
                                <div className="form-group">
                                    <label for="grainid">Grain Id</label>
                                    <input type="text" className="form-control width100" id="grainid" placeholder="Grain Id" onChange={this.filterByGrainId} />
                                </div>

                                 <button type="submit" className="btn btn-default pull-right btn-success">Search</button>
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
        <Dashboard url="GetDashboardInfo" />,
        document.getElementById('dashboard')
    )