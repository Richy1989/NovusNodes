/**
 * The graph object for the JointJS diagram.
 * @type {joint.dia.Graph}
 */
let graph = undefined;

/**
 * The paper object for the JointJS diagram.
 * @type {joint.dia.Paper}
 */
let paper = undefined;

// Keep track of the currently selected cell
let selectedCell = null;

// Keep track of the currently selected cell
let selectedLink = null;

/**
* Creates a JointJS paper and attaches it to the specified container.
* @param {string} paperContainerName - The ID of the container element.
*/

let BackgroundColor = 'white';
let StrokeColor = 'black';
let LinkColor = '#7f1ddc';

function reloadPage() {
    location.reload();
}

function JJSSetColorPalette(backgroundColor, strokeColor, linkColor) {
    BackgroundColor = backgroundColor;
    StrokeColor = strokeColor;
    LinkColor = linkColor;


    console.log('Setting color palette:', BackgroundColor, StrokeColor, LinkColor);
}

function JJSCreatePaper(paperContainerName) {
    const container = document.getElementById(paperContainerName);

    // Create the graph
    graph = new joint.dia.Graph();

    // Dynamically set paper size to fit the container
    paper = new joint.dia.Paper({
        el: container,
        model: graph,
        width: container.offsetWidth,
        height: container.offsetHeight,
        gridSize: 10,
        drawGrid: true,
        defaultLink: () => new joint.shapes.standard.Link({
            attrs: {
                line: {
                    stroke: LinkColor, // Set the default link color here
                    strokeWidth: 2,
                    targetMarker: {
                        'type': 'path',
                        'd': 'M 10 -5 0 0 10 5 Z',
                        'fill': LinkColor
                    }
                }
            }
        }),
        linkPinning: false,
        validateConnection: function (cellViewS, magnetS, cellViewT, magnetT, end, linkView) {
            // Prevent linking from input ports
            if (magnetS && magnetS.getAttribute('port-group') === 'in') return false;
            // Prevent linking from output ports to input ports within one element
            if (cellViewS === cellViewT) return false;
            // Prevent linking to output ports
            return magnetT && magnetT.getAttribute('port-group') === 'in';
        },
        validateMagnet: function (cellView, magnet) {
            // Note that this is the default behaviour. It is shown for reference purposes.
            // Disable linking interaction for magnets marked as passive
            return magnet.getAttribute('magnet') !== 'passive';
        }
    });

    // Resize the paper when the container size changes
    window.addEventListener('resize', () => {
        paper.setDimensions(container.offsetWidth, container.offsetHeight);
    });

    // Notify the .NET code when an element is moved
    graph.on('change:position', (element, newPosition) =>
    {
        DotNet.invokeMethodAsync('NovusNodo', 'ElementMoved', element.id, newPosition.x, newPosition.y);
    });

    // Notify the .NET code when a link is connected
    paper.on('link:connect', (linkView, evt, elementViewConnected) =>
    {
        DotNet.invokeMethodAsync(
            'NovusNodo',
            'LinkAdded',
            linkView.model.attributes.source.id,
            linkView.model.attributes.source.port,
            linkView.model.attributes.target.id,
            linkView.model.attributes.target.port);

        console.log('A link was connected to an element or port:', linkView.model);
    });

    paper.on('blank:pointerclick', function ()
    {
        ResetAll();
    });

    paper.on('link:pointerclick', function (linkView)
    {
        ResetAll();

        var currentLink = linkView.model;
        currentLink.attr('line/stroke', 'orange');
        currentLink.label(0, {
            attrs: {
                body: {
                    stroke: 'orange'
                }
            }
        });

        
        selectedLink = currentLink;
    });

    // Highlight the selected cell
    paper.on('element:pointerclick', (cellView) =>
    {
        ResetAll();
        
        selectedCell = cellView;
        cellView.highlight(); // Highlight the new selection
    });

    // Listen for cell removal events
    graph.on('cell:remove', (cell) => {
        console.log('Cell removed:', cell);
    });
}

// Handle the Delete key press
document.addEventListener('keydown', (event) => {
    if (event.key === 'Delete') {

        if (selectedLink) {
            const cell = selectedLink;
            cell.remove();
            LinkDeleted(selectedLink);
            selectedLink = undefined;
        }

        if (selectedCell) {
            const cell = selectedCell.model;
            cell.remove(); // Remove the selected cell from the graph
            ElementDeleted(cell.id); // Notify the .NET code
            selectedCell = undefined; // Clear the selection

        }
    }
});

function ResetAll() {

    if (selectedCell) {
        selectedCell.unhighlight(); // Unhighlight the previous selection
        selectedCell = undefined; 
    }

    selectedLink = undefined;

    paper.drawBackground({
        color: BackgroundColor
    });

    var elements = paper.model.getElements();
    for (var i = 0, ii = elements.length; i < ii; i++)
    {
        var currentElement = elements[i];
        currentElement.attr('body/stroke', StrokeColor);
    }

    var links = paper.model.getLinks();
    for (var j = 0, jj = links.length; j < jj; j++)
    {
        var currentLink = links[j];
        currentLink.attr('line/stroke', LinkColor);
        currentLink.label(0, {
            attrs: {
                body: {
                    stroke: LinkColor
                }
            }
        });
    }
}

/**
 * Logs the deletion of an element.
 * @param {string} elementId - The ID of the deleted element.
 */
