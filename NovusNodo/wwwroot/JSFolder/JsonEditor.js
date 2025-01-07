import { createJSONEditor } from './vanilla-jsoneditor.min.js';

export function JsonEditorCreateEditor(editorElementId, contentIn) {

    //function handleChange(
    //    updatedContent,
    //    previousContent,
    //    { contentErrors, patchResult }
    //) {
    //    // content is an object { json: JSONValue } | { text: string }
    //    console.log('onChange', {
    //        updatedContent,
    //        previousContent,
    //        contentErrors,
    //        patchResult,
    //    });
    //    content = updatedContent;
    //}

    /* import { createJSONEditor } from 'https://cdn.jsdelivr.net/npm/vanilla-jsoneditor@2/standalone.js';*/

    contentIn = JSON.parse(contentIn);

    // content is an object { json: JSONValue } | { text: string }
    let content = {
        json: contentIn,
        text: undefined,
    };

    const editor = createJSONEditor({
        target: document.getElementById(editorElementId),

        props: {
            mainMenuBar: false,
            content,
            mode: 'tree',
            /*onChange: handleChange,*/
        },
    });

    // use methods get, set, update, and onChange to get data in or out of the editor.
    // Use updateProps to update properties.

}