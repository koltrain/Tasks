define("GKIInstanceLicenseDetail", ["BusinessRulesApplierV2", "GKILicensingConstantsJs", "ConfigurationGrid",
    "ConfigurationGridGenerator", "ConfigurationGridUtilities"],
    function (BusinessRulesApplierV2, constants) {
        return {
            entitySchemaName: "GKIInstanceLicense",
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
                        "generateControlsConfig": { "bindTo": "generateActiveRowControlsConfig" },
                        "changeRow": { "bindTo": "changeRow" },
                        "unSelectRow": { "bindTo": "unSelectRow" },
                        "onGridClick": { "bindTo": "onGridClick" },
                        "activeRowActions": [
                            {
                                "className": "Terrasoft.Button",
                                "style": this.Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                                "tag": "save",
                                "markerValue": "save",
                                "imageConfig": { "bindTo": "Resources.Images.SaveIcon" }
                            },
                            {
                                "className": "Terrasoft.Button",
                                "style": this.Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                                "tag": "cancel",
                                "markerValue": "cancel",
                                "imageConfig": { "bindTo": "Resources.Images.CancelIcon" }
                            },
                            {
                                "className": "Terrasoft.Button",
                                "style": this.Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                                "tag": "remove",
                                "markerValue": "remove",
                                "imageConfig": { "bindTo": "Resources.Images.RemoveIcon" },
                                "visible": { "bindTo": "IsButtonVisible" }
                            }
                        ],
                        "initActiveRowKeyMap": { "bindTo": "initActiveRowKeyMap" },
                        "activeRowAction": { "bindTo": "onActiveRowAction" },
                        "multiSelect": { "bindTo": "MultiSelect" }
                    }
                },
                {
                    "operation": "insert",
                    "parentName": "Detail",
                    "propertyName": "tools",
                    "name": "AddTypedRecordButton",
                    "values": {
                        "itemType": Terrasoft.ViewItemType.INFORMATION_BUTTON,
                        "content": { "bindTo": "Resources.Strings.InfoButton" },
                        "controlConfig": {
                            "imageConfig": {
                                "bindTo": "Resources.Images.InfoIcon"
                            }
                        }
                    }
                }
            ]/**SCHEMA_DIFF*/,
            methods: {
				/**
				 * Дополняет колонки детали
				 * @overridden
				 */
                getGridDataColumns: function () {
                    var baseGridDataColumns = this.callParent(arguments);
                    var gridDataColumns = {
                        "GKIInstance.GKIIsLimitApllied": {
                            path: "GKIInstance.GKIIsLimitApllied"
                        },
                        "GKILic.GKILicStatus": {
                            path: "GKILic.GKILicStatus"
                        },
                        "GKILic.GKICount": {
                            path: "GKILic.GKICount"
                        }
                    };
                    return this.Ext.apply(baseGridDataColumns, gridDataColumns);
                },
				/**
				 * @overridden
				 * Проверка при сохранении записи
				 */
                saveRowChanges: function (row, callback, scope) {
                    scope = scope || this;
                    callback = callback || Terrasoft.emptyFn;
                    if (row && this.getIsRowChanged(row)) {
                        if (row.get("GKILic.GKICount") < row.changedValues.GKILimit) {
                            Terrasoft.showErrorMessage(this.get("Resources.Strings.GKICountErrorMessage"), null, this);
                            row.changedValues.GKILimit = row.values.GKILimit;
                            return;
                        }
                        else if (row.changedValues.GKILimitVIP > row.values.GKILimit
                            || row.validate.GKILimitVIP > row.changedValues.GKILimit) {
                            Terrasoft.showErrorMessage(this.get("Resources.Strings.GKIVIPLimitErrorMessage"), null, this);

                            return;
                        }
                        else
                            this.saveDataRow(row, callback, scope);
                    } else {
                        callback.call(scope);
                    }
                },
				/**
				 * @overridden
				 * Генерирует конфигурацию элементов редактирования активной строки реестра.
				 * @param {String} id Идентификатор строки.
				 * @param {Object} columnsConfig Конфигурация колонок реестра.
				 * @param {Array} rowConfig Конфигурация элементов редактирования.
				 */
                generateActiveRowControlsConfig: function (id, columnsConfig, rowConfig) {
                    this._setActiveRowConfig(id);
                    this.columnsConfig = columnsConfig;
                    var gridLayoutItems = [];
                    var currentColumnIndex = 0;
                    var gridData = this.getGridData();
                    var activeRow = gridData.get(id);
                    this.activeRowData = {
                        GKIInstance_GKIIsLimitApllied: activeRow.get("GKIInstance.GKIIsLimitApllied"),
                        GKILic_GKILicStatus: activeRow.get("GKILic.GKILicStatus"),
                        GKILic_GKICount: activeRow.get("GKILic.GKICount")
                    };
                    this.Terrasoft.each(columnsConfig, function (columnConfig) {
                        var columnName = columnConfig.key[0].name.bindTo;
                        var column = this.getColumnByColumnName(columnName);
                        var cellConfig = column
                            ? this.getCellControlsConfig(column)
                            : this.getActiveRowCellConfig(columnConfig, currentColumnIndex);
                        cellConfig = this.Ext.apply({
                            layout: {
                                colSpan: columnConfig.cols,
                                column: currentColumnIndex,
                                row: 0,
                                rowSpan: 1
                            }
                        }, cellConfig);

						/*
						GKILimit - Лимит лицензии приложения 
						GKILimitVIP - Лимит VIP пользователей:
						ЕСЛИ
							Приложения платформы. Учитывать лимит = Да
							И
							Лицензии приложения (по колонке Приложение) существует
							И
								Лицензия.Статус = Активна
							ТО
							Лицензии приложения.Лимит VIP пользователей доступна для редактирования
						
						Остальные поля недоступны
						*/
                        cellConfig.enabled =
                            (columnName === "GKILimit") ||
                                (columnName === "GKILimitVIP" && this.activeRowData.GKIInstance_GKIIsLimitApllied === true
                                    && this.activeRowData.GKILic_GKILicStatus.value === constants.GKILicStatus.Active)
                                ? true
                                : false;

                        gridLayoutItems.push(cellConfig);
                        currentColumnIndex += columnConfig.cols;
                    }, this);
                    this.applyBusinessRulesForActiveRow(id, gridLayoutItems);
                    var viewGenerator = Ext.create(this.getRowViewGeneratorClassName());
                    viewGenerator.viewModelClass = this;
                    var gridLayoutConfig = viewGenerator.generateGridLayout({
                        name: this.name,
                        items: gridLayoutItems
                    });
                    rowConfig.push(gridLayoutConfig);
                },
                //region overridden методы убирающие кнопки детали
                getCopyRecordMenuItem: Terrasoft.emptyFn,

                getDeleteRecordMenuItem: Terrasoft.emptyFn,

                getAddRecordMenuItem: Terrasoft.emptyFn,

                getEditRecordMenuItem: Terrasoft.emptyFn,

                getAddRecordButtonVisible: Terrasoft.emptyFn,

                editRecord: Ext.emptyFn,

                getDataImportMenuItemVisible: Terrasoft.emptyFn
                //endregion
            }
        };
    });
