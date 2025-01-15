// Define a class
class JointJSPage {
    // Constructor method to initialize properties
    constructor(graph, paper, paperId, netReference) {
        this.graph = graph;
        this.paper = paper;
        this.paperId = paperId;
        this.netReference = netReference;
    }
}

let jointJSPages = {};

//Joint JS Paper Component Reference Dictionary
//let jointJSComponentRefdictionary = {};
let selectedPaperTabId = null;

function JJSSetSelectedPaperTabId(id) {
    console.log("Setting SelectedPaperTabId to " + id);
    selectedPaperTabId = id;
}

/**
 * The currently selected cell in the JointJS diagram.
 * @type {joint.dia.CellView|null}
 */
let selectedCell = null;

/**
 * The currently selected link in the JointJS diagram.
 * @type {joint.dia.Link|null}
 */
let selectedLink = null;

//Checks if the curser is currently hovering over a port
let inPortHighlight = false;

let BackgroundColor = 'white';
let StrokeColor = 'black';
let LinkColor = '#7f1ddc';
let inputPortColor = '#4A90E2';
let outputPortColor = '#E6A502';
/**
 * Sets the color palette for the JointJS diagram.
 * @param {string} backgroundColor - The background color of the diagram.
 * @param {string} strokeColor - The stroke color for the elements.
 * @param {string} linkColor - The color of the links.
 */
function JJSSetColorPalette(backgroundColor, strokeColor, linkColor) {
    BackgroundColor = backgroundColor;
    StrokeColor = strokeColor;
    LinkColor = linkColor;

    console.log('Setting color palette:', BackgroundColor, StrokeColor, LinkColor);
}

/**
 * Finds the elements that are furthest along the x and y axes.
 * @returns {Object} An object containing the furthest elements along the x and y axes.
 */
function findFurthestElements() {
    var elements = jointJSPages[selectedPaperTabId].graph.getElements();
    var furthestXElement = null;
    var furthestYElement = null;
    var maxX = -Infinity;
    var maxY = -Infinity;

    elements.forEach(function (element) {
        var position = element.position();
        if (position.x > maxX) {
            maxX = position.x;
            furthestXElement = element;
        }
        if (position.y > maxY) {
            maxY = position.y;
            furthestYElement = element;
        }
    });

    return {
        furthestXElement: furthestXElement,
        furthestYElement: furthestYElement
    };
}

/**
 * Resizes the JointJS paper to fit the container and the furthest elements.
 */
function ResizePaper() {

    console.log('Resizing paper to fit the container and the furthest elements.');

    var container = document.getElementById("main_container");

    var furthest = findFurthestElements();

    let width = container.offsetWidth;
    let height = container.offsetHeight;

    if (furthest.furthestXElement != null && furthest.furthestYElement != null) {
        width = Math.max(width, furthest.furthestXElement.position().x + furthest.furthestXElement.size().width + 10);
        height = Math.max(height, furthest.furthestYElement.position().y + furthest.furthestYElement.size().height + 10);
    }
    console.log('Resizing paper to:', width, height);
    jointJSPages[selectedPaperTabId].paper.setDimensions(width, height);
}

/**
 * Creates a JointJS paper and attaches it to the specified container.
 * @param {string} paperContainerName - The ID of the container element.
 */
