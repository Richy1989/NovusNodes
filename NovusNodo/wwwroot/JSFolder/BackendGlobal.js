/**
 * Executes user-provided code with given parameters.
 *
 * @param {string} code - The user code to execute.
 * @param {string} parameters - The parameters to pass to the user code, in JSON format.
 * @returns {string} The result of the executed code, in JSON format.
 */
function RunUserCode(code, parameters)
{
    //console.log("Running user code: " + code);
    //console.log("With parameters: " + parameters);

    // Create a new function with `msg` as the parameter
    const func = new Function('msg', code);
    let result = func(JSON.parse(parameters));
    // Execute the function with the provided parameter
    return JSON.stringify(result);
}

module.exports = { RunUserCode };