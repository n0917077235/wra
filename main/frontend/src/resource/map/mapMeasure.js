import LayerDef from "../layerDef.js";
import { apiClient } from '../index.ts';

export default class MapMeasure {
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
  continuePolygonMsg = '點選以繼續畫多邊形';

  /**
   * Message to show when the user is drawing a line.
   * @type {string}
   */
  continueLineMsg = '點選以繼續畫線段';

  draw = undefined;
  type = 'none';
  source = new ol.source.Vector();
  vector;
  layers;
  changesUnsaved;

  constructor(map, layers, changesUnsaved) {
    this.vector = new ol.layer.Vector({
      source: this.source,
    });

    this.attachMap(map);
    this.layers = layers;
    this.changesUnsaved = changesUnsaved;
  }

  attachMap(map) {
    this.map = map;
    map.on('pointermove', e => this.pointerMoveHandler(e));

    map.getViewport().addEventListener('mouseout', () => {
      let h = this.helpTooltipElement;
      if (h) h.classList.add('hidden');
    });
  }

  // 'length' || 'area' || 'none'
  setType(type) {
    this.type = type;
    this.map.getInteractions().clear();

    if (type === 'none') {
      this.hideResult();
    } else {
      this.addInteraction(type);
    }
  }

  pointerMoveHandler(evt) {
    if (this.type === 'none' || evt.dragging) return;
    let helpMsg = '點選以開始量測';

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

  style = new ol.style.Style({
    fill: new ol.style.Fill({
      color: 'rgba(255, 255, 255, 0.2)',
    }),
    stroke: new ol.style.Stroke({
      color: 'rgba(255, 0, 255, 0.5)',
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

  addInteraction(t) {
    const type = t == 'area' ? 'Polygon' : 'LineString';

    this.draw = new ol.interaction.Draw({
      source: this.source,
      type,
      style: feature => {
        const geometryType = feature.getGeometry().getType();
        if (geometryType === type || geometryType === 'Point') return this.style;
      },
    });

    this.map.addInteraction(this.draw);
    this.createMeasureTooltip();
    this.createHelpTooltip();

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

  async save() {
    let layer = this._getLayer();
    if (layer === null) return;
    let features = layer.getSource().getFeatures();
    let parser = new ol.format.GeoJSON();
    let obj = parser.writeFeaturesObject(features, { featureProjection: 'EPSG:3857' });

    let data = {
      name: 'my123',
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
