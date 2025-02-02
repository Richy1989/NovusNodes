/**
 * Represents a link between two nodes.
 */
export class Link {
    /**
     * The line representing the link.
     * @type {null}
     */
    line = null;

    /**
     * Creates an instance of Link.
     * @param {string} id - The unique identifier for the link.
     * @param {Object} sourcePort - The source port of the link.
     * @param {Object} targetPort - The target port of the link.
     */
    constructor(id, sourcePort, targetPort) {
        this.id = id;
        this.sourcePort = sourcePort;
        this.targetPort = targetPort;
    }
}