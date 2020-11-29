define("GKIColorsCustomization", [],
function() {
	function addColors() {
		var colors = ["#000000", "#999999", "#ffffff", "#dfdfdf",
		"#ef7e63", "#c73920", "#eba793", "#7f2910",
		"#8ecb60", "#589928", "#d1e9bd", "#2c6018",
		"#64b8df", "#3a8bb1", "#d4ebf6", "#286581",
		"#6483c3", "#46639f", "#d1dcf0", "#2b467e",
		"#5bc8c4", "#4ca6a3", "#bfe8e7", "#327b78",
		"#f8d162", "#d3ae46", "#fbe8b2", "#a8882e",
		"#fbad43", "#cc8930", "#cc8930", "#a16a1f",
		"#8e8eb7", "#5e5684", "#e1deed", "#3c365b"];

		var colorModule = Terrasoft.controls.ColorMenuItem;

		colorModule.colors = colors;
	}

	addColors();
	
	return {};
});