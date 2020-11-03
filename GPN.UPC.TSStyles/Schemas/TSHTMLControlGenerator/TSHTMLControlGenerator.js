define("TSHTMLControlGenerator", ["TSHTMLControlGeneratorResources", "terrasoft", "ext-base", "TSHTMLControl"],
		function(resources) {
	var TSHTMLControlGenerator = Ext.define("Terrasoft.configuration.TSHTMLControlGenerator", {
		extend: "Terrasoft.ViewGenerator",
		alternateClassName: "Terrasoft.TSHTMLControlGenerator",
		generateTSHTMLControl: function(config) {
			var TSHTMLControl = {
				className: "Terrasoft.TSHTMLControl",
				id: config.name + "TSHTMLControl",
				selectors: {wrapEl: "#" + config.name + "TSHTMLControl"},
				myParam: {bindTo: config.getMyParam},
				myMethod: {bindTo: config.myMethod},
				getClick: {bindTo: config.getClick},
			};
			if (!Ext.isEmpty(config.wrapClassName)) {
				TSHTMLControl.classes = {
					wrapClassName: config.wrapClassName
				};
			}
			return TSHTMLControl;
		}
	});
	return Ext.create(TSHTMLControlGenerator);
});
