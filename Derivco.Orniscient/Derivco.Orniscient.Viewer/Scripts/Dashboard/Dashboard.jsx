var Dashboard = React.createClass({
    render: function() {
        return (
            <div className="container bigContainer ">
                <div className="row">
                    <div className="col-md-3 padding20">
                        <div className="row">
                            <h3>Filter options</h3>
                            <hr />
                        </div>
                    </div>
                    <div className="col-md-9" id="mynetwork">

                    </div>
                </div>
            </div>
        );
    },
    ComponentDidMount: function () {
        console.log('componentDidUpdate called');
        orniscient.init();
    }
});

ReactDOM.render(
        <Dashboard />,
        document.getElementById('dashboard')
    )