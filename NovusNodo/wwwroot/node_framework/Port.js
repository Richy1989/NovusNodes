/**
 * Represents a port on a node.
 * @class
 */
export class Port {
    width = 10;
    height = 10;
    connectedLinks = [];
    x = 0;
    y = 0;

    /**
     * Creates an instance of Port.
     * @param {string} portType - The type of the port ('input' or 'output').
     * @param {Node} node - The node to which this port belongs.
     * @param {Canvas} canvas - The canvas on which the node is drawn.
     */
    constructor(node, canvas, portType, id) {
        this.node = node;
        this.id = id;
        this.canvas = canvas;
        this.svg = node.svg;
        this.portType = portType;
        this.port = this.createPort();
    }

    createPort() {
        let background = "orange";

        if(this.portType == 'input') {
            this.x = - this.width / 2;
            background ="steelblue";
        }
        else if(this.portType == 'output') {
            this.x = this.node.width - this.width / 2;
        }

        this.y = this.node.height / 2 - this.height / 2;

        return this.node.group.append("rect")
            .attr("class", "port")
            .attr("port-type", this.portType)
            .attr("rx", 3)
            .style("fill", background)
            .style("stroke", this.canvas.getNodeStrokeColor())
            .attr("width", this.width)
            .attr("height", this.height)
            .attr("x", this.x)
            .attr("y", this.y);
    }

    updatePortPosition() {
        if(this.portType == 'input') {
            this.x = - this.width / 2;
        }
        else if(this.portType == 'output') {
            this.x = this.node.width - this.width / 2;
        }

        this.port.attr("x", this.x);
    }

    /**
     * Makes sure all connected linkes are moved when the node (and nodeport) are moved.
     */
    dragLinks() {
        for (let i = 0; i < this.connectedLinks.length; i++) {
            const link = this.connectedLinks[i];
            let line = this.canvas.svg.select('[id=\"' + link.id + '\"]');

            if(this.portType == 'input') {
                line.attr("x2", this.node.x + this.x + this.width / 2)
                    .attr("y2", this.node.y + this.y + this.height / 2);
            }
            else if(this.portType == 'output') {
                line.attr("x1", this.node.x + this.x + this.width / 2)
                    .attr("y1", this.node.y + this.y + this.height / 2);
            }
        }
    }   
}