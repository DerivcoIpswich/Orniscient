var DashboardGrainMethodReturnData = React.createClass({
    render: function () {
        var methodReturnData = this.props.data;

        var renderData = methodReturnData !== null && methodReturnData.indexOf('Error: ') === -1 ?
            (<div className="panel panel-success"><div className="panel-heading"><strong>The method was invoked successfully.</strong></div><div className="panel-body">{methodReturnData === '' ? 'No data was returned.' : methodReturnData}</div></div>) :
            (<div className="panel panel-danger"><div className="panel-heading"><strong>An error occured when invoking the method.</strong></div><div className="panel-body">{methodReturnData}</div></div>);

        return (
            methodReturnData !== null &&
            (<div className=""><h5>Result</h5>{renderData}</div>)
        );
    }
});