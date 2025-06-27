declare namespace TGOS {
    class TGDrawing {
        // Add methods and properties as needed
    }
    class TGMarker {
        // Add methods and properties as needed
    }
    class TGInfoWindow {
        constructor(content: string, position: any, options?: any);
        open(map: any, position?: any): void;
        close(): void;
    }
    class TGOnlineMap {
        constructor(element: HTMLElement, coordSys: any, options?: any);
        setCenter(point: TGPoint): void;
        setZoom(zoomLevel: number): void;
    }
    class TGPoint {
        constructor(x: number, y: number);
    }
    class TGSize {
        constructor(width: number, height: number);
    }
    class TGCoordSys {
        static EPSG3857: any;
    }
    class TGImage {
        constructor(url: string, size: TGSize, origin: TGPoint, anchor: TGPoint);
    }
    class TGEvent {
        static addListener(instance: any, eventName: string, handler: Function): void;
        static removeListener(instance: any, eventName: string): void;
    }
    class TGInfoWindow {
        constructor(content: string, position: TGPoint, options?: any);
        open(map: TGOnlineMap, position?: TGPoint): void;
        close(): void;
    }
    class TGLine {
        constructor(map: TGOnlineMap, path: any, options: any);
    }
    class TGLineString {
        constructor(points: TGPoint[]);
    }
    class TGData {
        constructor(options: any);
        addGeoJson(geoJson: any, options: any): any;
        forEachFeature(callback: (feature: any) => void): void;
        setMap(map: TGOnlineMap): void;
        overrideStyle(feature: any, style: any): void;
    }
    class TGMarkerCluster {
        constructor(map: TGOnlineMap, markers: TGMarker[], options: any);
        setMaxZoom(zoomLevel: number): void;
        setVisible(visible: boolean): void;
        setSearchBounds(bounds: number): void;
        redrawAll(force: boolean): void;
    }
}