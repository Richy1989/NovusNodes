// Class to handle zooming functionality for the canvas
export class CanvasZoom {
    
    // Reference to the canvas
    canvas = null;
    // Current zoom transformation state
    currentTransformation = { x: 0, y: 0, k: 1 };

    // Zoom scale limits
    zoomScale = {
        min: 1,
        max: 8
    }

    // Constructor to initialize the CanvasZoom instance
    constructor(canvas) {
        this.canvas = canvas;
        // Configure and apply zoom behavior to the canvas element
        d3.select('[id="' + this.canvas.id + '"]')
            .call(this.configZoom((e) => this.zoomed(e, canvas)))
            .on("dblclick.zoom", null); // Disable zoom on double-click
    }

    // Method to configure the zoom behavior
    configZoom(cb) {
        return d3.zoom()
            .filter(this.filterZoom) // Apply custom filter for zoom events
            .scaleExtent([this.zoomScale.min, this.zoomScale.max]) // Set zoom scale limits
            .translateExtent([[0, 0], [this.canvas.width, this.canvas.height]]) // Set translation limits
            .on("zoom", cb); // Set the zoom event callback
    }

    // Method to reset the zoom to the default state
    resetZoom() {
        // Remove "zoomed" CSS classname from all interactive elements
        this.canvas.nodeGroup.classed("zoomed", false);
        this.canvas.linkGroup.classed("zoomed", false);

        // Reset zoom transformation to the default state (k = 1, x = 0, y = 0)
        let zoom = this.configZoom((e) => this.zoomed(e, this.canvas));
        d3.select('[id="' + this.canvas.id + '"]').transition()
            .duration(500)
            .call(zoom.transform, d3.zoomIdentity);
    }
    
    // Custom filter for zoom events
    filterZoom(event) {
        // Allow zoom only when the Shift key is pressed and not on double-click
        return (event.type !== "dblclick" && event.shiftKey);
    }

    // Method to handle the zoom event
    zoomed(e, canvas) {
        const { transform } = e;
        // Update the current transformation state
        this.currentTransformation = transform;
        // Apply the transformation to the node and link groups
        canvas.nodeGroup.attr('transform', this.currentTransformation);
        canvas.linkGroup.attr('transform', this.currentTransformation);
    }
}