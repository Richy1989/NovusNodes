function reloadPage() {
    location.reload();
}

function InitCustomSideBar() {

    let sidebar = document.querySelector(".sidebarCustom");
    let resizer = document.querySelector(".resizerCustom");

    let initialX, initialWidth;

    let mousedownListener = (e) => {
        initialX = e.clientX;
        initialWidth = window.getComputedStyle(sidebar).width;
        initialWidth = parseInt(initialWidth, 10);

        document.addEventListener("mousemove", mousemoveListener);
        document.addEventListener("mouseup", mouseupListener);
    };

    let mousemoveListener = (e) => {
        let newX = e.clientX;
        let difX = newX - initialX;

        let newWidth = (-1) * (difX - initialWidth);

        console.log(newWidth);

        if (newWidth < 0) newWidth = 1;
        
        sidebar.style.width = `${newWidth}px`;
    };

    let mouseupListener = (e) => {
        document.removeEventListener("mousemove", mousemoveListener);
        document.removeEventListener("mouseup", mouseupListener);
    };

    resizer.addEventListener("mousedown", mousedownListener);
}