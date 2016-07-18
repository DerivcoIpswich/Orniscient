var DashboardGrainMethodParameters = React.createClass({
    render: function() {
        var methodData = this.props.data;
        var displayParameters = (<label for="parameters">Select a method.</label>);

        if (methodData != null) {
            if (methodData.parameters.length > 0) {
                displayParameters = methodData.parameters.map(function(parameter) {
                    return (
                            <div className="" key={parameter.Name}>
                                <label for="parameter" className="">{parameter.Name} : </label>
                                <input type="text" className="form-control paramInput" id={parameter.Name} placeholder={parameter.Type}/>
                            </div>
                            );
                }, this);
            } else {
                displayParameters = (<label for="parameters">{methodData.label} has no Orniscient methods.</label>);
            }
        }

        return (
            <div className="">
                <h5>Parameters</h5>
                    {displayParameters}
    </div>);
    }
});