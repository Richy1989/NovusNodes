function dragAndDrop(className) {
    // target elements with the "draggable" class
    interact(className).draggable({
            // enable inertial throwing
            inertia: false,
            // keep the element within the area of it's parent
            modifiers: [
                interact.modifiers.restrictRect({
                    restriction: '.main',
                    endOnly: true
                })
            ],
            // enable autoScroll
            autoScroll: true,

            listeners: {
                // call this function on every dragmove event
                move: dragMoveListener
            }
    });

    function dragMoveListener(event) {
        var target = event.target.parentElement;
        // keep the dragged position in the data-x/data-y attributes
        var x = (parseFloat(target.getAttribute('data-x')) || 0) + event.dx;
        var y = (parseFloat(target.getAttribute('data-y')) || 0) + event.dy;

        // translate the element
        target.style.transform = 'translate(' + x + 'px, ' + y + 'px)';

        // update the posiion attributes
        target.setAttribute('data-x', x);
        target.setAttribute('data-y', y);

        DotNet.invokeMethodAsync('NovusNodo', 'ElementMoved', target.id, x, y);
    };

    // this function is used later in the resizing and gesture demos
    //window.dragMoveListener = dragMoveListener
};

function SetElementPosition(elementID, xIn, yIn)
{
    x = parseFloat(xIn)
    y = parseFloat(yIn)
    
    var element = document.getElementById(elementID)

    element.style.transform = 'translate(' + x + 'px, ' + y + 'px)'

    element.setAttribute('data-x', x)
    element.setAttribute('data-y', y)
}

function ClearCanvas()
{
    var canvas = document.getElementById("canvas"),
        ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);
}

function DrawLineCoordinate(fromX, fromY, toX, toY)
{
    console.log("Drawing line from " + fromX + ", " + fromY + " to " + toX + ", " + toY);
    var canvas = document.getElementById("canvas"),
        ctx = canvas.getContext("2d");

    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;

    var ramp = ctx.createLinearGradient(0, 0, canvas.width / 1.5, 0);
    ramp.addColorStop("0", "blue");
    ramp.addColorStop("0.8", "magenta");
    ramp.addColorStop("1", "red");

    //ctx.setLineDash([20, 10]);
    ctx.strokeStyle = ramp;
    ctx.lineWidth = 5;

    //ctx.clearRect(0, 0, canvas.width, canvas.height);

    var $1_left = fromX,
        $1_top = fromY,
        $2_left = toX,
        $2_top = toY;

    // Adjust control points for a smoother curve
    ctx.beginPath();

    //console.log("Drawing line from " + $1_left + ", " + $1_top + " to " + $2_left + ", " + $2_top);
    //console.log("From Height: " + startPoint.offsetHeight + " Width: " + startPoint.offsetWidth);

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

function DrawLine(from, to)
{
    var startPoint = document.getElementById(from);
    var endPoint = document.getElementById(to);

    var $1_left = parseFloat(startPoint.getAttribute('data-x')),
        $1_top = parseFloat(startPoint.getAttribute('data-y')),
        $2_left = parseFloat(endPoint.getAttribute('data-x')),
        $2_top = parseFloat(endPoint.getAttribute('data-y'));


    $1_left = $1_left + startPoint.offsetWidth;
    $1_top = $1_top + startPoint.offsetHeight / 2;

    $2_left = $2_left + 20; //ToDo: Fix this! Why do I need that?
    $2_top = $2_top + endPoint.offsetHeight / 2;

    DrawLineCoordinate($1_left, $1_top, $2_left, $2_top);
}

function dragConnectionListener(event) {
    var target = event.target.parentElement;
    // keep the dragged position in the data-x/data-y attributes

    var elemX = parseFloat(target.getAttribute('data-x'));
    var elemY = parseFloat(target.getAttribute('data-y'));

    var x = (elemX || 0) + event.dx;
    var y = (elemY || 0) + event.dy;

    DrawLineCoordinate(elemX, elemY, x, y);
};