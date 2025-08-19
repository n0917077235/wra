export default class LineHighlight {
    // layerItems: { layerProps, map, popup }
    static enable(layerItems) {
        let map = layerItems.map;
        map.on('pointermove', e => LineHighlight._handle(e, layerItems));
        map.on('click', e => LineHighlight._handle(e, layerItems));
    }

    static _handle(event, layerItems) {
        let map = layerItems.map;
        let first = null;
        let layerFilter = layer => ol.util.getUid(layer) === ol.util.getUid(layerItems.vectorLayer);

        map.forEachFeatureAtPixel(event.pixel, f => {
            if (first !== null) return;
            let type = f.getGeometry().getType();
            if (type == "LineString") first = f;
        }, { layerFilter });

        if (first === null) return;
        LineHighlight._highlight(layerItems, first, event)
    }

    static _highlight(layerItems, f, event) {
        let layerProps = layerItems.layerProps;
        let changed = layerItems.changedItems;
        LineHighlight._resetOtherLineStyles(changed);
        let popup = layerItems.popup;

        // Highlight current one. Increse brightness by 40%
        let color = LineHighlight._lightenColor(layerProps.strokecolor, 40);

        let stroke = new ol.style.Stroke({
            color,
            width: layerProps.strokew + 2.5
        });

        let style = new ol.style.Style({ stroke: stroke });
        f.setStyle(style);
        changed.value.push({ feature: f, layerProps });

        // Show feature name popup
        let name = LineHighlight.getFeatureName(f)?.name;

        if (name) {
            let coordinate = event.coordinate;

            popup.content = {
                mode: 'simple',
                text: name,
                style: { whiteSpace: 'nowrap' },
            };

            popup.overlay.setPosition(coordinate);
        }
    }

    static _resetOtherLineStyles(changedItems) {
        for (let x of changedItems.value) {
            let stroke = new ol.style.Stroke({
                color: x.layerProps.strokecolor,
                width: x.layerProps.strokew,
            });

            let style = new ol.style.Style({ stroke: stroke });
            x.feature.setStyle(style);
        }

        changedItems.value = [];
    }

    static getFeatureName(f) {
        let name = f.get("name");
        if (name == null) name = f.get("NAME");
        if (!name) return name;
        let words = name.split(';');

        return {
            name: words[0],
            url: words.length >= 2 ? words[1] : null
        };
    }

    static _lightenColor(col, amt) {
        var usePound = false;

        if (col.charAt(0) == '#') {
            col = col.slice(1);
            usePound = true;
        }

        var num = parseInt(col, 16);
        var r = (num >> 16) + amt;
        if (r > 255) r = 255;
        else if (r < 0) r = 0;
        var b = ((num >> 8) & 0x00FF) + amt;
        if (b > 255) b = 255;
        else if (b < 0) b = 0;
        var g = (num & 0x0000FF) + amt;
        if (g > 255) g = 255;
        else if (g < 0) g = 0;
        return (usePound ? "#" : "") + (g | (b << 8) | (r << 16)).toString(16);
    }
}