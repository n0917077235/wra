import LayerDef from "../layerDef.js";
import { apiClient } from '../index.ts';

export default class MapDrawing {
  map;

  /**
   * Currently drawn feature.
   * @type {import("../src/ol/Feature.js").default}
   */
  sketch;

  /**
   * The help tooltip element.
   * @type {HTMLElement}
   */
  helpTooltipElement;

  /**
   * Overlay to show the help messages.
   * @type {Overlay}
   */
  helpTooltip;

  /**
   * The measure tooltip element.
   * @type {HTMLElement}
   */
  measureTooltipElement;

  /**
   * Overlay to show the measurement.
   * @type {Overlay}
   */
  measureTooltip;

  /**
   * Message to show when the user is drawing a polygon.
   * @type {string}
   */
  continuePolygonMsg = '點兩下完成繪圖';

  /**
   * Message to show when the user is drawing a line.
   * @type {string}
   */
  continueLineMsg = '點兩下完成繪圖';

  draw = undefined;
  type = 'none';
  source = new ol.source.Vector();
  vector;
  layers;
  changesUnsaved;
  fileMode;
  editing;
  _highlightedFeature = null;

  static highlightKey = 'highlight';

  constructor(map, layers, changesUnsaved, fileMode, editing) {
    this.vector = new ol.layer.Vector({
      source: this.source,
    });

    this._attachMap(map);
    this.layers = layers;
    this.changesUnsaved = changesUnsaved;
    this.fileMode = fileMode;
    this.editing = editing;
  }

  _attachMap(map) {
    this.map = map;
    map.on('pointermove', e => this.pointerMoveHandler(e));

    map.getViewport().addEventListener('mouseout', () => {
      let h = this.helpTooltipElement;
      if (h) h.classList.add('hidden');
    });

    this._enableAddMarkers();
    this._enableDeletion();
  }

  deleteSelected() {
    if (this._highlightedFeature === null) return;
    let layer = this._getLayer();
    if (layer === null) return;
    layer.getSource().removeFeature(this._highlightedFeature);
    this._highlightedFeature = null;
    this.changesUnsaved.value = true;
    this.editing.value = false;
  }

  _enableDeletion() {
    let map = this.map;

    map.on('singleclick', event => {
      this._resetHighlights();
      if (this.type !== 'none') return;
      let drawingLayer = this._getLayer();
      let layerFilter = layer => ol.util.getUid(layer) === ol.util.getUid(drawingLayer);
      let first = null;

      map.forEachFeatureAtPixel(event.pixel, f => {
        if (first !== null) return;
        first = f;
      }, { layerFilter });

      if (first === null) return;

      // highlight
      first.set(MapDrawing.highlightKey, '1');
      this._highlightedFeature = first;
      this.editing.value = true;
    });
  }

  _resetHighlights() {
    this._highlightedFeature?.set(MapDrawing.highlightKey, '0');
    this._highlightedFeature = null;
    this.editing.value = false;
  }

  _enableAddMarkers() {
    this.map.on('singleclick', evt => {
      if (this.type !== 'marker' && this.type !== 'text') return;
      let coordinate = evt.coordinate;
      let marker = new ol.Feature(new ol.geom.Point(coordinate));
      marker.set('pointType', 'marker');

      if (this.type === 'text') {
        let text = prompt('請輸入標記文字');
        if (text === null) return;
        marker.set('text', text);
      }

      let layer = this._getLayer();
      layer?.getSource().addFeature(marker);
    });
  }

  // type: 'length' | 'area' | 'marker' | 'text' |'none'
  setType(type) {
    this.type = type;
    this.sketch = null;
    this._removeCurrentInteraction();

    if (type === 'none') {
      this.hideResult();
    } else {
      this.addInteraction(type);
    }
  }

  _removeCurrentInteraction() {
    if (!this.draw) return;
    let map = this.map;

    map.getInteractions().forEach(x => {
      if (ol.util.getUid(x) === ol.util.getUid(this.draw)) map.removeInteraction(x);
    });
  }

  pointerMoveHandler(evt) {
    if (this.type === 'none' || evt.dragging) return;
    let helpMsg = '點選以開始標記';

    if (this.sketch) {
      const geom = this.sketch.getGeometry();
      if (geom instanceof ol.geom.Polygon) {
        helpMsg = this.continuePolygonMsg;
      } else if (geom instanceof ol.geom.LineString) {
        helpMsg = this.continueLineMsg;
      }
    }

    this.helpTooltipElement.innerHTML = helpMsg;
    this.helpTooltip.setPosition(evt.coordinate);
    this.helpTooltipElement.classList.remove('hidden');
  }

  /**
   * Format length output.
   * @param {LineString} line The line.
   * @return {string} The formatted length.
   */
  formatLength(line) {
    const length = ol.sphere.getLength(line);
    let output;
    if (length > 100) {
      output = Math.round((length / 1000) * 100) / 100 + ' ' + 'km';
    } else {
      output = Math.round(length * 100) / 100 + ' ' + 'm';
    }

    return output;
  };

  /**
   * Format area output.
   * @param {Polygon} polygon The polygon.
   * @return {string} Formatted area.
   */
  formatArea(polygon) {
    const area = ol.sphere.getArea(polygon);
    let output;
    if (area > 10000) {
      output = Math.round((area / 1000000) * 100) / 100 + ' ' + 'km<sup>2</sup>';
    } else {
      output = Math.round(area * 100) / 100 + ' ' + 'm<sup>2</sup>';
    }
    return output;
  };

  static styles = MapDrawing._getStyles(false);
  static _highlightedStyles = MapDrawing._getStyles(true);

