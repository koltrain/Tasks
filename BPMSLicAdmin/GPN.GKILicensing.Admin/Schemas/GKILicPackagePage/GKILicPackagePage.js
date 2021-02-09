define("GKILicPackagePage", [], function() {
	return {
		entitySchemaName: "GKILicPackage",
		attributes: {},
		modules: /**SCHEMA_MODULES*/{
			"Gauged692e6f0-b5fc-4380-be27-0f7ea19f0c31": {
				"moduleId": "Gauged692e6f0-b5fc-4380-be27-0f7ea19f0c31",
				"moduleName": "CardWidgetModule",
				"config": {
					"parameters": {
						"viewModelConfig": {
							"widgetKey": "Gauged692e6f0-b5fc-4380-be27-0f7ea19f0c31",
							"recordId": "05ab5f2d-04d6-428a-8219-9fde0c8536b1",
							"primaryColumnValue": {
								"getValueMethod": "getPrimaryColumnValue"
							}
						}
					}
				}
			},
			"Indicator8b082c1a-378e-4fe8-80d7-06e180a5adac": {
				"moduleId": "Indicator8b082c1a-378e-4fe8-80d7-06e180a5adac",
				"moduleName": "CardWidgetModule",
				"config": {
					"parameters": {
						"viewModelConfig": {
							"widgetKey": "Indicator8b082c1a-378e-4fe8-80d7-06e180a5adac",
							"recordId": "05ab5f2d-04d6-428a-8219-9fde0c8536b1",
							"primaryColumnValue": {
								"getValueMethod": "getPrimaryColumnValue"
							}
						}
					}
				}
			},
			"Chart80563b13-6a9e-4e7b-8ad6-97669ec7f8a8": {
				"moduleId": "Chart80563b13-6a9e-4e7b-8ad6-97669ec7f8a8",
				"moduleName": "CardWidgetModule",
				"config": {
					"parameters": {
						"viewModelConfig": {
							"widgetKey": "Chart80563b13-6a9e-4e7b-8ad6-97669ec7f8a8",
							"recordId": "05ab5f2d-04d6-428a-8219-9fde0c8536b1",
							"primaryColumnValue": {
								"getValueMethod": "getPrimaryColumnValue"
							}
						}
					}
				}
			}
		}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{
			"GKILicDetail": {
				"schemaName": "GKILicDetail",
				"entitySchemaName": "GKILic",
				"filter": {
					"detailColumn": "GKILicPackage",
					"masterColumn": "Id"
				}
			},
			"GKIInstanceLicPackageDetail": {
				"schemaName": "GKIInstanceLicPackageDetail",
				"entitySchemaName": "GKIInstanceLicPackage",
				"filter": {
					"detailColumn": "GKILicPackage",
					"masterColumn": "Id"
				}
			},
			"GKILicUserInstanceLicPackageDetail": {
				"schemaName": "GKILicUserInstanceLicPackageDetail",
				"entitySchemaName": "GKILicUserInstanceLicPackage",
				"filter": {
					"detailColumn": "GKILicPackage",
					"masterColumn": "Id"
				}
			},
		}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods: {},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "Gauged692e6f0-b5fc-4380-be27-0f7ea19f0c31",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 5,
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
				"name": "Indicator8b082c1a-378e-4fe8-80d7-06e180a5adac",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 5,
						"column": 0,
						"row": 5,
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
				"name": "Chart80563b13-6a9e-4e7b-8ad6-97669ec7f8a8",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 5,
						"column": 0,
						"row": 10,
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
					"bindTo": "GKIName",
					"enabled": false,
					"isRequired": false
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIDescription",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 1,
						"layoutName": "Header"
					},
					"bindTo": "GKIDescription",
					"enabled": true,
					"contentType": 0
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "GKILicUserInstanceLicPackageDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "Tabe48ea68cTabLabel",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "merge",
				"name": "ESNTab",
				"values": {
					"order": 2
				}
			},
			{
				"operation": "insert",
				"name": "Tabe48ea68cTabLabel",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.Tabe48ea68cTabLabelTabCaption"
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
				"name": "GKILicDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "Tabe48ea68cTabLabel",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIInstanceLicPackageDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "Tabe48ea68cTabLabel",
				"propertyName": "items",
				"index": 1
			},
		]/**SCHEMA_DIFF*/
	};
});
