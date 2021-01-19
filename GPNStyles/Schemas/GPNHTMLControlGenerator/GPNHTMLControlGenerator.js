define("GPNHTMLControlGenerator", ["GPNHTMLControlGeneratorResources", "terrasoft", "ext-base", "GPNHTMLControl"],
		function(resources) {
	var GPNHTMLControlGenerator = Ext.define("Terrasoft.configuration.GPNHTMLControlGenerator", {
		extend: "Terrasoft.ViewGenerator",
		alternateClassName: "Terrasoft.GPNHTMLControlGenerator",
		generateGPNHTMLControl: function(config) {
			var GPNHTMLControl = {
				className: "Terrasoft.GPNHTMLControl",
				id: config.name + "GPNHTMLControl",
				selectors: {wrapEl: "#" + config.name + "GPNHTMLControl"},
				myParam: {bindTo: config.getMyParam},
				myMethod: {bindTo: config.myMethod},
				getClick: {bindTo: config.getClick},
			};
			if (!Ext.isEmpty(config.wrapClassName)) {
				GPNHTMLControl.classes = {
					wrapClassName: config.wrapClassName
				};
			}
			return GPNHTMLControl;
		}
	});
	return Ext.create(GPNHTMLControlGenerator);
});
