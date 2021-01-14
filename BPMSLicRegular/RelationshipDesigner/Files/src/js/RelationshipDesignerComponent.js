define("RelationshipDesignerComponent", ["RelationshipDiagramComponent"], function() {
	Ext.define("Terrasoft.RelationshipDesignerComponent", {
		extend: "Terrasoft.Component",

		/**
		 * @inheritdoc Terrasoft.controls.Container#tpl
		 */
		tpl: [
			"<div id=\"relationship-designer-{id}-wrap\" class=\"{wrapClassName}\">" +
				"<ts-relationship-designer id=\"relationship-designer-{id}\" class=\"relationship-designer\"></ts-relationship-designer>" +
			"</div>"
		],

		diagramData: null,

		// region Methods: Protected

		getSelectors: function() {
			return {
				wrapEl: "#relationship-designer-" + this.id + "-wrap",
				designerEl: "#relationship-designer-" + this.id
			};
		},

		/**
		 * @inheritdoc Terrasoft.Component#getTplData
		 * @override
		 */
		getTplData: function() {
			const tplData = this.callParent(arguments);
			this.selectors = this.getSelectors();
			return tplData;
		},

		onAfterRender: function() {
			this.callParent(arguments);
			this.initializeDiagramData();
		},

		onAfterReRender: function() {
			this.callParent(arguments);
			this.initializeDiagramData();
		},

		initializeDiagramData: function() {
			var designerEl = this.designerEl || {};
			designerEl = designerEl.dom;
			if (!designerEl) {
				return;
			}
			designerEl.data = this.diagramData;
		},

		// endregion

		// region Methods: Public

		setDiagramData: function(diagramData) {
			if (diagramData === this.diagramData) {
				return;
			}
			this.diagramData = diagramData;
		}

		// endregion

	});
});