function JJSCreatePaper(paperContainerName, reference) {
    const container = document.getElementById(paperContainerName);

    // Create the graph
    const graph = new joint.dia.Graph();

    // Dynamically set paper size to fit the container
    const paper = new joint.dia.Paper({
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
        ResizePaper();
    });

    //// Notify the .NET code when an element is moved
    //graph.on('change:position', (element, newPosition) =>
    //{
    //    //jointJSPages[selectedPaperTabId].netReference.invokeMethodAsync("NovusHome.ElementMoved", element.id, newPosition.x, newPosition.y);
    //});

    paper.on('element:pointerdown', function (elementView, evt, x, y) {
        console.log('Element mousedown at', x, y);
        // Additional logic when the element is clicked
    });

    paper.on('element:pointerup', function (elementView, evt, x, y) {
        console.log('Element mousedown at', x, y);

        var element = elementView.model;
        var position = element.get('position');
        console.log('Element position on mousedown:', position);

        jointJSPages[selectedPaperTabId].netReference.invokeMethodAsync("NovusHome.ElementMoved", element.id, position.x, position.y);

        // Additional logic when the element is clicked
    });

    // Notify the .NET code when a link is connected
    paper.on('link:connect', (linkView, evt, elementViewConnected) =>
    {
        jointJSPages[selectedPaperTabId].netReference.invokeMethodAsync(
            'NovusHome.LinkAdded',
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

    paper.on('element:pointerclick', function (elementView, evt, x, y) {
        
        ResetAll();

        const elementPosition = elementView.model.position();

        // Calculate the relative position of the click
        const relativeX = x - elementPosition.x;
        const relativeY = y - elementPosition.y;

        // Check if the click was in the inject button
        if (relativeX < 0) {
            jointJSPages[selectedPaperTabId].netReference.invokeMethodAsync('NovusHome.InjectorElementClicked', elementView.model.id);
        }
        else {

            selectedCell = elementView;
            elementView.highlight(); // Highlight the new selection
        }
    });

    // Highlight the selected cell
    paper.on('element:pointerdblclick', (cellView) => {
        console.log('Element double clicked:', cellView.model.id);
        NovusUIManagementRef.invokeMethodAsync("NovusUIManagement.CellDoubleClick", selectedPaperTabId, cellView.model.id);
    });

    // Add event listener for mouseover and mouseout
    paper.on('cell:mouseover', function (cellView, evt) {
        const portId = evt.target.getAttribute('port');
        if (portId) {
            highlightPortColorOnHover(cellView.model, portId); // Hover color
        }
    });

    paper.on('cell:mouseout', function (cellView, evt) {
        const portId = evt.target.getAttribute('port');
        if (portId) {
            restorePortColorOnHover(cellView.model, portId); // Hover reset color
        }
    });

    const jointjspaper = new JointJSPage(graph, paper, paperContainerName, reference);
    jointJSPages[paperContainerName] = jointjspaper;
}


// Function to change port color on hover
function highlightPortColorOnHover(element, portId)
{
    if (!inPortHighlight) {
        inPortHighlight = true;
        const originalColor = element.getPort(portId).attrs.portBody.fill;

        let color = lightenColor(originalColor, 40);
        console.log("Highlighed", color);
        element.portProp(portId, 'attrs/portBody/originalFill', originalColor);
        element.portProp(portId, 'attrs/portBody/fill', color);
    }
}

// Function to restore port color on hover
function restorePortColorOnHover(element, portId) {
    inPortHighlight = false;
    const originalColor = element.portProp(portId, 'attrs/portBody/originalFill');
    console.log('Original color:', originalColor);
    if (originalColor) {
        element.portProp(portId, 'attrs/portBody/fill', originalColor);
    }
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

function ResetAll()
{
    const paper = jointJSPages[selectedPaperTabId].paper;

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
    jointJSPages[selectedPaperTabId].netReference.invokeMethodAsync('NovusHome.ElementRemoved', elementId);
}


function JJSCreateInjectorNode(id, color, text, width, height)
{
    // Define the custom element
    let injectorDef = joint.dia.Element.define('custom.InjectorNode', {
        markup: '<g class="rotatable"><g class="scalable"><rect/></g><text/><foreignObject class="inject-button-container"><body xmlns="http://www.w3.org/1999/xhtml"><button class="inject-button">Inject</button></body></foreignObject></g>',
        defaults: joint.util.deepSupplement({
            type: 'custom.InjectNode',
            size: { width: 100, height: 40 },
            attrs: {
                'rect': { width: 100, height: 40, rx: 5, ry: 5, fill: '#f3f3f3', stroke: '#000' },
                'text': { text: 'Inject Node', 'ref-x': 0.5, 'ref-y': 0.5, 'y-alignment': 'middle', 'x-alignment': 'middle', 'font-size': 14, 'font-family': 'Arial, helvetica, sans-serif' },
                '.inject-button-container': { 'ref-x': -30, 'ref-y': 0.5, 'y-alignment': 'middle', width: 80, height: 40 }
            }
        })
    });
    return injectorDef;
    
}


/**
* Creates a custom node element and adds it to the graph.
* @param {string} id - The ID of the node.
* @param {string} color - The color of the node.
* @param {string} text - The text to display on the node.
* @param {number} x - The x-coordinate of the node's position.
* @param {number} y - The y-coordinate of the node's position.
*/
function JJSCreateNodeElement(id, color, text, width, height, x, y, type) {

    console.log('Creating node element:', id, color, text, width, height, x, y);

    let buttonWidth = 0;
    if (type == 1)
        buttonWidth = 40;


    joint.shapes.custom = {};
    joint.shapes.custom.NovusNode = joint.dia.Element.define('custom.NovusNode', {
        size: { width: parseFloat(width), height: parseFloat(height) },
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
            button: {
                height: parseFloat(height),
                width: buttonWidth,
                refX: buttonWidth * -1,
                rx: 5,
                ry: 5,
                refY: 0,
                fill: '#38761D',
                stroke: StrokeColor,
                strokeWidth: 1,
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
            {
                tagName: 'rect',
                selector: 'button',
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
                fill: inputPortColor, //'#4A90E2',//'#023047',
                stroke: '#4A90E2'
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
                fill: outputPortColor,//'#E6A502',
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

    let node = null;

    node = new joint.shapes.custom.NovusNode({
        id: id,
        position: { x: parseFloat(x), y: parseFloat(y) },
        attrs: {
            headerLabel: {
                text: text,
            }
        },
        ports: {
            groups: {
                'in': portsIn,
                'out': portsOut
            }
        }
    });
    
    jointJSPages[selectedPaperTabId].graph.addCell(node);
}

function JJSDisableNode(nodeId) {
    var node = jointJSPages[selectedPaperTabId].graph.getCell(nodeId);
    // Read the current 'root/class' attribute
    let currentClasses = node.attr('root/class') || '';

    // Check if 'disabled-node' is not already in the class list
    if (!currentClasses.includes('disabled-node')) {
        // Append the 'disabled-node' class
        const updatedClasses = currentClasses + ' disabled-node';
        node.attr('root/class', updatedClasses.trim());
    }
}

function JJSEnableNode(nodeId) {
    var node = jointJSPages[selectedPaperTabId].graph.getCell(nodeId);
    // Read the current 'root/class' attribute
    let currentClasses = node.attr('root/class') || '';

    // Remove the 'disabled-node' class if it exists
    const updatedClasses = currentClasses.replace('disabled-node', '').trim();
    node.attr('root/class', updatedClasses);
}

/**
 * Changes the label of a node element.
 * @param {string} nodeId - The ID of the node.
 * @param {string} text - The new text for the node label.
 */
function JJSChangeNodeLabel(nodeId, text) {
    console.log('Changing node label:', nodeId, text);
    var node = jointJSPages[selectedPaperTabId].graph.getCell(nodeId);
    node.attr('headerLabel/text', text);
    autosize(node);
}

/**
 * Automatically resizes the element to fit its content.
 * @param {joint.dia.Element} element - The element to resize.
 */
function autosize(element) {
    var view = jointJSPages[selectedPaperTabId].paper.findViewByModel(element);
    var text = view.selectors.headerLabel;

    if (text) {
        var padding = 50;
        // Use bounding box without transformations so that our auto-sizing works
        // even on e.g. rotated element.
        var bbox = text.getBBox();
        // Give the element some padding on the right/bottom.

        var width = bbox.width + padding;
        var height = element.size().height;
        element.resize(width, height);

        jointJSPages[selectedPaperTabId].netReference.invokeMethodAsync("NovusHome.ElementResized", element.id, width, height);
    }
}


/**
 * Adds an input port to the specified node.
 * @param {string} nodeId - The ID of the node to which the input port will be added.
 * @param {string} portId - The ID of the input port to be added.
 */
function JJSAddInputPort(nodeId, portId) {
    console.log('Adding input port:', nodeId, portId);
    var node = jointJSPages[selectedPaperTabId].graph.getCell(nodeId);

    node.addPorts([
        {
            id: portId,
            group: 'in',
            attrs: {
                label: { text: '' },
                portBody: {
                    fill: inputPortColor,
                },
            }
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

    var node = jointJSPages[selectedPaperTabId].graph.getCell(nodeId);

    node.addPorts([
        {
            id: portId,
            group: 'out',
            attrs: {
                label: { text: '' },
                portBody: {
                    fill: outputPortColor,
                },
            }
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

    source = jointJSPages[selectedPaperTabId].graph.getCell(sourceID);
    target = jointJSPages[selectedPaperTabId].graph.getCell(targetID);

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

    link.addTo(jointJSPages[selectedPaperTabId].graph);
}

/**
* Logs the deletion of a link and notifies the .NET code.
* @param {Object} link - The deleted link object.
*/
function LinkDeleted(link) {
    console.log('Link deleted:', link);
    console.log('Link deleted source:', link.attributes.source);
    console.log('Link deleted target:', link.attributes.target);

    jointJSPages[selectedPaperTabId].netReference.invokeMethodAsync(
        'NovusHome.LinkRemoved',
        link.attributes.source.id,
        link.attributes.source.port,
        link.attributes.target.id,
        link.attributes.target.port);
}

// Function to close and remove the paper
function JJSCloseAndRemovePaper(pageid) {
    const paper = jointJSPages[pageid].paper;
    const graph = jointJSPages[pageid].graph;

    // Unbind all events on the paper and graph
    paper.off();  // Unbinds all events from the paper
    graph.off();  // Unbinds all events from the graph

    // If necessary, clear the graph to remove all elements
    graph.clear();

    // Remove a key-value pair
    delete jointJSPages.pageid;

    console.log('Paper and graph have been cleaned up and removed.');
}