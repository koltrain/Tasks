define("GKILicSection", ["ConfigurationEnums"], function(ConfigurationEnums) {
	return {
		entitySchemaName: "GKILic",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		methods: {
			/*
			* Переопределение цета записи с остатком лицензий 0
			* @overridden
			*/
			prepareResponseCollectionItem: function(item) {
                this.callParent(arguments);
                item.customStyle = null;
                var availableCount = item.get("GKIAvailableCount");
                if (availableCount == 0) {
                    item.customStyle = {
                        "background": "#FFB100"
                    };
                }
			},
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
			 * @overridden
			 * @desc удаление "Импорт данных" в меню "Действия"
			 */
			getDataImportMenuItemVisible: Terrasoft.emptyFn,
			/**
			 * @overridden
			 * @desc удаление "Удалить" в меню "Действия"
			 */
			isVisibleDeleteAction: Terrasoft.emptyFn,
			/**
			 * @public
			 * @desc Открытие страницы продукта лицензирования (GKILicPackage)
			 */
			onGKIOpenLicProductPage: function() {
				var schemaName = "GKILicPackagePage";
				var primaryColumnValue = this.getActiveRow().get("GKILicPackage")?.value;
				this.openCard(schemaName, ConfigurationEnums.CardStateV2.EDIT, primaryColumnValue);
			}
		},
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "remove",
				"name": "SeparateModeAddRecordButton",
				"parentName": "SeparateModeActionButtonsContainer"
			},
			{ 
				"operation": "remove",
				"name": "DataGridActiveRowCopyAction",
				"parentName": "DataGrid",
			},
			{ 
				"operation": "remove",
				"name": "DataGridActiveRowDeleteAction",
				"parentName": "DataGrid",
			}
		]/**SCHEMA_DIFF*/
	};
});
