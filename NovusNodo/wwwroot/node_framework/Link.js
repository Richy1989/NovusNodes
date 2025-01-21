export class Link {
    line = null;

    constructor(id, sourcePort, targetPort) {
        this.id = id;
        this.sourcePort = sourcePort;
        this.targetPort = targetPort;
    }
}