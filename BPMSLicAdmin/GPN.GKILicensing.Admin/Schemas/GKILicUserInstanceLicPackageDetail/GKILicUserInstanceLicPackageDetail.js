define("GKILicUserInstanceLicPackageDetail", ["BusinessRulesApplierV2", "ConfigurationGrid", "ConfigurationGridGenerator", "ConfigurationGridUtilities"], 
function(BusinessRulesApplierV2) {
	return {
		entitySchemaName: "GKILicUserInstanceLicPackage",
		attributes: {
			"IsEditable": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: true
			}
		},
      	mixins: {
			ConfigurationGridUtilities: "Terrasoft.ConfigurationGridUtilities"
		},
      	details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[
          {
				"operation": "merge",
				"name": "DataGrid",
				"values": {
					"className": "Terrasoft.ConfigurationGrid",
					"generator": "ConfigurationGridGenerator.generatePartial",
					"generateControlsConfig": {"bindTo": "generateActiveRowControlsConfig"},
					"changeRow": {"bindTo": "changeRow"},
					"unSelectRow": {"bindTo": "unSelectRow"},
					"onGridClick": {"bindTo": "onGridClick"},
					"activeRowActions": [
						{
							"className": "Terrasoft.Button",
							"style": this.Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
							"tag": "save",
							"markerValue": "save",
							"imageConfig": {"bindTo": "Resources.Images.SaveIcon"}
						},
						{
							"className": "Terrasoft.Button",
							"style": this.Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
							"tag": "cancel",
							"markerValue": "cancel",
							"imageConfig": {"bindTo": "Resources.Images.CancelIcon"}
						},
						{
							"className": "Terrasoft.Button",
							"style": this.Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
							"tag": "remove",
							"markerValue": "remove",
							"imageConfig": {"bindTo": "Resources.Images.RemoveIcon"}
						}
					],
					"initActiveRowKeyMap": {"bindTo": "initActiveRowKeyMap"},
					"activeRowAction": {"bindTo": "onActiveRowAction"},
					"multiSelect": {"bindTo": "MultiSelect"}	
				}
			}
        ]/**SCHEMA_DIFF*/,
		methods: {
			/**
			 * @overridden
			 * Генерирует конфигурацию элементов редактирования активной строки реестра.
			 * @param {String} id Идентификатор строки.
			 * @param {Object} columnsConfig Конфигурация колонок реестра.
			 * @param {Array} rowConfig Конфигурация элементов редактирования.
			 */
			generateActiveRowControlsConfig: function(id, columnsConfig, rowConfig) {
				this.columnsConfig = columnsConfig;
				var gridLayoutItems = [];
				var currentColumnIndex = 0;
				this.Terrasoft.each(columnsConfig, function(columnConfig) {
					var columnName = columnConfig.key[0].name.bindTo;
					var column = this.getColumnByColumnName(columnName);
					var cellConfig = this.getCellControlsConfig(column);
					cellConfig = this.Ext.apply({
						layout: {
							colSpan: columnConfig.cols,
							column: currentColumnIndex,
							row: 0,
							rowSpan: 1
						}
					}, cellConfig);
					
					//Блокировка полей записи детали
					cellConfig.enabled = columnName === "GKILicPackage" || columnName === "GKISyncedState" ||
						columnName === "GKILastSyncDateTime" ? false : true;

					gridLayoutItems.push(cellConfig);
					currentColumnIndex += columnConfig.cols;
				}, this);
				var gridData = this.getGridData();
				var activeRow = gridData.get(id);
				var rowClass = {prototype: activeRow};
				BusinessRulesApplierV2.applyRules(rowClass, gridLayoutItems);
				var viewGenerator = this.Ext.create("Terrasoft.ViewGenerator");
				viewGenerator.viewModelClass = {prototype: this};
				var gridLayoutConfig = viewGenerator.generateGridLayout({
					name: this.name,
					items: gridLayoutItems
				});
				rowConfig.push(gridLayoutConfig);
			}
		}
	};
});
