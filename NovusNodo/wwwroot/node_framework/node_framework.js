import { Node } from "./Node.js";
import { Link } from "./Link.js";
import { Canvas } from "./Canvas.js";

let netCanvasReference = null;
let canvasTabs = {};
let selectedPaperTabId = null;

export function setSelectedCanvasTabId(id) {
    console.log("Setting Selected Canvas TabId to " + id);
    selectedPaperTabId = id;
}

export function createCanvas(id, reference) {
    netCanvasReference = reference;

    // Get the computed dimensions of the parent container
    const parentContainer = d3.select("#main_container");
    const width = parentContainer.node().clientWidth;
    const height = parentContainer.node().clientHeight;

    canvasTabs[id] = new Canvas(id, netCanvasReference, width, height);
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

export function addInputPorts(nodeId, portId)
{
    if (selectedPaperTabId == null) {
        console.log("No Canvas Tab Selected");
        return;
    }

    console.log("Adding Input Port to Node " + nodeId);
    const canvas = canvasTabs[selectedPaperTabId];
    const node = canvas.getNode(nodeId);
    node.addInputPort(portId);
}

export function addOutputPorts(nodeId, portId)
{
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

export function setDarkMode(isDarkMode) {
    Object.values(canvasTabs).forEach(element => {
        element.setDarkMode(isDarkMode);
    });
}


/* 
let nodeCounter = 0;

for (let i = 0; i < 5; i++) {
    const node = new Node(nodeCounter++, canvas, 140, 40, 100, 100 + i * 100, "#00cc00", 3);
    node.setLabelText("Node " + nodeCounter);
    canvas.addNode(node);
}
 */