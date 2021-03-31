// Определение схемы и установка ее зависимостей от других модулей.
define("GKILicPackageDetailWithEdit", ["ConfigurationGrid", "ConfigurationGridGenerator",
    "ConfigurationGridUtilities"], function () {
        return {
            entitySchemaName: "GKILicPackageUser",
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
            diff: /**SCHEMA_DIFF*/[
                {
                    "operation": "insert",
                    "name": "GKISelectAllButton",
                    "parentName": "Detail",
                    "propertyName": "tools",
                    "values": {
                        "itemType": this.Terrasoft.ViewItemType.BUTTON,
                        "caption": {
                            "bindTo": "Resources.Strings.SelectAllCaption"
                        },
                        "click": {
                            "bindTo": "onGKISelectAllButtonClick"
                        }
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
                            "bindTo": "Resources.Strings.DeselectAllCaption"
                        },
                        "click": {
                            "bindTo": "onGKIDeselectAllButtonClick"
                        }
                    }
                },
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
                                "imageConfig": { "bindTo": "Resources.Images.RemoveIcon" }
                            }
                        ],
                        "initActiveRowKeyMap": { "bindTo": "initActiveRowKeyMap" },
                        "activeRowAction": { "bindTo": "onActiveRowAction" },
                        "multiSelect": { "bindTo": "MultiSelect" }
                    }
                }
            ]/**SCHEMA_DIFF*/,
            methods: {

                /**
                * @protected
                * @desc обработка кнопки "Выбрать все"
                */
                onGKISelectAllButtonClick: function () {
                    forEach(element in this.getDataGrid())
                    this.setCorrectBoolValue(false, element);
                },

                /**
                * @protected
                * @desc oбработка кнопки "Отозвать все"
                */
                onGKIDeselectAllButtonClick: function () {
                    forEach(element in this.getDataGrid())
                    this.setCorrectBoolValue(true, element);
                },

                /**
                * @protected
                * @desc oбработка условия изменения значения поля
                * @param {boolean, dataGridRaw}
                */
                setCorrectBoolValue: function (isCheckForActive, element) {
                    var isActive = element.get("GKIActive");

                    !isCheckForActive
                        ? !isActive ? element.set("GKIActive", true) : null
                        : isActive ? element.set("GKIActive", false) : null;
                }
            }
        };
    });
