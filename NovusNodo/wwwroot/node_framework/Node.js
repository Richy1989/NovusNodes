import { Port } from "./Port.js";

/**
 * Represents a draggable node in an SVG.
 */
export class Node {
    /**
     * Creates an instance of Node.
     * @param {string} id - The unique identifier for the node.
     * @param {number} x - The initial x-coordinate of the node.
     * @param {number} y - The initial y-coordinate of the node.
     */
    constructor(id, canvas, width, height, x, y, color, nodeType) {
        this.svg = d3.select("svg");
        this.canvas = canvas;
        this.id = id;
        this.width = width;
        this.height = height;
        this.color = color;
        this.x = x;
        this.y = y;
        this.offsetX = 0;
        this.offsetY = 0;
        this.nodeType = nodeType;
        this.isDragging = false;
        this.group = this.createNodeGroup();
        this.createPorts();
    }

    /**
     * Creates the SVG group element for the node and sets up the drag behavior.
     * @returns {d3.Selection} The created SVG group element.
     */
    createNodeGroup() {
        const group = this.svg.append("g")
            .attr("class", "node-group")
            .attr("transform", `translate(${this.x},${this.y})`)
            
            .call(d3.drag()
                .on("start", this.dragStarted.bind(this))
                .on("drag", this.dragged.bind(this))
                .on("end", this.dragEnded.bind(this)));

        group.append("rect")
            .attr("class", "node")
            .attr("stroke", this.canvas.getNodeStrokeColor())
            .attr("name", "nodebody")
            .attr("fill", this.color)
            .attr("width", this.width)
            .attr("height", this.height);

            this.label = group.append("text")
            .attr("class", "label")
            .attr("x", this.width / 2)
            .attr("y", this.height / 4)
            .text("Label");

       /* group.append("circle")
            .attr("class", "button")
            .attr("cx", -10)
            .attr("cy", 25)
            .attr("r", 10);*/

        return group;
    }

    /**
     * Sets the text of the label and resizes the node if necessary.
     * @param {string} text - The text to set.
     */
    setLabelText(text) {
        this.label.text(text);
        const textWidth = this.label.node().getBBox().width;
        if (textWidth > this.width) {
            this.width = textWidth + 20; // Add some padding
            this.updateNodeDimensions();
        }
    }

    /**
     * Updates the dimensions of the node.
     */
    updateNodeDimensions() {
        this.group.select("rect.node").attr("width", this.width);
        this.label.attr("x", this.width / 2);
    }

    /**
     * Handles the start of the drag event.
     * @param {d3.D3DragEvent} event - The drag event.
     */
    dragStarted(event) {
        if (d3.select(event.sourceEvent.target).attr("name") === "nodebody") {
            this.isDragging = true;
            const transform = d3.select(event.sourceEvent.target.parentNode).attr("transform");
            const translate = transform.match(/translate\(([^,]+),([^\)]+)\)/);
            this.offsetX = event.x - parseFloat(translate[1]);
            this.offsetY = event.y - parseFloat(translate[2]);
        }
    }

    /**
     * Handles the drag event.
     * @param {d3.D3DragEvent} event - The drag event.
     */
    dragged(event) {
        if (this.isDragging) {
            this.x = event.x - this.offsetX;
            this.y = event.y - this.offsetY;
            this.group.attr("transform", `translate(${this.x},${this.y})`);
            this.inputPort.dragLinks();
            this.outputPort.dragLinks();
        }
    }

    /**
     * Handles the end of the drag event.
     * @param {d3.D3DragEvent} event - The drag event.
     */
    dragEnded(event) {
        this.isDragging = false;
    }

    createPorts() {
        if (this.nodeType == 1 || this.nodeType == 3) {
            this.inputPort = new Port(this, this.canvas, 'input', crypto.randomUUID());//addPort(-5, this.height / 2 - 5, 'input');
        }
    
        if (this.nodeType == 2 || this.nodeType == 3) {
            this.outputPort = new Port(this, this.canvas, 'output', crypto.randomUUID());//= addPort(this.width - 5, this.height / 2 - 5, 'output');
        }
    }
}