define("GKIHTMLControlGenerator", ["GKIHTMLControlGeneratorResources", "terrasoft", "ext-base", "GKIHTMLControl"],
		function(resources) {
	var GKIHTMLControlGenerator = Ext.define("Terrasoft.configuration.GKIHTMLControlGenerator", {
		extend: "Terrasoft.ViewGenerator",
		alternateClassName: "Terrasoft.GKIHTMLControlGenerator",
		generateGKIHTMLControl: function(config) {
			var GKIHTMLControl = {
				className: "Terrasoft.GKIHTMLControl",
				id: config.name + "GKIHTMLControl",
				selectors: {wrapEl: "#" + config.name + "GKIHTMLControl"},
				myParam: {bindTo: config.getMyParam},
				myMethod: {bindTo: config.myMethod},
				getClick: {bindTo: config.getClick},
			};
			if (!Ext.isEmpty(config.wrapClassName)) {
				GKIHTMLControl.classes = {
					wrapClassName: config.wrapClassName
				};
			}
			return GKIHTMLControl;
		}
	});
	return Ext.create(GKIHTMLControlGenerator);
});
