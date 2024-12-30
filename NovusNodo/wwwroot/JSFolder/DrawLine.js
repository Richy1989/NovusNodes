//https://codepen.io/Ragtime-Kitty/pen/yrNNdq


function DrawLineOld(starterID, endID) {
    var canvas = document.getElementById("canvas"),
        ctx = canvas.getContext("2d"),
        $1 = $('#' + starterID),
        $2 = $('#' + endID);

    var drawThatShit = function () {
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

        var $1_left = $1.offset().left,
            $1_top = $1.offset().top,
            $2_left = $2.offset().left,
            $2_top = $2.offset().top;

        ctx.beginPath();
        ctx.moveTo($1_left + $1.width() / 2, $1_top + $1.height() / 2);
        ctx.quadraticCurveTo($1.width() * 2, $1.height() * 2, $2_left, $2_top + $2.height() / 2);
        ctx.stroke();
    }


    drawThatShit();


    $(window).resize(function () {
        drawThatShit();
    });

    

}