define("GKIBaseProgressBarIndicator", [],
	function() {
		Ext.define("Terrasoft.controls.GKIBaseProgressBarIndicator", {
			alternateClassName: "Terrasoft.GKIBaseProgressBarIndicator",
			override: "Terrasoft.BaseProgressBarIndicator",
 
				/**
				 * Indicator color.
				 * @overridden
				 * @protected
				 * @type {String}
				 * @desc: добавлены цвета ГПН
				 */
				progressColor: "#006fba",

				/**
				 * Indicator used colors.
				 * @overridden
				 * @protected
				 * @type {String[]}
				 * @desc: добавлены цвета ГПН
				 */
				sectorsColors: ["#a8abb9", "#71c5f0", "#006fba"],
		});
	});