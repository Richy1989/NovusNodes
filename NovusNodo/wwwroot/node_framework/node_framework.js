import { Node } from "./Node.js";
import { Link } from "./Link.js";
import { Canvas } from "./Canvas.js";

let canvas = new Canvas(1600, 1000);
let nodeCounter = 0;

for (let i = 0; i < 5; i++) {
    const node = new Node(nodeCounter++, canvas, 140, 40, 100, 100 + i * 100, "#00cc00", 3);
    node.setLabelText("Node " + nodeCounter);
    canvas.addNode(node);
}

