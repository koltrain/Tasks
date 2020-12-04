define("GKICompletenessIndicator", [],
	function() {
		Ext.define("Terrasoft.controls.GKICompletenessIndicator", {
			alternateClassName: "Terrasoft.GKICompletenessIndicator",
			override: "Terrasoft.CompletenessIndicator",
 
			/**
			 * Sector colors
			 * #BPMS-35
			 * @overridden
			 */
			sectorsColors: ["#a8abb9", "#71c5f0", "#006fba"],
		});
	});