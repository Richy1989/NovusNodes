/**
 * A reference to the Novus object.
 * @type {any}
 */
let NovusReference = undefined;

/**
 * Sets the NovusReference to the provided value.
 * @param {any} novusReference - The reference to set.
 */
function GJSSetNovusReference(novusReference) {
    console.log("Setting NovusReference to " + novusReference);
    NovusReference = novusReference;
}

/**
 * Reloads the current page.
 */
function GJSReloadPage() {
    location.reload();
}

/**
 * Initializes the custom sidebar with resizable functionality.
 */
function GJSInitSettingsSideBar() {
    let sidebar = document.querySelector(".sidebarCustom");
    let resizer = document.querySelector(".resizerCustom");

    let initialX, initialWidth;

    /**
     * Handles the mousedown event on the resizer.
     * @param {MouseEvent} e - The mousedown event.
     */
    let mousedownListener = (e) => {
        initialX = e.clientX;
        initialWidth = window.getComputedStyle(sidebar).width;
        initialWidth = parseInt(initialWidth, 10);

        document.addEventListener("mousemove", mousemoveListener);
        document.addEventListener("mouseup", mouseupListener);
    };

    /**
     * Handles the mousemove event to resize the sidebar.
     * @param {MouseEvent} e - The mousemove event.
     */
    let mousemoveListener = (e) => {
        let newX = e.clientX;
        let difX = newX - initialX;

        let newWidth = (-1) * (difX - initialWidth);

        if (newWidth < 0) newWidth = 1;

        sidebar.style.width = `${newWidth}px`;
    };

    /**
     * Handles the mouseup event to stop resizing the sidebar.
     * @param {MouseEvent} e - The mouseup event.
     */
    let mouseupListener = (e) => {
        document.removeEventListener("mousemove", mousemoveListener);
        document.removeEventListener("mouseup", mouseupListener);
    };

    resizer.addEventListener("mousedown", mousedownListener);
}

function GJSRunUserCode(code, parameters) {

    console.log("Running user code: " + code);
    console.log("With parameters: " + parameters);

    try {
        // Create a new function with `param` as the parameter
        const func = new Function('msg', code);
        let result = func(parameters);
        console.log("Result: " + result);
        // Execute the function with the provided parameter
        return result
    } catch (error) {
        console.error('Error executing code:', error);
    }
}