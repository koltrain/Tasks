(function() {
	require.config({
		paths: {
			"RelationshipDiagramComponent": Terrasoft.getFileContentUrl("RelationshipDesigner", "src/js/relationship-diagram-component/relationship-diagram-component.js"),
			"RelationshipDesignerModule": Terrasoft.getFileContentUrl("RelationshipDesigner", "src/js/RelationshipDesignerModule.js"),
			"RelationshipDesignerComponent": Terrasoft.getFileContentUrl("RelationshipDesigner", "src/js/RelationshipDesignerComponent.js")
		},
		shim: {
			"RelationshipDiagramComponent": {
				deps: ["ng-core"]
			}
		}
	});
}());
