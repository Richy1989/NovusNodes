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
    constructor(id, canvas, color, name, x, y, nodeType, startIconPath = null, endIconPath = null) {
        this.svg = canvas.svg;
        this.canvas = canvas;
        this.id = id;

        this.name = name;
        this.color = color;
        this.x = x;
        this.y = y;
        this.x_old = x;
        this.y_old = y;
        this.offsetX = 0;
        this.offsetY = 0;
        this.nodeType = nodeType;
        this.isDragging = false;
        
        this.startIconPath = startIconPath;
        this.endIconPath = endIconPath;

        this.width = 120;
        this.height = 40;

        this.iconWidth = this.height - 6;

        this.calculateWiddth();
        this.group = this.createNodeGroup();
        this.setLabelText(name);
    }

    /**
     * Creates the SVG group element for the node and sets up the drag behavior.
     * @returns {d3.Selection} The created SVG group element.
     */
    createNodeGroup() {
        const group = this.canvas.nodeGroup.append("g")
            .attr("class", "node-group")
            .attr("transform", `translate(${this.x},${this.y})`)
             .call(d3.drag()
                .on("start", this.dragStarted.bind(this))
                .on("drag", this.dragged.bind(this))
                .on("end", this.dragEnded.bind(this)));

        // Append the body rectangle to the group
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
            })
            .on("dblclick", () => {
                this.onNodeBodyDoubleClick();
            });

        //Append the start icon
        if(this.startIconPath != null) {
            // Append an image inside the group
            group.append('image')
                .attr('x', 8)   // Position of the image
                .attr('y', (this.height - this.iconWidth) / 2)
                .attr('width', this.iconWidth) // Width of the image
                .attr('height', this.iconWidth) // Height of the image
                .attr("pointer-events", "none")
                .attr('href', this.startIconPath);    // Add a class for additional styling
        }

        //Append the injector button
        let node = this;
        if(this.nodeType == 1) {
            let buttonHeight =  this.height - 10;
            let buttonWidth =  30;

            // Append a rectangle (button) to the group
            group.append("rect")
            .attr("class", "injectorButton")
            .attr("x", -buttonWidth)
            .attr("y", this.height / 2 - buttonHeight / 2)
            .attr("width", buttonWidth)
            .attr("height", buttonHeight)
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
    
        console.log(`Node width before label: ${this.width}`);
        //Append the text label
        this.label = group.append("text")
        .attr("class", "label")
        .attr("x", this.width / 2)
        .attr("y", this.height / 2)
        .attr("pointer-events", "none")
        .text(this.name);

        this.calculateWiddth();

        //Append the start icon
        if(this.endIconPath != null) {
            // Append an image inside the group
            this.endIcon = group.append('image')
                .attr('x', this.width - this.iconWidth - 5)   // Position of the image
                .attr('y', (this.height - this.iconWidth) / 2)
                .attr('width', this.iconWidth) // Width of the image
                .attr('height', this.iconWidth) // Height of the image
                .attr("pointer-events", "none")
                .attr('href', this.endIconPath)  // Path to the image file
        }

        return group;
    }

    calculateWiddth() {
        let textWidth = 120;
        if(this.label != null)
            textWidth = this.label.node().getBBox().width;
        
        let multiplier = 0;
        if(this.startIconPath != null)
            multiplier++;
        if(this.endIconPath != null)
            multiplier++;

        this.width = Math.max(120, textWidth + (this.iconWidth + 20) * multiplier + 20);
    }

      /**
     * Updates the dimensions of the node and moved the port positions accordingly.
     */
    /**
     * Updates the dimensions of the node by recalculating its width and adjusting the positions of its elements.
     * 
     * This method performs the following actions:
     * 1. Recalculates the width of the node.
     * 2. Updates the width attribute of the rectangle representing the node.
     * 3. Adjusts the x-coordinate of the label based on the presence of start and end icons.
     * 4. Updates the position of the output port if it exists.
     * 
     * @method updateNodeDimensions
     */
      updateNodeDimensions() {
        this.calculateWiddth();
        this.group.select("rect.node").attr("width", this.width);
        
        let isStartIcon = 0;
        if(this.startIconPath != null)
            isStartIcon = 1;

        let isEndIcon = 0;
        if(this.endIconPath != null)
            isEndIcon = 1;

        let xcoord = this.iconWidth * isStartIcon + (this.width - this.iconWidth * isStartIcon - this.iconWidth * isEndIcon) / 2;

        this.label.attr("x", xcoord);
        if(this.endIcon != null)
            this.endIcon.attr("x", this.width - this.iconWidth - 5);
        
        if(this.outputPort != null) {
            this.outputPort.updatePortPosition();
        }

        if (this.outputPort) {
            this.outputPort.dragLinks();
        }
    }

    /**
     * Sets the text of the label and resizes the node if necessary.
     * @param {string} text - The text to set.
     */
    setLabelText(text) {
        this.label.text(text);
        this.updateNodeDimensions();
    }

    setEnabled(enabled) {
        if (enabled) {
            this.group.select("rect.node")
                .attr("stroke", this.canvas.getNodeStrokeColor())
                .attr("stroke-dasharray", "0")
                .attr("fill", this.color)
                .attr("opacity", 1);
        } else {
            this.group.select("rect.node")
                .attr("stroke", "gray")
                .attr("stroke-dasharray", "4,2")
                .attr("fill", "#f0f0f0")
                .attr("opacity", 0.5);
        }
    }

    /**
     * Handles the double-click event on the node body.
     */
    onNodeBodyDoubleClick() {
        console.log(`Node double-clicked: ID=${this.id}`);
        // Dispatch the custom event to notify that the button has been clicked
        const moveEvent = new CustomEvent("nodeDoubleClicked", {bubbles: true,
            detail: {
                id: this.id
            }
        });
        this.svg.node().dispatchEvent(moveEvent);
    }

    removeNode() {
        this.group.remove();
    }

    markAsSelected() {
        this.svg.select('[id=\"' + this.id + '\"]').attr("stroke", "orange");
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
            let x = event.x - this.offsetX;
            let y = event.y - this.offsetY;

            if(this.x != x || this.y != y) {
                this.updatePosition(event.x - this.offsetX, event.y - this.offsetY);
            }
        }
    }

    /**
     * Handles the end of the drag event.
     * @param {d3.D3DragEvent} event - The drag event.
     */
    dragEnded(event) {
        this.isDragging = false;
        
        const transform = this.canvas.CanvasZoom.currentTransformation;

        let x = (this.x * transform.k + transform.x) / transform.k;
        let y = (this.y * transform.k + transform.y) / transform.k;

        let updateNeeded = false;
        // Check boundaries
        if (x < 0) {
            x = 40;
            updateNeeded = true;
        }
        if (y < 0) {
            y = 40;
            updateNeeded = true;
        }

        if (x > this.canvas.width / transform.k) {
            x = this.canvas.width / transform.k;
        }

        if (y > this.canvas.height / transform.k) {
            y = this.canvas.height / transform.k;
        }

        if(updateNeeded) {
            this.updatePosition(x, y);
        }

        if (this.x_old != this.x || this.y_old != this.y) {
            this.x_old = this.x;
            this.y_old = this.y;
        
            console.log(`Node moved: ID=${this.id}, X=${this.x}, Y=${this.y}`);
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