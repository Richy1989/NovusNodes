/**
 * A reference to the Novus object.
 * @type {any}
 */
let NovusUIManagementRef = undefined;

/*let JointJSPaperComponentRef = undefined;*/

/**
 * Sets the NovusUIManagementRef to the provided value.
 * @param {any} novusReference - The reference to set.
 */
function GJSSetNovusUIManagementRef(novusReference) {
    console.log("Setting NovusUIManagementRef to " + novusReference);
    NovusUIManagementRef = novusReference;
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

/*let codeMirrorInstance;*/
function initializeCodeMirror(elementId, initialCode) {
    const element = document.getElementById(elementId);
    if (element) {
        codeMirrorInstance = CodeMirror(element, {
            value: initialCode || "",
            mode: "javascript",
            lineNumbers: true,
            tabSize: 2,
            indentWithTabs: true,
        });
    }
}

//function getCodeMirrorValue() {
//    return codeMirrorInstance ? codeMirrorInstance.getValue() : "";
//}

//function setCodeMirrorValue(newValue) {
//    if (codeMirrorInstance) {
//        codeMirrorInstance.setValue(newValue);
//    }
//}

//window.scrollToBottom = (element) => {
//    element.scrollTop = element.scrollHeight;
//};

function scrollToBottom(elementid)
{
    element = document.getElementById(elementid);
    element.scrollTop = element.scrollHeight;
}

function lightenColor(hex, percent) {
    // Convert HEX to RGB
    let r = parseInt(hex.slice(1, 3), 16);
    let g = parseInt(hex.slice(3, 5), 16);
    let b = parseInt(hex.slice(5, 7), 16);

    // Increase RGB values by the given percentage
    r = Math.min(255, r + (255 - r) * (percent / 100));
    g = Math.min(255, g + (255 - g) * (percent / 100));
    b = Math.min(255, b + (255 - b) * (percent / 100));

    // Convert RGB back to HEX
    r = Math.round(r).toString(16).padStart(2, '0');
    g = Math.round(g).toString(16).padStart(2, '0');
    b = Math.round(b).toString(16).padStart(2, '0');

    return `#${r}${g}${b}`;
}