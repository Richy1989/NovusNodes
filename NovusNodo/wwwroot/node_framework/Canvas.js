import { Node } from "./Node.js";
import { Link } from "./Link.js";

export class Canvas {
    
    isDarkMode = true;
    nodeList = [];
    linkList = [];
    tempLine = null
    selectedLink = null;
    isLinking = false;

    /**
     * Creates an instance of Canvas.
     * @param {string} id - The unique identifier for the canvas.
     */
    constructor(height, width) {
        this.height = height;
        this.width = width;
        this.nodes = [];
        this.links = [];
        this.svg = this.createSvg();
        this.init();
    }

    getLinkColor() {
        return this.isDarkMode ? "white" : "#778899";
    }
    
    getNodeStrokeColor() {
        return this.isDarkMode ? "#f0ead6" : "black";
    }

    getBackgroundColor() {
        return this.isDarkMode ? "#1a1a1a" : "#f0ead6";
    }

    createSvg() {
        let localSVG = d3.select("svg")
            .attr("width", this.width)
            .attr("height", this.height)
            .style("background-color", this.getBackgroundColor())
            .style("border", "1px solid black");
    
        // Grid configuration
        const rows = Math.ceil(this.height / 20);
        const cols = Math.ceil(this.width / 20);
        const spacing = 20; // Distance between dots
        const dotRadius = 0.5;
    
        // Generate grid data
        const gridData = [];
        for (let row = 0; row < rows; row++) {
            for (let col = 0; col < cols; col++) {
                gridData.push({
                    x: col * spacing,
                    y: row * spacing
                });
            }
        }
    
        // Draw the dots
        localSVG.selectAll("circle")
            .data(gridData)
            .enter()
            .append("circle")
            .attr("cx", d => d.x + spacing / 2)
            .attr("cy", d => d.y + spacing / 2)
            .attr("r", dotRadius)
            .style("fill", "steelblue");
    
        return localSVG;
    }

    init() {
        const canvas = this;
        this.svg.on("pointerup", function (event) {
            if (!canvas.isLinking) {
                return;
            }
            canvas.isLinking = false;
            const svgNode = canvas.svg.node(); // Get the underlying DOM element
            const svgRect = svgNode.getBoundingClientRect();

            const svgX = svgRect.x; // X position of the SVG element
            const svgY = svgRect.y; // Y position of the SVG element

            canvas.isLinking = false;

            if (canvas.tempLine) {
                canvas.tempLine.remove();
                canvas.tempLine = null;
            }

            console.log("Mouse up", event.x, event.y);

            for (let i = 0; i < canvas.nodeList.length; i++) {
                const node = canvas.nodeList[i];
                const inputPort = canvas.nodeList[i].inputPort;

                let inputPortX = inputPort.x;
                let inputPortY = inputPort.y;

                const portX = parseFloat(node.x) + parseFloat(inputPortX);
                const portY = parseFloat(node.y) + parseFloat(inputPortY);

                let mouseX = event.x - svgX;
                let mouseY = event.y - svgY;

                if (mouseX > portX && mouseX < portX + 10 && mouseY > portY && mouseY < portY + 10) {
                    canvas.addLink(canvas.sourcePort, inputPort);
                    canvas.drawLinks();
                    break;
                }
            }
            canvas.svg.on("pointermove", null);
        });

        d3.select("body").on("keydown", function (event) {
            if (event.key === "Delete") {
                canvas.deleteSelectedLink();
            }
        });
    }
    
    addNode(node) {
        const canvas = this;
        node.outputPort.port.on("pointerdown", function (event) {
            const transform = d3.select(event.target.parentNode).attr("transform");
            const translateParent = transform.match(/translate\(([^,]+),([^\)]+)\)/);
    
            const portCenterX = parseFloat(translateParent[1]) + parseFloat(d3.select(event.target).attr("x")) + node.outputPort.width / 2;
            const portCenterY = parseFloat(translateParent[2]) + parseFloat(d3.select(event.target).attr("y")) + node.outputPort.height / 2;
    
            console.log("Mouse down", portCenterX, portCenterY);
            canvas.isLinking = true;
            canvas.sourcePort = node.outputPort;//d3.select(event.target);
            canvas.tempLine = canvas.svg.append("line")
                .attr("x1", portCenterX)
                .attr("y1", portCenterY)
                .attr("x2", event.x)
                .attr("y2", event.y)
                .attr("stroke", canvas.getLinkColor())
                .attr("stroke-width", 2);
    
                canvas.svg.on("pointermove", function (event) {
                if (canvas.isLinking && canvas.tempLine) {
                    const [x, y] = d3.pointer(event);
                    canvas.tempLine.attr("x2", x).attr("y2", y);
                }
            });
        });
        this.nodeList.push(node);
    }
    /**
     * Adds a link between two ports.
     * @param {Port} sourcePort - The source port.
     * @param {Port} targetPort - The target port.
     */
    addLink(sourcePort, targetPort) {
        const link = new Link(crypto.randomUUID(), sourcePort, targetPort);
        this.linkList.push(link);
    }
    /**
     * Draws all the links on the canvas.
     */
    drawLinks() {
        for (let i = 0; i < this.linkList.length; i++) {
            const link = this.linkList[i];
            const sourcePort = link.sourcePort;
            const targetPort = link.targetPort;

            let sourcePortX = sourcePort.node.x + sourcePort.x + 5;
            let sourcePortY = sourcePort.node.y + sourcePort.y + 5;

            let targetPortX = targetPort.node.x + targetPort.x + 5;
            let targetPortY = targetPort.node.y + targetPort.y + 5;
            
            const canvas = this;
            
            const line = this.svg.append("line")
                .attr("class", "link")
                .attr("id", link.id)
                .attr("x1", sourcePortX)
                .attr("y1", sourcePortY)
                .attr("x2", targetPortX)
                .attr("y2", targetPortY)
                .attr("stroke", this.getLinkColor())
                .on("click", function () {
                    canvas.resetAllLinkColors();
                    canvas.selectedLink = link.id;
                    d3.select(this).attr("stroke", "orange");
                    console.log("Selected link", link.id);
                });

                link.line = line;

            sourcePort.connectedLinks.push(line);
            targetPort.connectedLinks.push(line);
        }
    }

    resetAllLinkColors() {
        for (let i = 0; i < this.linkList.length; i++) {
            const link = this.linkList[i];
            link.line.attr("stroke", this.getLinkColor());
        }
    }

    deleteSelectedLink() {
        if (this.selectedLink) {
           // console.log("Deleting link", this.selectedLink.id);   
            console.log("Deleting link", this.selectedLink);   
            // Remove the line element from the SVG
            d3.select("#"+this.selectedLink).remove();
            
            // Remove the link from the link list
            //this.linkList = this.linkList.filter(link => link.id !== this.selectedLink.id); // Remove the link from the list
            
            // Remove the link from the connected links list of the source and target ports
            //this.selectedLink.sourcePort.connectedLinks = this.selectedLink.sourcePort.connectedLinks.filter(link => link.id !== this.selectedLink.id);
            //this.selectedLink.targetPort.connectedLinks = this.selectedLink.targetPort.connectedLinks.filter(link => link.id !== this.selectedLink.id);
            this.selectedLink = null;
        }
    }
}