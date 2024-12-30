function dragAndDrop(className) {
    // target elements with the "draggable" class
    interact(className)
        .draggable({
            // enable inertial throwing
            inertia: false,
            // keep the element within the area of it's parent
            modifiers: [
                interact.modifiers.restrictRect({
                    restriction: 'parent',
                    endOnly: true
                })
            ],
            // enable autoScroll
            autoScroll: true,

            listeners: {
                start(event) {
                    console.log(event.type, event.target)
                },
                // call this function on every dragmove event
                move: dragMoveListener
            }
        })

    function dragMoveListener(event) {
        var target = event.target
        // keep the dragged position in the data-x/data-y attributes
        var x = (parseFloat(target.getAttribute('data-x')) || 0) + event.dx
        var y = (parseFloat(target.getAttribute('data-y')) || 0) + event.dy

        console.log("Event Set: " + " to " + x + ", " + y)

        // translate the element
        target.style.transform = 'translate(' + x + 'px, ' + y + 'px)'

        // update the posiion attributes
        target.setAttribute('data-x', x)
        target.setAttribute('data-y', y)

        DotNet.invokeMethodAsync('NovusNodo', 'ElementMoved', target.id, x, y)
    }

    // this function is used later in the resizing and gesture demos
    //window.dragMoveListener = dragMoveListener
};

function SetElementPosition(elementID, xIn, yIn)
{
    x = parseFloat(xIn)
    y = parseFloat(yIn)
    console.log("Setting position of " + elementID + " to " + x + ", " + y)

    var element = document.getElementById(elementID)

    element.style.transform = 'translate(' + x + 'px, ' + y + 'px)'

    //element.style.position = "absolute";
    //element.style.left = x + 'px';
    //element.style.top = y + 'px';

    element.setAttribute('data-x', x)
    element.setAttribute('data-y', y)
}

function DrawLine(from, to)
{
    var startPoint = document.getElementById(from);
    var endPoint = document.getElementById(to);

    var canvas = document.getElementById("canvas"),
        ctx = canvas.getContext("2d"),
        $1 = $('#' + from),
        $2 = $('#' + to);

    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;

    var ramp = ctx.createLinearGradient(0, 0, canvas.width / 1.5, 0);
    ramp.addColorStop("0", "blue");
    ramp.addColorStop("0.8", "magenta");
    ramp.addColorStop("1", "red");

    ctx.setLineDash([20, 10]);
    ctx.strokeStyle = ramp;
    ctx.lineWidth = 5;

    ctx.clearRect(0, 0, canvas.width, canvas.height);

    var $1_left = parseFloat(startPoint.getAttribute('data-x')),
        $1_top = parseFloat(startPoint.getAttribute('data-y')),
        $2_left = parseFloat(endPoint.getAttribute('data-x')),
        $2_top = parseFloat(endPoint.getAttribute('data-y'));

    console.log("Drawing line from " + $1_left + ", " + $1_top + " to " + $2_left + ", " + $2_top)

    console.log("From Height: " + $1.height() + " Width: " + $1.width())
    //ctx.beginPath();
    //ctx.moveTo($1_left + $1.width(), $1_top + $1.height() / 2);
    //ctx.quadraticCurveTo($1.width() * 2, $1.height() * 2, $2_left, $2_top + $2.height() / 2);
    //ctx.stroke();

    // Adjust control points for a smoother curve
    ctx.beginPath();

    $1_left = $1_left + startPoint.offsetWidth;
    $1_top = $1_top + startPoint.offsetHeight / 2;

    ctx.moveTo($1_left, $1_top);

    // Control points for a smooth Bezier curve
    var controlX1 = $1_left + 100; // Control point 1: 100px to the right of the start point
    var controlY1 = $1_top; // Control point 1: same height as the start point
    var controlX2 = $2_left - 100; // Control point 2: 100px to the left of the end point
    var controlY2 = $2_top; // Control point 2: same height as the end point

    // Create a Bezier curve between the two elements
    ctx.bezierCurveTo(controlX1, controlY1, controlX2, controlY2, $2_left, $2_top);

    // Stroke the path to draw the line
    ctx.stroke();
}