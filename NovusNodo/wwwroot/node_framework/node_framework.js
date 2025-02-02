import { Node } from "./Node.js";
import { Link } from "./Link.js";
import { Canvas } from "./Canvas.js";

// Object to store canvas instances by their IDs
let canvasTabs = {};

// ID of the currently selected canvas tab
let selectedPaperTabId = null;

// Boolean indicating whether dark mode is enabled
let isDarkMode = true;

let mouseX = 0;
let mouseY = 0;

// Update mouse position on movement
document.addEventListener("mousemove", (event) => {
    mouseX = event.clientX;
    mouseY = event.clientY;
});

// Add event listeners for keydown events for copy and paste
document.addEventListener('keydown', (event) => {
    if (event.ctrlKey && event.key === 'c') {

        const canvas = canvasTabs[selectedPaperTabId];
        let idList = canvas.getSelectedNodeIds();

        //Return if nothing is selected
        if(idList.length <= 0) {
            console.log("No Nodes Selected");
            return;
        }

        if (selectedPaperTabId == null) {
            console.log("No Canvas Tab Selected");
            return;
        }
        
        console.log("Starting Copy: ", canvas);
        console.log("Selected Node Ids: ", idList);

        canvas.netCanvasReference.invokeMethodAsync("NovusNode.ClipboardCopyNodes", idList)
        .then(returnValue => {
            return navigator.clipboard.writeText(returnValue);
        })
        .then(() => {
            console.log("Copied to clipboard");
        })
        .catch(err => {
            console.error("Failed to copy:", err);
        });
    }
    
    if (event.ctrlKey && event.key === 'v') {
        const canvas = canvasTabs[selectedPaperTabId];
        navigator.clipboard.readText()
        .then(clipboard => {
            canvas.netCanvasReference.invokeMethodAsync("NovusNode.ClipboardPasteNodes", parseFloat(mouseX), parseFloat(mouseY), clipboard)
            .then(addedNodes => 
                {
                    canvas.dragMultipleSelection = canvas.nodeList.filter(node => addedNodes.includes(node.id));
                    // Mark all selected nodes
                    canvas.resetAllColors();
                    canvas.dragMultipleSelection.forEach((d) => d.markAsSelected());
                })
        });
    }
});


/**
 * Sets the selected canvas tab ID.
 * @param {string} id - The ID of the canvas tab to select.
 */
export function setSelectedCanvasTabId(id) {
    console.log("Setting Selected Canvas TabId to " + id);
    selectedPaperTabId = id;
}

/**
 * Creates a new canvas with the given ID and reference.
 * @param {string} id - The ID of the canvas.
 * @param {string} reference - The reference for the canvas.
 */
export function createCanvas(id, reference) {
    // Get the computed dimensions of the parent container
    const parentContainer = d3.select("#main_container");
    const width = parentContainer.node().clientWidth;
    const height = parentContainer.node().clientHeight;

    // Create a new Canvas instance and store it in canvasTabs
    canvasTabs[id] = new Canvas(id, reference, width, height);
    canvasTabs[id].setDarkMode(isDarkMode);
    canvasTabs[id].changeBackgroundColor();
}

/**
 * Removes the canvas tab with the given ID.
 * @param {string} id - The ID of the canvas tab to remove.
 */
export function removeCanvasTab(id) {
    console.log("Removing Canvas Tab " + id, canvasTabs[id]);
    
    if(canvasTabs[id] == null)
        return;
        
    canvasTabs[id].delete();
    delete canvasTabs[id];
}

/**
 * Sets the dark mode for all canvases.
 * @param {boolean} isInDarkMode - Whether dark mode is enabled.
 */
export function setDarkMode(isInDarkMode) {
    isDarkMode = isInDarkMode;
    Object.values(canvasTabs).forEach(element => {
        element.setDarkMode(isDarkMode);
        element.changeBackgroundColor();
        element.resetAllColors();
    });
}

/**
 * Sets the raster size for the canvas.
 *
 * This function updates the raster size for all canvas tabs. If no canvas tab is selected,
 * it logs a message and returns without making any changes.
 *
 * @param {number} rasterSize - The new raster size to be set for the canvas.
 */
