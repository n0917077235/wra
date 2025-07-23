export default class LayerMap {
    // layerId => [layer]
    layers = new Map();

    get(layerId) {
        return this.layers.get(layerId);
    }

    add(layerId, layer) {
        let v = this.layers.get(layerId);

        if (v === undefined) {
            this.layers.set(layerId, [layer]);
        } else {
            v.push(layer);
        }
    }
}