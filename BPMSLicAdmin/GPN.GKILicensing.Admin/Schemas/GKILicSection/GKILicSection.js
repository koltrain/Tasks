define("GKILicSection", ["ConfigurationEnums"], function(ConfigurationEnums) {
	return {
		entitySchemaName: "GKILic",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[]/**SCHEMA_DIFF*/,
		methods: {
			/**
			 * Opens edit page for record by primary column value.
			 * @overridden
			 * @protected
			 * @param {String} primaryColumnValue Primary column value.
			 */
			editRecord: function(primaryColumnValue) {
				this.onGKIOpenLicProductPage();
			},
			/**
			 * @overridden
			 * @desc замещение кнопки Открыть 
			 */
			onActiveRowAction: function(buttonTag, primaryColumnValue) {
				switch (buttonTag) {
					case "edit":
						this.onGKIOpenLicProductPage();
						break;
					default:
						this.callParent(arguments);
						break;
				}
			},

			/**
			 * @public
			 * @desc Открытие страницы продукта лицензирования (GKILicPackage)
			 */
			onGKIOpenLicProductPage: function() {
				var schemaName = "GKILicPackagePage";
				var primaryColumnValue = this.getActiveRow().get("GKILicPackage")?.value;
				this.openCard(schemaName, ConfigurationEnums.CardStateV2.EDIT, primaryColumnValue);
			}
		}
	};
});
