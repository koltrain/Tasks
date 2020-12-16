define("GKIGaugeViewConfig", [],
	function() {
		Ext.define("Terrasoft.configuration.GKIGaugeViewConfig", {
			alternateClassName: "Terrasoft.GKIGaugeViewConfig",
			override: "Terrasoft.GaugeViewConfig",
 
			/**
			 * Generates configuration of Gauge module view.
			 * @overridden
			 * @desc sectorsColors has been changed
			 * @protected
			 * @virtual
			 * @param {Object} config Configuration object.
			 * @param {Terrasoft.BaseEntitySchema} config.entitySchema Object schema.
			 * @param {String} config.style View style.
			 * @return {Object} Returns configuration of Gauge module view.
			 */
			generate: function(config) {
				var style = config.style || "";
				var wrapClassName = Ext.String.format("{0}", style);
				var chartId = Terrasoft.Component.generateId();
				return {
					"name": "chart-wrapper-" + chartId,
					"itemType": Terrasoft.ViewItemType.CONTAINER,
					"wrapClass": ["gauge-wrapper"],
					"styles": {
						"display": "block",
						"float": "left",
						"width": "100%",
						"height": "100%",
						"background": "white"
					},
					"items": [{
						"name": "chart-wrapper-" + chartId,
						"itemType": Terrasoft.ViewItemType.CONTAINER,
						"classes": {wrapClassName: [wrapClassName, "default-widget-header"]},
						"items": [{
							"name": "caption-" + chartId,
							"labelConfig": {
								"classes": ["default-widget-header-label"],
								"labelClass": ""
							},
							"itemType": Terrasoft.ViewItemType.LABEL,
							"caption": {"bindTo": "caption"}
						}]
					}, {
						"name": "gaugeChart-" + chartId,
						"itemType": Terrasoft.ViewItemType.MODULE,
						"className": "Terrasoft.GaugeChart",
						"value": {"bindTo": "value"},
						"sectorsBounds": {"bindTo": "getSectorsBounds"},
						"sectorsColors": ["#A1DAF8", "#2CB4E9", "#006FBA"],
						"reverseSectors": {"bindTo": "getReverseSectors"}
					}]
				};
			}
		});
	});