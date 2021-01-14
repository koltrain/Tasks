(function() {
	require.config({
		paths: {
			"page-wizard-component": Terrasoft.getFileContentUrl("DesignerTools", "src/js/page-wizard-component.js"),
			"BasePropertiesPageModule": Terrasoft.getFileContentUrl("DesignerTools", "src/js/BasePropertiesPageModule.js"),
			"EntityColumnPropertiesPageModule": Terrasoft.getFileContentUrl("DesignerTools", "src/js/EntityColumnPropertiesPageModule.js"),
			"ClientUnitParameterPropertiesPageModule": Terrasoft.getFileContentUrl("DesignerTools", "src/js/ClientUnitParameterPropertiesPageModule.js"),
			"ControlGroupPropertiesPageModule": Terrasoft.getFileContentUrl("DesignerTools", "src/js/ControlGroupPropertiesPageModule.js"),
			"DetailPropertiesPageModule": Terrasoft.getFileContentUrl("DesignerTools", "src/js/DetailPropertiesPageModule.js")
		},
		shim: {
			"page-wizard-component": {
				deps: ["ng-core"]
			}
		}
	});
}());
