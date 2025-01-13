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
    console.log("Scrolling to bottom of ID:" + elementid);
    element = document.getElementById(elementid);
    console.log("Scrolling to bottom of Element:" + element);
    element.scrollTop = element.scrollHeight;
    console.log("Scrolling to bottom of Element Scroll Height:" + element.scrollHeight);
}