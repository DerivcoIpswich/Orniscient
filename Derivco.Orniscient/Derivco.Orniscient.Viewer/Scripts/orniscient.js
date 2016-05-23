
(function (orniscient, $, undefined) {

    var hub = $.connection.orniscientHub,
        nodes = new vis.DataSet([]),
        edges = new vis.DataSet([]),
        container = document.getElementById('mynetwork'),
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
    var data = {
        nodes: nodes,
        edges: edges
    };

    orniscient.init = function () {

        var network = new vis.Network(container, data, options);
        network.on("selectNode", function (params) {
            if (params.nodes.length == 1) {
                if (network.isCluster(params.nodes[0]) == true) {
                    network.openCluster(params.nodes[0]);
                }
            }
        });

        console.log('doing the init now');
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
        console.log('doing init function now');
        return hub.server.getCurrentSnapshot()
            .done(function (data) {
                console.log('return from server with grain count of : ' + data.length);
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
        nodes.add({
            id: grainData.Id,
            label: grainData.GrainName,
            color: grainData.Colour,
            silo: 'C'
        });

        //add the edge (link)
        if (grainData.LinkToId !== '') {
            edges.add({
                from: grainData.Id,
                to: grainData.LinkToId
            });
        }
    }



}(window.orniscient = window.orniscient || {}, jQuery));



//$(function () {

//    var hub = $.connection.orniscientHub,
//        nodes = new vis.DataSet([]),
//        edges = new vis.DataSet([]),
//        container = document.getElementById('mynetwork'),
//        arrows = { to: { scaleFactor: 1 } };

//    var options = {
//        autoResize: true,
//        height: '100%',
//        nodes: {
//            shape: 'dot',
//            scaling: {
//                min: 10,
//                max: 30,
//                label: {
//                    min: 8,
//                    max: 30,
//                    drawThreshold: 12,
//                    maxVisible: 20
//                }
//            }
//        },
//        interaction: {
//            hover: true,
//            navigationButtons: true
//        },
//        layout: {
//            randomSeed: 2
//        }
//    };
//    var data = {
//        nodes: nodes,
//        edges: edges
//    };

//    var network = new vis.Network(container, data, options);
//    network.on("selectNode", function (params) {
//        if (params.nodes.length == 1) {
//            if (network.isCluster(params.nodes[0]) == true) {
//                network.openCluster(params.nodes[0]);
//            }
//        }
//    });
//    $.connection.hub.start()
//		.then(init);

//    function init() {
//        return hub.server.getCurrentSnapshot()
//            .done(function (data) {

//                $.each(data, function (index, grainData) {
//                    addToNodes(grainData);
//                });
//            })
//            .fail(function (data) {
//                alert('Oops, we cannot connect to the server...');
//            });
//    }

//    function addToNodes(grainData) {

//        //add the node
//        nodes.add({
//            id: grainData.Id,
//            label: grainData.GrainName,
//            color: grainData.Colour,
//            silo: 'C'
//        });

//        //add the edge (link)
//        if (grainData.LinkToId !== '') {
//            edges.add({
//                from: grainData.Id,
//                to: grainData.LinkToId
//            });
//        }
//    }

//    //add client side methods for updates
//    $.extend(hub.client, {
//        grainActivationChanged: function (diffModel) {
//            $.each(diffModel.NewGrains, function (index, grainData) {
//                addToNodes(grainData);
//            });
//        }
//    });

//});