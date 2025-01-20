import { Port } from "./Port.js";

/**
 * Represents a draggable node in an SVG.
 */
export class Node{    
    /**
     * Creates an instance of Node.
     * @param {string} id - The unique identifier for the node.
     * @param {number} x - The initial x-coordinate of the node.
     * @param {number} y - The initial y-coordinate of the node.
     */
    constructor(id, canvas, color, name, width, height, x, y, nodeType) {
        this.svg = canvas.svg;
        this.canvas = canvas;
        this.id = id;
        this.width = width;
        this.height = height;
        this.name = name;
        this.color = color;
        this.x = x;
        this.y = y;
        this.offsetX = 0;
        this.offsetY = 0;
        this.nodeType = nodeType;
        this.isDragging = false;
        this.group = this.createNodeGroup();
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
            .attr("id", this.id)
            .attr("class", "node")
            .attr("stroke", this.canvas.getNodeStrokeColor())
            .attr("name", "nodebody")
            .attr("fill", this.color)
            .attr("width", this.width)
            .attr("height", this.height)
            .on("click", () => {
                this.canvas.resetAllColors();
                this.markAsSelected();
                this.canvas.selectedNode = this;
            });

        let node = this;
        if(this.nodeType == 1) {

            let buttonHeight =  this.height - 5;
            let buttonWidth =  40;

            // Append a rectangle (button) to the group
            group.append("rect")
            .attr("class", "injectorButton")
            .attr("x", -buttonWidth)
            .attr("y", this.height / 2 - buttonHeight / 2)
            .attr("width", buttonWidth)
            .attr("height", buttonHeight)
            .attr("stroke", this.canvas.getNodeStrokeColor())
            .on("click", function() {
                // Dispatch the custom event to notify that the button has been clicked
                const moveEvent = new CustomEvent("nodeInjectorButtonClicked", {bubbles: true,
                    detail: {
                        id: node.id
                    }
                });
                node.svg.node().dispatchEvent(moveEvent);
            });
        }
            
        this.label = group.append("text")
        .attr("class", "label")
        .attr("x", this.width / 2)
        .attr("y", this.height / 4)
        .attr("pointer-events", "none") 
        .text(this.name);

        return group;
    }

    removeNode() {
        this.group.remove();
    }

    markAsSelected() {
        this.svg.select('[id=\"' + this.id + '\"]').attr("stroke", "orange");
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
        if(!this.canvas.isMultiNodeDragging) {
            if (d3.select(event.sourceEvent.target).attr("name") === "nodebody") {
                this.isDragging = true;
                const transform = d3.select(event.sourceEvent.target.parentNode).attr("transform");
                const translate = transform.match(/translate\(([^,]+),([^\)]+)\)/);
                this.offsetX = event.x - parseFloat(translate[1]);
                this.offsetY = event.y - parseFloat(translate[2]);
            }
        }
    }

    /**
     * Handles the drag event.
     * @param {d3.D3DragEvent} event - The drag event.
     */
    dragged(event) {
        if (this.isDragging) {
            this.updatePosition(event.x - this.offsetX, event.y - this.offsetY);
        }
    }

    /**
     * Handles the end of the drag event.
     * @param {d3.D3DragEvent} event - The drag event.
     */
    dragEnded(event) {
        this.isDragging = false;

        //Bring the event back to visible area if it goes out of the canvas
        let needUpdate = false;
        let x = this.x;
        let y = this.y;
        if(this.x < 0) { 
            x = 100;
            needUpdate = true;
        }
        if(this.y < 0) {
            y = 100;
            needUpdate = true;
        }

        if(needUpdate) {
            this.updatePosition(x, y);
        }
    }

    updatePosition(x, y) {
        this.x = x;
        this.y = y;

        this.group.attr("transform", `translate(${this.x},${this.y})`);
        if (this.inputPort) {
            this.inputPort.dragLinks();
        }
        if (this.outputPort) {
            this.outputPort.dragLinks();
        }

        // Dispatch the custom event to notify that the node has moved
        const moveEvent = new CustomEvent("nodeMoved", {bubbles: true,
            detail: {
                id: this.id,
                x: this.x,
                y: this.y
            }
        });
        this.svg.node().dispatchEvent(moveEvent);
    }

    addInputPort(id) {
        this.inputPort = new Port(this, this.canvas, 'input', id);
    }

    addOutputPorts(id) {
        this.outputPort = new Port(this, this.canvas, 'output', id);
    }

    getPort(id) {
        if (this.inputPort != null && this.inputPort.id == id)
        {
            return this.inputPort;
        }
        else if (this.outputPort != null && this.outputPort.id == id)
        {
            return this.outputPort;
        }
    }
}