function ElementDeleted(elementId) {
    console.log('Element deleted:', elementId);

    DotNet.invokeMethodAsync('NovusNodo', 'ElementRemoved', elementId);
}

/**
* Creates a custom node element and adds it to the graph.
* @param {string} id - The ID of the node.
* @param {string} color - The color of the node.
* @param {string} text - The text to display on the node.
* @param {number} x - The x-coordinate of the node's position.
* @param {number} y - The y-coordinate of the node's position.
*/
function JJSCreateNodeElement(id, color, text, x, y) {

    console.log('Creating node element:', id, color, text, x, y);

    joint.shapes.custom = {};
    joint.shapes.custom.NovusNode = joint.dia.Element.define('custom.NovusNode', {
        size: { width: 120, height: 40 },
        attrs: {
            body: {
                refWidth: '100%',
                refHeight: '100%',
                rx: 5,
                ry: 5,
                fill: color, // Node-RED style color
                stroke: StrokeColor,
                strokeWidth: 1,
            },
            header: {
                refWidth: '100%',
                height: 20,
                fill: '#0000001a', // Dark header color
                strokeWidth: 1,
            },
            headerLabel: {
                refX: '50%',
                refY: '25%',
                textAnchor: 'middle',
                textVerticalAnchor: 'middle',
                fontSize: 14,
                fill: '#000000',
                text: 'Node Title',
                fontWeight: 'bold',
            },
        },

    }, {
        markup: [
            {
                tagName: 'rect',
                selector: 'body',
            },
            {
                tagName: 'rect',
                selector: 'header',
            },
            {
                tagName: 'text',
                selector: 'headerLabel',
            },
            {
                tagName: 'text',
                selector: 'label',
            },
        ],
    });

    var portsIn = {
        position: {
            name: 'left'
        },
        attrs: {
            portBody: {
                magnet: true,
                r: 7,
                fill: '#023047',
                stroke: '#023047'
            }
        },
        label: {
            position: {
                name: 'left',
                args: { y: 6 }
            },
            markup: [{
                tagName: 'text',
                selector: 'label',
                className: 'label-text'
            }]
        },
        markup: [{
            tagName: 'circle',
            selector: 'portBody'
        }]
    };

    var portsOut = {
        position: {
            name: 'right'
        },
        attrs: {
            portBody: {
                magnet: true,
                r: 7,
                fill: '#E6A502',
                stroke: '#023047'
            }
        },
        label: {
            position: {
                name: 'right',
                args: { y: 6 }
            },
            markup: [{
                tagName: 'text',
                selector: 'label',
                className: 'label-text'
            }]
        },
        markup: [{
            tagName: 'circle',
            selector: 'portBody'
        }]
    };

    const node = new joint.shapes.custom.NovusNode({
        id: id,
        position: { x: parseFloat(x) || 100, y: parseFloat(y) || 100 },
        attrs: {
            headerLabel: {
                text: text,
            },
        },
        ports: {
            groups: {
                'in': portsIn,
                'out': portsOut
            }
        }
    });

    graph.addCell(node);
}

/**
 * Adds an input port to the specified node.
 * @param {string} nodeId - The ID of the node to which the input port will be added.
 * @param {string} portId - The ID of the input port to be added.
 */
function JJSAddInputPort(nodeId, portId) {
    var node = graph.getCell(nodeId);

    node.addPorts([
        {
            id: portId,
            group: 'in',
            attrs: { label: { text: '' } }
        }
    ]);
}

/**
 * Adds an output port to the specified node.
 * @param {string} nodeId - The ID of the node to which the output port will be added.
 * @param {string} portId - The ID of the output port to be added.
 */
function JJSAddOutputPort(nodeId, portId) {

    console.log('Adding output port:', nodeId, portId);

    var node = graph.getCell(nodeId);

    node.addPorts([
        {
            id: portId,
            group: 'out',
            attrs: { label: { text: '' } }
        }
    ]);
}

/**
 * Creates a link between two ports on the graph.
 * @param {string} sourceID - The ID of the source node.
 * @param {string} sourcePortID - The ID of the source port.
 * @param {string} targetID - The ID of the target node.
 * @param {string} targetPortID - The ID of the target port.
 */
function JJSCreateLink(sourceID, sourcePortID, targetID, targetPortID) {

    console.log('Creating link:', sourceID, sourcePortID, targetID, targetPortID);

    source = graph.getCell(sourceID);
    target = graph.getCell(targetID);

    const link = new joint.shapes.standard.Link({
        source: { id: sourceID, port: sourcePortID },
        target: { id: targetID, port: targetPortID },
    });

    link.attr('line/stroke', LinkColor);
    link.label(0, {
        attrs: {
            body: {
                stroke: LinkColor
            }
        }
    });

    link.addTo(graph);
}

/**
* Logs the deletion of a link and notifies the .NET code.
* @param {Object} link - The deleted link object.
*/
function LinkDeleted(link) {
    console.log('Link deleted:', link);
    console.log('Link deleted source:', link.attributes.source);
    console.log('Link deleted target:', link.attributes.target);

    DotNet.invokeMethodAsync(
        'NovusNodo',
        'LinkRemoved',
        link.attributes.source.id,
        link.attributes.source.port,
        link.attributes.target.id,
        link.attributes.target.port);
}