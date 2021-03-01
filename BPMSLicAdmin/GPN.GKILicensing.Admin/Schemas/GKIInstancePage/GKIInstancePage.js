define("GKIInstancePage", [], function() {
	return {
		entitySchemaName: "GKIInstance",
		attributes: {},
		modules: /**SCHEMA_MODULES*/{
			"Indicator81ddd525-e83e-4dff-bf03-a9adba819e86": {
				"moduleId": "Indicator81ddd525-e83e-4dff-bf03-a9adba819e86",
				"moduleName": "CardWidgetModule",
				"config": {
					"parameters": {
						"viewModelConfig": {
							"widgetKey": "Indicator81ddd525-e83e-4dff-bf03-a9adba819e86",
							"recordId": "971c0d63-3e46-4edd-98aa-4e29688416b4",
							"primaryColumnValue": {
								"getValueMethod": "getPrimaryColumnValue"
							}
						}
					}
				}
			},
			"Indicatorb1d303da-be3c-49ed-843b-c9939571e083": {
				"moduleId": "Indicatorb1d303da-be3c-49ed-843b-c9939571e083",
				"moduleName": "CardWidgetModule",
				"config": {
					"parameters": {
						"viewModelConfig": {
							"widgetKey": "Indicatorb1d303da-be3c-49ed-843b-c9939571e083",
							"recordId": "971c0d63-3e46-4edd-98aa-4e29688416b4",
							"primaryColumnValue": {
								"getValueMethod": "getPrimaryColumnValue"
							}
						}
					}
				}
			},
			"Indicator5b260bc0-9bb0-4a51-8cad-7cd0c7a790d8": {
				"moduleId": "Indicator5b260bc0-9bb0-4a51-8cad-7cd0c7a790d8",
				"moduleName": "CardWidgetModule",
				"config": {
					"parameters": {
						"viewModelConfig": {
							"widgetKey": "Indicator5b260bc0-9bb0-4a51-8cad-7cd0c7a790d8",
							"recordId": "971c0d63-3e46-4edd-98aa-4e29688416b4",
							"primaryColumnValue": {
								"getValueMethod": "getPrimaryColumnValue"
							}
						}
					}
				}
			}
		}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{
			"Files": {
				"schemaName": "FileDetailV2",
				"entitySchemaName": "GKIInstanceFile",
				"filter": {
					"masterColumn": "Id",
					"detailColumn": "GKIInstance"
				}
			},
			"GKIInstanceLicUserDetail": {
				"schemaName": "GKIInstanceLicUserDetail",
				"entitySchemaName": "GKIInstanceLicUser",
				"filter": {
					"detailColumn": "GKIInstance",
					"masterColumn": "Id"
				}
			},
			"GKIInstanceLicenseDetail": {
				"schemaName": "GKIInstanceLicenseDetail",
				"entitySchemaName": "GKIInstanceLicense",
				"filter": {
					"detailColumn": "GKIInstance",
					"masterColumn": "Id"
				}
			},
			"GKIInstanceGroupADDetail": {
				"schemaName": "GKIInstanceGroupADDetail",
				"entitySchemaName": "GKIInstanceGroupAD",
				"filter": {
					"detailColumn": "GKIInstance",
					"masterColumn": "Id"
				}
			},
			"GKIGroupADUsersDetail": {
				"schemaName": "GKIGroupADUsersDetail",
				"entitySchemaName": "GKIGroupADUsers",
				"filter": {
					"detailColumn": "GKIInstance",
					"masterColumn": "Id"
				}
			},
			"GKIGroupADInstanceLicenseDetail": {
				"schemaName": "GKIGroupADInstanceLicenseDetail",
				"entitySchemaName": "GKIGroupADInstanceLicense",
				"filter": {
					"detailColumn": "GKIInstance",
					"masterColumn": "Id"
				}
			}
		}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods: {
			/**
			 * @overridden
			 */
			onEntityInitialized: function() {
				this.callParent(arguments);
				if (!this.get("GKICustomerID")){
					this.GKIsetCustomerID();
				}
			},
			/**
			 * @public
			 * @desc редактируемость поля Идентификатор клиента
			 */
			isGKICustomerIDEnabledMethod: function() {
				if (!this.get("GKICustomerID")){
					return true;
				}
				return false;
			},
			/**
			 * @private
			 * @desc устанавливает CustomerID в поле GKICustomerID, если существует только один CustomerID
			 */
			GKIsetCustomerID: function() {
				var esq = this.Ext.create("Terrasoft.EntitySchemaQuery", {
					rootSchemaName: "GKICustomerID"
				});
				esq.addColumn("Id");
				esq.addColumn("Name");
				esq.getEntityCollection(function(response) {
					if (response.success && response.collection.getItems().length === 1 
						&& response.collection.getItems()[0]) {
							this.set("GKICustomerID", {
								value: response.collection.getItems()[0].get("Id"),
								displayValue: response.collection.getItems()[0].get("Name")
							});
					}
				}, this);
			}
		},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "Indicator81ddd525-e83e-4dff-bf03-a9adba819e86",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 4,
						"column": 0,
						"row": 0,
						"layoutName": "ProfileContainer",
						"useFixedColumnHeight": true
					},
					"itemType": 4,
					"classes": {
						"wrapClassName": [
							"card-widget-grid-layout-item"
						]
					}
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "Indicatorb1d303da-be3c-49ed-843b-c9939571e083",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 4,
						"column": 0,
						"row": 4,
						"layoutName": "ProfileContainer",
						"useFixedColumnHeight": true
					},
					"itemType": 4,
					"classes": {
						"wrapClassName": [
							"card-widget-grid-layout-item"
						]
					}
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Indicator5b260bc0-9bb0-4a51-8cad-7cd0c7a790d8",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 4,
						"column": 0,
						"row": 8,
						"layoutName": "ProfileContainer",
						"useFixedColumnHeight": true
					},
					"itemType": 4,
					"classes": {
						"wrapClassName": [
							"card-widget-grid-layout-item"
						]
					}
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "GKIName",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 0,
						"layoutName": "Header"
					},
					"bindTo": "GKIName"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIUrl",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 1,
						"layoutName": "Header"
					},
					"bindTo": "GKIUrl"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "GKIVersion",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 2,
						"layoutName": "Header"
					},
					"bindTo": "GKIVersion",
					"enabled": true
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "GKIApplicationStatus",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 2,
						"layoutName": "Header"
					},
					"bindTo": "GKIApplicationStatus",
					"enabled": false,
					"contentType": 3
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "GKICustomerID",
				"values": {
                  	"hint" : {"bindTo": "Resources.Strings.CustomerIDHint"},
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 3,
						"layoutName": "Header"
					},
					"bindTo": "GKICustomerID",
					"enabled": {
						"bindTo": "isGKICustomerIDEnabledMethod"
					}
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "GKIInstanceUnit",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 3,
						"layoutName": "Header"
					},
					"bindTo": "GKIInstanceUnit"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "insert",
				"name": "GKIIsLimitApllied",
				"values": {
                  	"hint" : {"bindTo": "Resources.Strings.IsLimitAplliedHint"},
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 4,
						"layoutName": "Header"
					},
					"bindTo": "GKIIsLimitApllied"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 6
			},
			{
				"operation": "insert",
				"name": "GeneralInfoTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.GeneralInfoTabCaption"
					},
					"items": [],
					"order": 0
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIInstanceLicenseDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIInstanceLicUserDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "NotesAndFilesTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.NotesAndFilesTabCaption"
					},
					"items": [],
					"order": 2
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Files",
				"values": {
					"itemType": 2
				},
				"parentName": "NotesAndFilesTab",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "NotesControlGroup",
				"values": {
					"itemType": 15,
					"caption": {
						"bindTo": "Resources.Strings.NotesGroupCaption"
					},
					"items": []
				},
				"parentName": "NotesAndFilesTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Notes",
				"values": {
					"bindTo": "GKINotes",
					"dataValueType": 1,
					"contentType": 4,
					"layout": {
						"column": 0,
						"row": 0,
						"colSpan": 24
					},
					"labelConfig": {
						"visible": false
					},
					"controlConfig": {
						"imageLoaded": {
							"bindTo": "insertImagesToNotes"
						},
						"images": {
							"bindTo": "NotesImagesCollection"
						}
					}
				},
				"parentName": "NotesControlGroup",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIADGroupsTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.GKIADGroupsTabCaption"
					},
					"items": [],
					"order": 1
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "GKIInstanceGroupADDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GKIADGroupsTab",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIGroupADInstanceLicenseDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GKIADGroupsTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "GKIGroupADUsersDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GKIADGroupsTab",
				"propertyName": "items",
				"index": 2
			}
		]/**SCHEMA_DIFF*/
	};
});
