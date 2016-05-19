$(function () {

    var hub = $.connection.orniscientHub,
        nodes = new vis.DataSet([]),
        container = document.getElementById('mynetwork');

    var options = {
        autoResize: true,
        height: '100%',
        nodes: {
            shape: 'dot',
            scaling: {
                min: 10,
                max: 30,
                label: {
                    min: 8,
                    max: 30,
                    drawThreshold: 12,
                    maxVisible: 20
                }
            }
        },
        interaction: {
            hover: true,
            navigationButtons: true
        },
        layout: {
            randomSeed: 2
        }
    };
    var data = {
        nodes: nodes,
        //edges: edges
    };

    var network = new vis.Network(container, data, options);
    network.on("selectNode", function (params) {
        if (params.nodes.length == 1) {
            if (network.isCluster(params.nodes[0]) == true) {
                network.openCluster(params.nodes[0]);
            }
        }
    });
    var i = 1;
    $.connection.hub.start()
		.then(init);

    function init() {
        return hub.server.getCurrentSnapshot()
            .done(function (data) {
                
                $.each(data, function(index, value) {
                    nodes.add({
                        id: i,
                        label: value.GrainName,
                        color: '#FFFF00',
                        silo: 'C'
                    });
                    i++;
                });
            })
            .fail(function(data) {
                alert('Oops, we cannot connect to the server...');
            });
    }

    //add client side methods for updates
    $.extend(hub.client, {
        grainActivationChanged: function (diffModel) {
            $.each(diffModel.NewGrains, function (index,grainData) {
                nodes.add({
                    id: i,
                    label: grainData.GrainName,
                    color: '#FF0000',
                    silo: 'C'
                });
                i++;
            });
        }
    });

});