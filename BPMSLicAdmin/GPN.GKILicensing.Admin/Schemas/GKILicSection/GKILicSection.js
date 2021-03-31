define("GKILicSection", ["ConfigurationEnums"], function(ConfigurationEnums) {
	return {
		entitySchemaName: "GKILic",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		methods: {

			/**
			* @protected
            * @desc переопределение цета записи с остатком лицензий 0
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
			 * @protected
			 * @desc opens edit page for record by primary column value
			 * @overridden
			 * @param {String} primaryColumnValue Primary column value.
			 */
			editRecord: function(primaryColumnValue) {
				this.onGKIOpenLicProductPage();
			},

			/**
			 * @protected
			 * @desc замещение кнопки Открыть
			 * @overridden
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
			 * @overridden
			 * @desc удаление "Импорт данных" в меню "Действия"
			 */
			getDataImportMenuItemVisible: Terrasoft.emptyFn,

			/**
			 * @public
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
			},

			/**
			 * @public
			 * @desc удаление кнопки вертикального реестра
			 * @overridden
			 */
			getToggleSectionButtonIsVisible: Terrasoft.emptyFn
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
