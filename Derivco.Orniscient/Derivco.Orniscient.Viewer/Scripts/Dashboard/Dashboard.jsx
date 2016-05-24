var Dashboard = React.createClass({
    filterByGrainId: function (event) {
        $.each(orniscient.data.nodes.get(), function (index, grainData) {

            grainData.hidden = grainData.label.indexOf(event.target.value) === -1;
            orniscient.data.nodes.update(grainData);

            //need to remove all the edges where this grain was the from grain.
            var affectedEdges = orniscient.data.edges.get({
                filter: function(item) {
                    return item.from === grainData.id || item.to===grainData.id;
                }
            });

            $.each(affectedEdges, function(index, edge) {
                edge.hidden = grainData.hidden;
            });
            orniscient.data.edges.update(affectedEdges);
        });
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
                            <h3>Filter options</h3>
                            <hr />
                            <form>
                                 <div className="form-group">
                                    <label for="grainType">Grain Type</label>
                                    <input type="text" className="form-control width100" id="grainType" placeholder="Grain Type" />
                                 </div>
                                <div className="form-group">
                                    <label for="grainid">Grain Id</label>
                                    <input type="text" className="form-control width100" id="grainid" placeholder="Grain Id" onChange={this.filterByGrainId} />
                                </div>
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