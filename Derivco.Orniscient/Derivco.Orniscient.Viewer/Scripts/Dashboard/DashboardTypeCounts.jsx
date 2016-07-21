var DashboardTypeCounts = React.createClass({
    render: function () {

        var items = this.props.data.map(function (typeCounter) {
            return (<li className="" key={typeCounter.TypeName }>{typeCounter.TypeName} : <strong>{typeCounter.Total} </strong></li>);

        });

        return (
            <div>
                <h4>Grain Type counts</h4>
                <hr />
                <ul className="">
                    {items}
                </ul>
            </div>
        );
    }
});