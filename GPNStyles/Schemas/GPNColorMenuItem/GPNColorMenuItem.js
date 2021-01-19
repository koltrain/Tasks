define("GPNColorMenuItem", [],
	function() {
		Ext.define("Terrasoft.controls.GPNColorMenuItem", {
			alternateClassName: "Terrasoft.GPNColorMenuItem",
			override: "Terrasoft.ColorMenuItem",
 
			/**
			 * Array of colors for advanced mode.
			 * @overridden
			 * #BPMS-29
			 * @desc: заменены на цвета ГПН
			 * @type {String[]}
			 */
			colors: ["#5d607b", "#9fa7b5", "#a8abb9", "#c4c6d3",
			"#dde0e6", "#ebebeb", "#f3f4f8", "#ffffff",
			"#003d76", "#00569b", "#006fba", "#2378d8",
			"#0097d8", "#2cb4e9", "#71c5f0", "#a1daf8",
			"#d60100", "#ffb100", "#46b145", "#793dc0",
			"#03a5a5", "#bce4fa", "#d4dce5"],
		});
	});