export function setCanvasRasterSize(rasterSize) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    for (const [key, value] of Object.entries(canvasTabs)) {
        value.setRasterSize(rasterSize);
    }
}

/**
 * Sets the line style for all canvases.
 * @param {boolean} useCubicBezier - Whether to use cubic bezier for lines.
 */
export function setLineStyle(useCubicBezier) {
    Object.values(canvasTabs).forEach(element => {
        element.useCubicBezier = useCubicBezier;
        element.drawLinks();
    });
}

/**
 * Resets the zoom level of the selected canvas.
 */
export function resetZoom() {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    canvasTabs[selectedPaperTabId].CanvasZoom.resetZoom();
}

/**
 * Resizes the canvas with the given ID.
 * @param {string} id - The ID of the canvas to resize.
 */
export function resizeCanvas(id) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }
    canvasTabs[id].resizeCanvas();
}

/**
 * Creates a new node on the selected canvas.
 * @param {string} id - The ID of the node.
 * @param {string} color - The color of the node.
 * @param {string} name - The name of the node.
 * @param {number} x - The x-coordinate of the node.
 * @param {number} y - The y-coordinate of the node.
 * @param {string} nodeType - The type of the node.
 */
export function createNode(id, color, name, x, y, pluginSettings) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    console.log("Plugin Settings: ", pluginSettings);

    const canvas = canvasTabs[selectedPaperTabId];
    const node = new Node(id, canvas, color, name, x, y, pluginSettings);
    canvas.addNode(node);
}

/**
 * Adds an input port to the specified node.
 * @param {string} nodeId - The ID of the node.
 * @param {string} portId - The ID of the port to add.
 */
export function addInputPorts(nodeId, portId) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    console.log("Adding Input Port to Node " + nodeId);
    const canvas = canvasTabs[selectedPaperTabId];
    const node = canvas.getNode(nodeId);
    node.addInputPort(portId);
}

/**
 * Adds an output port to the specified node.
 * @param {string} nodeId - The ID of the node.
 * @param {string} portId - The ID of the port to add.
 */
export function addOutputPorts(nodeId, portId) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    console.log("Adding Output Port to Node " + nodeId);
    const canvas = canvasTabs[selectedPaperTabId];
    const node = canvas.getNode(nodeId);
    node.addOutputPorts(portId);
    canvas.attachPortListeners(node);
}

/**
 * Adds a link between two nodes.
 * @param {string} sourceNodeId - The ID of the source node.
 * @param {string} sourcePortId - The ID of the source port.
 * @param {string} targetNodeId - The ID of the target node.
 * @param {string} targetPortId - The ID of the target port.
 */
export function addLink(sourceNodeId, sourcePortId, targetNodeId, targetPortId) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    console.log("Adding Link from " + sourceNodeId + " to " + targetNodeId);
    console.log("Source Port: " + sourcePortId + " Target Port: " + targetPortId);

    const canvas = canvasTabs[selectedPaperTabId];
    const sourceNode = canvas.getNode(sourceNodeId);
    const targetNode = canvas.getNode(targetNodeId);

    console.log("Node: ", sourceNode);
    console.log("Target: ", targetNode);

    const sourcePort = sourceNode.getPort(sourcePortId);
    const targetPort = targetNode.getPort(targetPortId);
    const link = new Link(canvas, sourcePort, targetPort);
    canvas.addLink(sourcePort, targetPort);
}

/**
 * Sets the name of the specified node.
 * @param {string} nodeId - The ID of the node.
 * @param {string} name - The new name of the node.
 */
export function setNodeName(nodeId, name) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    const canvas = canvasTabs[selectedPaperTabId];
    const node = canvas.getNode(nodeId);
    node.setLabelText(name);
}

/**
 * Enables or disables the specified node.
 * @param {string} nodeId - The ID of the node.
 * @param {boolean} isEnabled - Whether the node is enabled.
 */
export function enableDisableNode(nodeId, isEnabled) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    const canvas = canvasTabs[selectedPaperTabId];
    const node = canvas.getNode(nodeId);
    node.setEnabled(isEnabled);
}