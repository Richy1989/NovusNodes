export class CanvasZoom {
    
    canvas = null;
    currentTransformation = { x: 0, y: 0, k: 1 };

    zoomScale = {
        min: 1,
        max: 8
    }

    constructor(canvas)
    {
        this.canvas = canvas;
        d3.select('[id=\"' + this.canvas.id + '\"]')
        .call(this.configZoom((e)=> 
            {
                const {transform} = e;
                this.currentTransformation = transform;
                canvas.nodeGroup.attr('transform', this.currentTransformation);
                canvas.linkGroup.attr('transform', this.currentTransformation);
            }))
        .on("dblclick.zoom", null);
    }

    configZoom(cb) {
        return d3.zoom()
            .filter(this.filterZoom)
            .scaleExtent([this.zoomScale.min, this.zoomScale.max])
            .translateExtent([[0, 0], [this.canvas.width, this.canvas.height]])
            .on("zoom", cb);
    }
    
    filterZoom(event) {
        return (event.type !== "dblclick" && event.shiftKey ); // event.type !== "wheel" && 
    }

    zoomed(e) {
        const {transform} = e;
       /*  d3.select(".panning-layer").attr("transform", transform); */
        this.canvas.nodeGroup.attr('transform', transform);
        this.canvas.linkGroup.attr('transform', transform);
        // You can add anything you wish to perform during zoomEvent
    }
}