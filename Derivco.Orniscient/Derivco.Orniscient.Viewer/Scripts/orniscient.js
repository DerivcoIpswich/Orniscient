
(function (orniscient, $, undefined) {

    var hub = $.connection.orniscientHub,
        nodes = new vis.DataSet([]),
        edges = new vis.DataSet([]),
        typeCounts = [],
        container,
        arrows = { to: { scaleFactor: 1 } },
        summaryView = false;

    var options = {
        autoResize: true,
        height: '100%',
        nodes: {
            borderWidth: 3,
            shape: 'dot',
            scaling: {
                customScalingFunction: function (min, max, total, value) {
                    return value / total;
                },
                min: 5,
                max: 150,
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
        console.log('orniscient.init was called');
        container = document.getElementById('mynetwork');
        var network = new vis.Network(container, orniscient.data, options);
        network.on("selectNode", function (params) {
            if (params.nodes.length == 1) {
                if (network.isCluster(params.nodes[0]) == true) {
                    network.openCluster(params.nodes[0]);
                }
            }
        }).
            on("hoverNode", function (params) {
                //get the node's information from the server.
                var node = nodes.get(params.node);
                if (node.ServerCalled !== true) {

                    var requestData = {
                        GrainType: node.graintype,
                        GrainId: node.grainId
                    };

                    var xhr = new XMLHttpRequest();
                    xhr.open('post', orniscienturls.getGrainInfo, true);
                    xhr.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
                    xhr.onload = function () {
                        var grainInfo = [];
                        var infoRows = "";

                        //add the info we can for the grain. As per Spencer....
                        infoRows = infoRows + "<tr><td><strong>Grain Id</strong></td><td>" + node.grainId + "</td></tr>";
                        infoRows = infoRows + "<tr><td><strong>Silo</strong></td><td>" + node.silo + "</td></tr>";
                        infoRows = infoRows + "<tr><td><strong>Grain Type</strong></td><td>" + node.graintype + "</td></tr>";

                        if (xhr.responseText != null && xhr.responseText !== "") {
                            grainInfo = JSON.parse(xhr.responseText);
                            for (var i = 0; i < grainInfo.length; i++) {
                                infoRows = infoRows + "<tr><td><strong>" + grainInfo[i].FilterName + "<strong></td><td>" + grainInfo[i].Value + "</td></tr>";
                            }
                        }
                        node.title = "<h5>" + node.label + "</h5><table class='table'>" + infoRows + "</table>";
                        node.ServerCalled = true;
                        orniscient.data.nodes.update(node);
                    }
                    xhr.send(JSON.stringify(requestData));
                }
            });


        $.extend(hub.client, {
            grainActivationChanged: function (diffModel) {
                console.log('changes sent from server');
                console.log(diffModel);

                window.dispatchEvent(new CustomEvent('orniscientUpdated', { detail: diffModel.TypeCounts }));
                $.each(diffModel.NewGrains, function (index, grainData) {
                    addToNodes(grainData, diffModel.SummaryView);
                });

            }
        });
        $.connection.hub.logging = true;
        $.connection.hub.error(function (error) {
            console.log('SignalR error: ' + error);
        });
        $.connection.hub.start().then(init);

    }


    function init() {
        return orniscient.getServerData();
    }

    orniscient.getServerData = function getServerData(filter) {
        console.log('getting server data');
        if (filter === null)
            filter = {};

        return hub.server.GetCurrentSnapshot(filter)
            .done(function (data) {
                $.each(data.NewGrains, function (index, grainData) {
                    addToNodes(grainData,data.SummaryView);
                });
            })
            .fail(function (data) {
                alert('Oops, we cannot connect to the server...');
            });
    }

    function addToNodes(grainData, isSummaryView) {
        if (isSummaryView === true) {


            if (summaryView === false && isSummaryView===true) {
                orniscient.data.nodes.clear();
                orniscient.data.edges.clear();
                summaryView = true;
            }

            //find and update 
            var updateNode = nodes.get(grainData.Id);
            if (updateNode != undefined) {

                updateNode.value = grainData.Count;
                orniscient.data.nodes.update(updateNode);
                return;
            }
        }

        //otherwise add new
        var label = (isSummaryView ? grainData.TypeShortName + '(' + grainData.Count + ')' : grainData.GrainName);
        console.log(label);
        var node = {
            id: grainData.Id,
            label: label,
            color: {
                border: grainData.Colour

            },
            //border: grainData.Colour,
            silo: grainData.Silo,
            linkToId: grainData.LinkToId,
            graintype: grainData.Type,
            grainId: grainData.GrainId,
            group: grainData.Silo
        }
        if (grainData.Count > 1) {
            node.value = grainData.Count;
        }

        //add the node
        orniscient.data.nodes.add(node);

        //add the edge (link)
        if (grainData.LinkToId !== null && grainData.LinkToId !== '') {
            orniscient.data.edges.add({
                id: grainData.TypeShortName + '_' + grainData.GrainId + 'temp',
                from: grainData.TypeShortName + '_' + grainData.GrainId,
                to: grainData.LinkToId
            });
        }
    }
}(window.orniscient = window.orniscient || {}, jQuery));
