define("GKIInstanceLicPackageDetail", ["BusinessRulesApplierV2", "ConfigurationGrid", "ConfigurationGridGenerator", "ConfigurationGridUtilities"], function () {
    return {
        entitySchemaName: "GKIInstanceLicPackage",
        attributes: {},
        mixins: {
            ConfigurationGridUtilities: "Terrasoft.ConfigurationGridUtilities"
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
                    "content": {"bindTo": "Resources.Strings.InfoButton" },
					"controlConfig": {
						"imageConfig": {
							"bindTo": "Resources.Images.InfoIcon"
						}
					}
                }
            }
        ]/**SCHEMA_DIFF*/,
        messages: {
            "GetSelectedRow": {
                mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.PUBLISH
            },
            "GetSelectedRows": {
                mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.PUBLISH
            },
            "GetRow": {
                mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.PUBLISH
            },
            "GetRows": {
                mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.PUBLISH
            }
        },
        methods: {
			/**
			 * @overridden
			 * действия после выбора ряда детали
			 * @param {Object} row 
			 */
            getSelectedItems: function() {
                var row = this.callParent(arguments);
                var selectedRow = this.$ActiveRow;
                if (!this.get("MultiSelect")) {
                    if (selectedRow != null) {
                        var esq = Ext.create("Terrasoft.EntitySchemaQuery", {
                            rootSchemaName: "GKIInstanceLicPackage"
                        });
                        this.sandbox.publish("GetRow", {row: selectedRow}, ["RowInDash"]);
                        esq.addColumn("GKIInstance");
                        esq.filters.add("GKIInstanceLicPackageIdFilter", Terrasoft.createColumnFilterWithParameter("Id", selectedRow));
                        esq.getEntityCollection(function (res) {
                            if (res.success) {
                                if (res.collection.first() != null)
                                    this.sandbox.publish("GetSelectedRow", { row: res.collection.first() }, ["row"]);}
                        }, this);
                    }
                }
                else {
                    var rows = this.get("SelectedRows");
                    if (rows.length > 0) {
                        var filteredRows = [];
                        var esq = Ext.create("Terrasoft.EntitySchemaQuery", {
                            rootSchemaName: "GKIInstanceLicPackage"
                        });
                        esq.addColumn("GKIInstance");
                        esq.filters.add("GKIInstanceLicPackageIdsFilter", Terrasoft.createColumnInFilterWithParameters("Id", rows));
                        esq.getEntityCollection(function (res) {
                            if (res.success) {
                                res.collection.each(function (row) {
                                    if (row != null)
                                        filteredRows.push(row.get("GKIInstance").value);
                                });
                            }
                            this.sandbox.publish("GetSelectedRows", { rows: filteredRows }, ["rows"]);
                        }, this);
                    }
                }
                return row;
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
