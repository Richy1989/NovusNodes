import { Node } from "./Node.js";
import { Link } from "./Link.js";
import { Canvas } from "./Canvas.js";

let canvasTabs = {};
let selectedPaperTabId = null;
let isDarkMode = true;

export function setSelectedCanvasTabId(id) {
    console.log("Setting Selected Canvas TabId to " + id);
    selectedPaperTabId = id;
}

export function createCanvas(id, reference) {

    // Get the computed dimensions of the parent container
    const parentContainer = d3.select("#main_container");
    const width = parentContainer.node().clientWidth;
    const height = parentContainer.node().clientHeight;

    canvasTabs[id] = new Canvas(id, reference, width, height);
    canvasTabs[id].setDarkMode(isDarkMode);
    canvasTabs[id].changeBackgroundColor();
}

export function removeCanvasTab(id)
{
    console.log("Removing Canvas Tab " + id, canvasTabs[id]);
    canvasTabs[id].delete();
    delete canvasTabs[id];
}

export function setDarkMode(isInDarkMode) {
    isDarkMode = isInDarkMode;
    Object.values(canvasTabs).forEach(element => {
        element.setDarkMode(isDarkMode);
        element.changeBackgroundColor();
        element.resetAllColors();
    });
}

export function setLineStyle(useCubicBezier) {
    Object.values(canvasTabs).forEach(element => {
        element.useCubicBezier = useCubicBezier;
        element.drawLinks();
    });
}

export function resetZoom() {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    canvasTabs[selectedPaperTabId].CanvasZoom.resetZoom();
}

export function resizeCanvas(id) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }
    canvasTabs[id].resizeCanvas();
}

export function createNode(id, color, name, width, height, x, y, nodeType) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    const canvas = canvasTabs[selectedPaperTabId];
    const node = new Node(id, canvas, color, name, width, height, x, y, nodeType);
    canvas.addNode(node);
}

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

export function addOutputPorts(nodeId, portId) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    console.log("Adding Output Port to Node " + nodeId);
    const canvas = canvasTabs[selectedPaperTabId];
    const node = canvas.getNode(nodeId);
    node.addOutputPorts(portId);
    canvas.attachPortListeners(node)
}

export function addLink(sourceNodeId, sourcePortId, targetNodeId, targetPortId)
{
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    const canvas = canvasTabs[selectedPaperTabId];
    const sourceNode = canvas.getNode(sourceNodeId);
    const targetNode = canvas.getNode(targetNodeId);
    const sourcePort = sourceNode.getPort(sourcePortId);
    const targetPort = targetNode.getPort(targetPortId);
    const link = new Link(canvas, sourcePort, targetPort);
    canvas.addLink(sourcePort, targetPort);
}

export function setNodeName(nodeId, name) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    const canvas = canvasTabs[selectedPaperTabId];
    const node = canvas.getNode(nodeId);
    node.setLabelText(name);
}

export function enableDisableNode(nodeId, isEnabled) {
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    const canvas = canvasTabs[selectedPaperTabId];
    const node = canvas.getNode(nodeId);
    node.setEnabled(isEnabled);
}


/* 
let nodeCounter = 0;

for (let i = 0; i < 5; i++) {
    const node = new Node(nodeCounter++, canvas, 140, 40, 100, 100 + i * 100, "#00cc00", 3);
    node.setLabelText("Node " + nodeCounter);
    canvas.addNode(node);
}
 */