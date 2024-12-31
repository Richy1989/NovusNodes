/**
 * The graph object for the JointJS diagram.
 * @type {joint.dia.Graph}
 */
const graph = undefined;

/**
 * The paper object for the JointJS diagram.
 * @type {joint.dia.Paper}
 */
const paper = undefined;

/**
 * Creates a JointJS paper and attaches it to the specified container.
 * @param {string} paperContainerName - The ID of the container element.
 */
function CreatePaper(paperContainerName) { 
    const container = document.getElementById(paperContainerName);

    // Create the graph
    graph = new joint.dia.Graph();

    // Dynamically set paper size to fit the container
    paper = new joint.dia.Paper({
        el: container,
        model: graph,
        width: container.offsetWidth,
        height: container.offsetHeight,
        gridSize: 10
    });

    // Optionally, resize the paper when the container size changes
    window.addEventListener('resize', () => {
        paper.setDimensions(container.offsetWidth, container.offsetHeight);
    });
}

/**
 * Adds a custom HTML node to the JointJS graph.
 */
function AddNode() {
    // Define a custom element
    const CustomHTMLShape = joint.dia.Element.define('html.Custom', {
        size: { width: 100, height: 50 },
    }, {
        // Customize the element rendering
        markup: '<foreignObject width="100%" height="100%"><body xmlns="http://www.w3.org/1999/xhtml"></body></foreignObject>'
    });

    // Instantiate the custom element
    const customElement = new CustomHTMLShape();
    customElement.position(150, 150);
    customElement.resize(200, 100);

    // Inject an existing div into the custom shape
    const customDiv = document.getElementById('customDiv');
    customDiv.style.display = 'block'; // Make the div visible
    customElement.findView(paper).vel.appendChild(customDiv);

    // Add the element to the graph
    graph.addCell(customElement);
}
/*
// Create two shapes
const rect1 = new joint.shapes.standard.Rectangle();
rect1.position(100, 100);
rect1.resize(100, 40);
rect1.attr({
    body: { fill: 'blue' },
    label: { text: 'Rectangle 1', fill: 'white' }
});
rect1.addTo(graph);

const rect2 = new joint.shapes.standard.Rectangle();
rect2.position(400, 100);
rect2.resize(100, 40);
rect2.attr({
    body: { fill: 'green' },
    label: { text: 'Rectangle 2', fill: 'white' }
});
rect2.addTo(graph);

// Add ports for connection
rect1.addPort({ id: 'port1', group: 'in' });
rect2.addPort({ id: 'port2', group: 'out' });

// Define port styles (optional)
paper.options.markup = [
    {
        tagName: 'circle',
        selector: 'portBody',
        attributes: { r: 6, fill: '#0000ff' }
    }
];
*/