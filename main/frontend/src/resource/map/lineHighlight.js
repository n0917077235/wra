export default class LineHighlight {
    static enable(layerProps, map) {
        let changedItems = [];
        map.on('pointermove', e => LineHighlight._handle(e, map, layerProps, changedItems));
        map.on('click', e => LineHighlight._handle(e, map, layerProps, changedItems));
    }

    static _handle(event, map, layerProps, changedItems) {
        let first = null;

        map.forEachFeatureAtPixel(event.pixel, f => {
            if (first !== null) return;
            let type = f.getGeometry().getType();
            if (type == "LineString") first = f;
        });

        if (first === null) return;
        LineHighlight._getFeatureName(layerProps.layerId, first);
        LineHighlight._highlight(changedItems, layerProps, first)
    }

    static _highlight(changedItems, layerProps, f) {
        // Reset other line styles
        for (let x of changedItems) {
            let stroke = new ol.style.Stroke({
                color: x.layerProps.strokecolor,
                width: x.layerProps.strokew,
            });

            let style = new ol.style.Style({ stroke: stroke });
            x.feature.setStyle(style);
        }

        // Clear the array
        changedItems.length = 0;

        // Highlight current one. Increse brightness by 40%
        let color = LineHighlight._lightenColor(layerProps.strokecolor, 40);

        let stroke = new ol.style.Stroke({
            color,
            width: layerProps.strokew + 2.5
        });

        let style = new ol.style.Style({ stroke: stroke });
        f.setStyle(style);
        changedItems.push({ feature: f, layerProps });
        // var tgpoint = e.point;

        // ls2.forEach((v, k) => {
        //     if (v instanceof TGOS.TGInfoWindow) v.close();
        // });

        // ls3.forEach((v, k) => {
        //     var ok = v['geometry'].getPath();
        //     var ok1 = e.target.getPath().getPath();
        //     if (ok != ok1) return;
        //     var tgw = new TGOS.TGInfoWindow(k, e.point, { pixelOffset: new TGOS.TGSize(0, 0) });
        //     tgw.open(pMap.value, e.point);
        //     ls2.set(k, tgw);
        // });
    }

    static _getFeatureName(layerId, f) {
        let name = f.get("name");
        if (name == null) name = f.get("NAME");
        if (name) name = name.split(';')[0];
        return name;
        let type = f.getGeometry().getType();

        let id = type == "LineString"
            ? layerId + "_" + name + "_" + i
            : layerId + "_" + f.get("id");

        return { name, id };
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