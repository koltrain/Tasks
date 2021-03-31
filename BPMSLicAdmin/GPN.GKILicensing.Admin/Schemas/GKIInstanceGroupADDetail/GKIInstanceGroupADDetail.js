define("GKIInstanceGroupADDetail", ["ConfigurationGrid", "ConfigurationGridGenerator", "ConfigurationGridUtilities", "ConfigurationEnums", "LookupMultiAddMixin"], function (configurationEnums) {
    return {
        entitySchemaName: "GKIInstanceGroupAD",
        attributes: {
			"IsEditable": {
                dataValueType: Terrasoft.DataValueType.BOOLEAN,
                type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                value: true
            }
        },
        mixins: {
            ConfigurationGridUtilities: "Terrasoft.ConfigurationGridUtilities",
            LookupMultiAddMixin: "Terrasoft.LookupMultiAddMixin"
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
                            "imageConfig": { "bindTo": "Resources.Images.RemoveIcon" }
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
                    "content": {"bindTo": "Resources.Strings.InfoButton" },
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
             * @overridden
             * @desc переопределение базового метода инициализации схемы
             */
            init: function () {
                this.callParent(arguments);
                //Инициализация миксина.
                this.mixins.LookupMultiAddMixin.init.call(this);
            },

            /**
			 * @overridden
			 * @desc переопределение базового метода отображения кнопки добавления
			 */
            getAddRecordButtonVisible: function () {
                //Отображать кнопку добавления если деталь развернута, даже если для детали не реализована страница редактирования.
                return this.getToolsVisible();
            },

            /**
			 * @overridden
			 * @desc переопределение базового метода обработчика события сохранения страницы редактирования детали.
			 */
            
            onCardSaved: function () {
                // Открывает справочное окно с множественным выбором записей.
                this.openLookupWithMultiSelect();
            },

            /**
			 * @overridden
			 * @desc Переопределение базового метода добавления записи на деталь
			 */
            addRecord: function () {
                // Открывает справочное окно с множественным выбором записей.
                this.openLookupWithMultiSelect(true);
            },

            /**
			 * @private
			 * @desc returns конфигурационный объект для справочного окна
			 */
            getMultiSelectLookupConfig: function () {
                return {
                    rootEntitySchemaName: "GKIInstanceGroupAD",
                    // Колонка корневой схемы.
                    rootColumnName: "Id",
                    rootColumnName: "GKIInstance",

                    relatedEntitySchemaName: "GKIGroupAD",
                    // Колонка связанной схемы.
                    relatedColumnName: "GKIGroupAD"
                };
            }
        }
    };
});