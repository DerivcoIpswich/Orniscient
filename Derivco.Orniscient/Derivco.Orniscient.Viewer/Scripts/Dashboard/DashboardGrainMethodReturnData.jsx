var DashboardGrainMethodReturnData = React.createClass({
    render: function() {
        if (this.props.data != undefined) {
            var methodReturnData = this.props.data;
            var panelClassName = '',
                panelMessage = '',
                methodResult = '';

            if (methodReturnData !== null && methodReturnData.indexOf('Error: ') === -1) {
                panelClassName = 'panel panel-success';
                panelMessage = 'The method was invoked successfully.';
            } else {
                panelClassName = 'panel panel-danger';
                panelMessage = 'An error occured when invoking the method.';
            }

            if (methodReturnData === '') {
                methodResult = <div className="panel-body">No data was returned.</div>;
            } else {
                try {

                    var retObj = JSON.parse(methodReturnData);
                    methodResult = <div className="panel-body"><pre className="result">{JSON.stringify(retObj, null, '\t')}</pre></div>;
                } catch (exception) {
                    methodResult = <div className="panel-body">{methodReturnData}</div>;
                }
            }

            return (
                methodReturnData !== null &&
                (<div className="">
                    <h5>Result</h5>
                    <div className={panelClassName}>
                        <div className="panel-heading">
                            <strong>{panelMessage}</strong>
                        </div>
                       {methodResult}
                    </div>
                </div>)
            );
        }
        return (<div></div>);
    }
});