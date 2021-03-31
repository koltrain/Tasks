define("GKIInstanceLicUserDetail", [], function() {
	return {
		entitySchemaName: "GKIInstanceLicUser",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
        attributes: {
			"IsUserPage": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: false
			}
		},
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
                        "visible": {"bindTo": "IsUserPage"}
                    }
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
				this.getPage();
			},
			//region overridden методы убирающие кнопки детали
				getCopyRecordMenuItem: Terrasoft.emptyFn,

				getDeleteRecordMenuItem: Terrasoft.emptyFn,

				getAddRecordMenuItem: Terrasoft.emptyFn,

				getEditRecordMenuItem: Terrasoft.emptyFn,

				getAddRecordButtonVisible: Terrasoft.emptyFn,
				
				editRecord: Ext.emptyFn,

				getDataImportMenuItemVisible: Terrasoft.emptyFn,
			//endregion
			
			/**
			 * @overridden
			 * При выборе записи, отправляется сообщение на деталь, для применения фильтра
			 * @param {*} row Выбранная запись
			 */
			rowSelected: function (row) {
				if (!this.get("MultiSelect")) {
                    if (row != null) {
                        var esq = Ext.create("Terrasoft.EntitySchemaQuery", {
                            rootSchemaName: "GKIInstanceLicUser"
                        });

                        esq.addColumn("GKIInstance");
						esq.addColumn("GKILicUser");

                        esq.filters.add("GKIInstanceLicPackageIdFilter", Terrasoft.createColumnFilterWithParameter("Id", row));
                       

                        esq.getEntityCollection(function (res) {
                            if (res.success) {
                                if (res.collection.first() != null)
                                    this.sandbox.publish("GetSelectRow", { row: res.collection.first() }, ["rowsWithFilter"]);
                            }
                        }, this);
                    }
				}
                else {
                    var rows = this.get("SelectedRows");
                    rows.push(row);

                    if (rows.length > 0) {
                        var filteredRows = [];

                        var esq = Ext.create("Terrasoft.EntitySchemaQuery", {
                            rootSchemaName: "GKIInstanceLicUser"
                        });

                        esq.addColumn("GKIInstance");

                        esq.filters.add("GKIInstanceLicPackageIdFilter", Terrasoft.createColumnInFilterWithParameters("Id", rows));
                        

                        esq.getEntityCollection(function (res) {
                            if (res.success) {
                                res.collection.each(function (row) {
                                    if (row != null)
                                        filteredRows.push(row.get("GKIInstance").value);
                                });
                            }

                            this.sandbox.publish("GetSelectRows", { rows: filteredRows }, ["rowsWithFilter"]);
                        }, this);
                    }
                }
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
			}
		},
		messages: {
            "GetSelectRow": {
                mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.PUBLISH
            },
            "GetSelectRows": {
                mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.PUBLISH
            }
        }
	};
});
