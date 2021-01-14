define("ClientUnitParameterPropertiesPageModule", [
	"ClientUnitSchemaModelItemDesignerResources",
	"SchemaModelItemDesignerResources",
	"EntityColumnPropertiesPageModule"
	],
	function(resources) {
	Ext.define("Terrasoft.ClientUnitParameterPropertiesPageModule", {
		extend: "Terrasoft.EntityColumnPropertiesPageModule",

		/**
		 * @inheritDoc Terrasoft.DateTimeColumnPropertiesPageModule
		 * override
		 */
		getPropertiesPageTranslation: function(viewModel) {
			const translation = this.callParent(arguments);
			translation.isRequiredLabel = resources.localizableStrings.isRequiredLabel;
			return translation;
		},

		/**
		 * @inheritDoc Terrasoft.EntityColumnPropertiesPageModule
		 * override
		 */
		getPageItemType: function() {
			return "processParameter";
		},

		/**
		 * @override
		 */
		init: function() {
			this.initResources(resources);
			this.callParent(arguments);
		}

	});
	return Terrasoft.ClientUnitParameterPropertiesPageModule;
});