  static _getStyles(highlight) {
    return new ol.style.Style({
      fill: new ol.style.Fill({
        color: 'rgba(255, 255, 255, 0.7)',
      }),
      stroke: new ol.style.Stroke({
        color: highlight ? 'red' : 'rgba(255, 0, 255, 0.5)',
        width: 4,
      }),
      image: new ol.style.Circle({
        radius: 5,
        stroke: new ol.style.Stroke({
          color: 'rgba(0, 0, 0, 0.7)',
        }),
        fill: new ol.style.Fill({
          color: 'rgba(255, 255, 255, 0.2)',
        }),
      }),
    });
  }

  static markerStyle(text, highlight) {
    let f = highlight ? 'pin.png' : 'pin_magenta.png';
    let fontSize = highlight ? '22px' : '18px';

    return new ol.style.Style({
      image: new ol.style.Icon({
        scale: highlight ? 0.85 : 0.65,
        anchor: [0.5, 1],
        src: require(`@/assets/image/map/${f}`),
      }),
      text: new ol.style.Text({
        font: `${fontSize} Calibri,sans-serif`,
        fill: new ol.style.Fill({ color: '#000' }),
        stroke: new ol.style.Stroke({
          color: '#fff', width: 6
        }),
        text
      }),
    });
  }

  static styleFunc(feature) {
    let highlight = feature.get(MapDrawing.highlightKey) === '1';

    if (feature.get('pointType') === 'marker') {
      let text = feature.get('text');
      return MapDrawing.markerStyle(text, highlight);
    }

    return highlight ? MapDrawing._highlightedStyles : MapDrawing.styles;
  }

  typeMap = {
    area: 'Polygon',
    length: 'LineString',
    marker: 'Point',
    text: 'Point',
  };

  addInteraction(t) {
    const type = this.typeMap[t];

    this.draw = new ol.interaction.Draw({
      source: this.source,
      type,
      style: MapDrawing.styles,
    });

    this.map.addInteraction(this.draw);
    this.createHelpTooltip();
    this.createMeasureTooltip();

    let listener;

    this.draw.on('drawstart', evt => {
      // set sketch
      this.sketch = evt.feature;
      let tooltipCoord = evt.coordinate;

      listener = this.sketch.getGeometry().on('change', e => {
        const geom = e.target;
        let output;
        if (geom instanceof ol.geom.Polygon) {
          output = this.formatArea(geom);
          tooltipCoord = geom.getInteriorPoint().getCoordinates();
        } else if (geom instanceof ol.geom.LineString) {
          output = this.formatLength(geom);
          tooltipCoord = geom.getLastCoordinate();
        }

        this.measureTooltipElement.innerHTML = output;
        this.measureTooltip.setPosition(tooltipCoord);
      });
    });

    this.draw.on('drawend', e => {
      this.measureTooltipElement.className = 'ol-tooltip ol-tooltip-static';
      this.measureTooltip.setOffset([0, -7]);

      // unset sketch
      this.sketch = null;

      // unset tooltip so that a new one can be created
      this.hideResult();
      this.measureTooltipElement = null;
      this.createMeasureTooltip();
      ol.Observable.unByKey(listener);

      this.addToLayer(e.feature);
    });
  }

  _getLayer() {
    let allLayers = this.layers.get(LayerDef.DRAWING);
    if (allLayers.length === 0) return null;
    return allLayers[0];
  }

  addToLayer(feature) {
    let layer = this._getLayer();
    if (layer === null) return;
    layer.getSource().addFeature(feature);
    this.changesUnsaved.value = true;
  }

  clear() {
    let layer = this._getLayer();
    layer?.getSource().clear();
  }

  async load(name) {
    let layer = this._getLayer();
    if (layer === null) return;
    let source = layer.getSource();
    source.clear();
    let n = encodeURIComponent(name);
    let res = await apiClient.get(`/GeoJson/GetDrawing?name=${n}`);

    let features = new ol.format.GeoJSON().readFeatures(res.data, {
      dataProjection: 'EPSG:4326',
      featureProjection: 'EPSG:3857'
    });

    source.addFeatures(features);
  }

  async save(name) {
    let layer = this._getLayer();
    if (layer === null) return;
    let features = layer.getSource().getFeatures();
    let parser = new ol.format.GeoJSON();
    let obj = parser.writeFeaturesObject(features, { featureProjection: 'EPSG:3857' });

    let data = {
      name,
      geojson: JSON.stringify(obj),
    };

    await apiClient.put(`/GeoJson/SetDrawing`, data);
  }

  hideResult() {
    let m = this.measureTooltipElement;
    if (m) m.classList.add('hidden');
  }

  createHelpTooltip() {
    let h = this.helpTooltipElement;
    if (h) h.parentNode.removeChild(h);
    this.helpTooltipElement = document.createElement('div');
    this.helpTooltipElement.className = 'ol-tooltip hidden';
    this.helpTooltip = new ol.Overlay({
      element: this.helpTooltipElement,
      offset: [15, 0],
      positioning: 'center-left',
    });

    this.map.addOverlay(this.helpTooltip);
  }

  createMeasureTooltip() {
    let m = this.measureTooltipElement;
    if (m) m.parentNode.removeChild(m);
    this.measureTooltipElement = document.createElement('div');
    this.measureTooltipElement.className = 'ol-tooltip ol-tooltip-measure';
    this.measureTooltip = new ol.Overlay({
      element: this.measureTooltipElement,
      offset: [0, -15],
      positioning: 'bottom-center',
      stopEvent: false,
      insertFirst: false,
    });

    this.map.addOverlay(this.measureTooltip);
  }

}
