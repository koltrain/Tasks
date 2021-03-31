define("GKILicUserInstanceLicPackageDetail", ["BusinessRulesApplierV2","GKILicensingConstantsJs", "ConfigurationGrid", "ConfigurationGridGenerator", "ConfigurationGridUtilities"], 
function(BusinessRulesApplierV2,constants) {
	return {
		entitySchemaName: "GKILicUserInstanceLicPackage",
		attributes: {
			"IsEditable": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: true
			},
			"IsPackagePage": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: false
			},
			"IsUserPage": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: false
			}
		},
      	mixins: {
			ConfigurationGridUtilities: "Terrasoft.ConfigurationGridUtilities"
		},
		messages: {
			"GetSelectedRow": {
                mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.SUBSCRIBE
            },
            "GetSelectedRows": {
                mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.SUBSCRIBE
            },
			"GetSelectRow":{
				mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.SUBSCRIBE
			},
			"GetSelectRows":{
				mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.SUBSCRIBE
			}
		},
      	details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[
			{
                "operation": "insert",
				"parentName": "Detail",
				"propertyName": "tools",
                "name": "AddTypedRecordButton",
                "values": {
					"itemType": Terrasoft.ViewItemType.INFORMATION_BUTTON,
                    "content": {"bindTo": "Resources.Strings.GKIInfoButtonHint" },
					"controlConfig": {
						"imageConfig": {
							"bindTo": "Resources.Images.InfoIcon"
						},
						"visible": {"bindTo": "IsPackagePage"}
					}
                }
            },
			{
                "operation": "insert",
				"parentName": "Detail",
				"propertyName": "tools",
                "name": "ProcessButton",
                "values": {
					"itemType": Terrasoft.ViewItemType.INFORMATION_BUTTON,
                    "content": {"bindTo": "Resources.Strings.InfoButton" },
					"controlConfig": {
						"imageConfig": {
							"bindTo": "Resources.Images.InfoIcon"
						},
						"visible": {"bindTo": "IsUserPage"}
					}
                }
			},
			{
				"operation": "insert",
				"name": "GKISelectAllButton",
				"parentName": "Detail",
				"propertyName": "tools",
				"values": {
					"itemType": this.Terrasoft.ViewItemType.BUTTON,
					"caption": {
						"bindTo": "Resources.Strings.GKISelectAllCaption"
					},
					"click": {
						"bindTo": "onGKISelectAllButtonClick"
					},
					"visible": {"bindTo": "IsUserPage"}
				}
			},
			{
				"operation": "insert",
				"name": "GKIDeselectAllButton",
				"parentName": "Detail",
				"propertyName": "tools",
				"values": {
					"itemType": this.Terrasoft.ViewItemType.BUTTON,
					"caption": {
						"bindTo": "Resources.Strings.GKIDeselectAllCaption"
					},
					"click": {
						"bindTo": "onGKIDeselectAllButtonClick"
					},
					"visible": {"bindTo": "IsUserPage"}
				}
			},
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
			 * @overriden
			 * действия на инциализации детали
			 */
			init: function () {
				this.callParent(arguments);

				this.sandbox.subscribe("GetSelectedRow", function (args) {
					var filterGroup = Terrasoft.createFilterGroup();

					filterGroup.add("GKIInstanceIdFilter", Terrasoft.createColumnFilterWithParameter(
						Terrasoft.ComparisonType.EQUAL, "GKIInstance", args.row.values.GKIInstance.value
					));

					this.setFiltersAndReload(filterGroup);
				}, this, ["row"]);

				this.sandbox.subscribe("GetSelectRow", function (args) {
					var filterGroup = Terrasoft.createFilterGroup();

					filterGroup.add("GKIInstanceIdsFilter", Terrasoft.createColumnFilterWithParameter(
						Terrasoft.ComparisonType.EQUAL, "GKIInstance", args.row.values.GKIInstance.value
					));
					filterGroup.add("GKILicUserFilter", Terrasoft.createColumnFilterWithParameter(
                        Terrasoft.ComparisonType.EQUAL, "GKILicUser", this.get("Id")
					));

					filterGroup.add("GKILicTypeFilter", Terrasoft.createColumnFilterWithParameter(
                        Terrasoft.ComparisonType.EQUAL, "[GKILic:GKILicPackage:GKILicPackage].GKILicType", constants.GKILicType.Personal
					));
					filterGroup.add("GKILicStatusFilter", Terrasoft.createColumnFilterWithParameter(
                        Terrasoft.ComparisonType.EQUAL, "[GKILic:GKILicPackage:GKILicPackage].GKILicStatus", constants.GKILicStatus.Active
                    ));
					this.setFiltersAndReload(filterGroup);
				}, this, ["rowsWithFilter"]);

				this.sandbox.subscribe("GetSelectRows", function (args) {
					var filterGroup = Terrasoft.createFilterGroup();

					filterGroup.add("GKIInstanceIdsFIlter", Terrasoft.createColumnInFilterWithParameters("GKIInstance", args.rows));
					
					filterGroup.add("GKILicUserFilter", Terrasoft.createColumnFilterWithParameter(
                        Terrasoft.ComparisonType.EQUAL, "GKILicUser", this.get("Id")
					));

					filterGroup.add("GKILicTypeFilter", Terrasoft.createColumnFilterWithParameter(
                        Terrasoft.ComparisonType.EQUAL, "[GKILic:GKILicPackage:GKILicPackage].GKILicType", constants.GKILicType.Personal
					));
					filterGroup.add("GKILicStatusFilter", Terrasoft.createColumnFilterWithParameter(
                        Terrasoft.ComparisonType.EQUAL, "[GKILic:GKILicPackage:GKILicPackage].GKILicStatus", constants.GKILicStatus.Active
                    ));

					this.setFiltersAndReload(filterGroup);
				}, this, ["rowsWithFilter"]);

				this.sandbox.subscribe("GetSelectedRows", function (args) {
					var filterGroup = Terrasoft.createFilterGroup();

					filterGroup.add("GKIInstanceIdsFIlter", Terrasoft.createColumnInFilterWithParameters("GKIInstance", args.rows));

					this.setFiltersAndReload(filterGroup);
				}, this, ["rows"]);
				this.getPage();
			},

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
			},

			/**
			 * @public
			 * @desc Обновление детали и добавление фильтров
			 * @param {Object} filterGroup 
			 */
			setFiltersAndReload: function (filterGroup) {
				this.set("DetailFilters", filterGroup);

				this.updateDetail({ "reloadAll": true });
			},

			/**
			* @desc показывает кнопки на детали в зависимости от ссылки страницы
			* @public
			*/
			getPage: function() {
				var pageName = this.get("CardPageName");
				if(pageName == 'GKILicUserPage') {
					this.set("IsUserPage", true)
				}
				if(pageName == 'GKILicPackagePage') {
					this.set("IsPackagePage", true)
				}
			},

			/**
			* @protected
			* @desc обработка кнопки "Выбрать все"
			*/
			   onGKISelectAllButtonClick: function () {
				Terrasoft.each(
					this.getGridData(),
					function(item){
						this.setCorrectBoolValue(true, item);
						item.save();
					}, this);
			},

			/**
			* @protected
			* @desc oбработка кнопки "Отозвать все"
			*/
			onGKIDeselectAllButtonClick: function () {
				Terrasoft.each(
					this.getGridData(),
					function(item){
						this.setCorrectBoolValue(false, item);
						item.save();
					}, this);
			},

			/**
			* @protected
			* @desc oбработка условия изменения значения поля
			* @param {boolean, dataGridRow}
			*/
			setCorrectBoolValue: function (isCheckForActive, element) {
				var isActive = element.get("GKIActive");
				if (isCheckForActive !== isActive)
					element.set("GKIActive", isCheckForActive);
			}
		}
	};
});
