
(function (orniscient, $, undefined) {

    var hub = $.connection.orniscientHub,
        nodes = new vis.DataSet([]),
        edges = new vis.DataSet([]),
        container,
        arrows = { to: { scaleFactor: 1 } };

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
    orniscient.data = {
        nodes: nodes,
        edges: edges
    };

    orniscient.init = function () {
        container = document.getElementById('mynetwork');
        var network = new vis.Network(container, orniscient.data, options);
        network.on("selectNode", function (params) {
            if (params.nodes.length == 1) {
                if (network.isCluster(params.nodes[0]) == true) {
                    network.openCluster(params.nodes[0]);
                }
            }
        });

        $.connection.hub.start().then(init);

        //add client side methods for updates
        $.extend(hub.client, {
            grainActivationChanged: function (diffModel) {
                $.each(diffModel.NewGrains, function (index, grainData) {
                    addToNodes(grainData);
                });
            }
        });
    }

    function init() {
        return hub.server.getCurrentSnapshot()
            .done(function (data) {
                $.each(data, function (index, grainData) {
                    addToNodes(grainData);
                });
            })
            .fail(function (data) {
                alert('Oops, we cannot connect to the server...');
            });
    }

    function addToNodes(grainData) {

        //add the node
        orniscient.data.nodes.add({
            id: grainData.Id,
            label: grainData.GrainName,
            color: grainData.Colour,
            silo: 'C',
            linkToId:grainData.LinkToId
        });

        //add the edge (link)
        if (grainData.LinkToId !== '') {
            orniscient.data.edges.add({
                id:grainData.Id,
                from: grainData.Id,
                to: grainData.LinkToId
            });
        }
    }
}(window.orniscient = window.orniscient || {}, jQuery));
