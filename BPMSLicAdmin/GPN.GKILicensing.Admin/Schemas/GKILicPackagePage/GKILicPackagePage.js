define("GKILicPackagePage", [], function () {
    return {
        entitySchemaName: "GKILicPackage",
        attributes: {
            "GKIInstanceId": {
                dataValueType: Terrasoft.DataValueType.GUID
            }
        },
        modules: /**SCHEMA_MODULES*/{
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
			},
			"Indicator404a6372-9d68-400e-b286-02bffbff45b2": {
				"moduleId": "Indicator404a6372-9d68-400e-b286-02bffbff45b2",
				"moduleName": "CardWidgetModule",
				"config": {
					"parameters": {
						"viewModelConfig": {
							"widgetKey": "Indicator404a6372-9d68-400e-b286-02bffbff45b2",
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
			}
		}/**SCHEMA_DETAILS*/,
        businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
        messages: {
			"GetRow": {
                mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.SUBSCRIBE
            },
            "GetRows": {
                mode: Terrasoft.MessageMode.PTP,
                direction: Terrasoft.MessageDirectionType.SUBSCRIBE
            }
		},
        methods: {
			init: function () {
			this.callParent(arguments);
			//отложили динамические дешборды из-за нестабильности формирования
			/*
			this.sandbox.subscribe("GetRow", function (args) {
				this.generateDashboardCountLic(args.row);
				this.generateDashboardActiveLic(args.row);
				this.generateDashboardUserCountUsingProductOnInstance(args.row);
			}, this, ["RowInDash"]);
			*/
		},
		/**
		 * Формирование и отрисовка дашбоарда "Количество экземпляров с установленным продуктом"
		 * @param {*} row 
		 */
		generateDashboardCountLic:function(row){
			var moduleIndicatorCount= "Indicator8b082c1a-378e-4fe8-80d7-06e180a5adac";
			var parametersConfig = {
				aggregationType: 1,
				caption: "Количество экземпляров с установленным продуктом",
				entitySchemaName: "GKIInstanceLicPackage",
				filterData: "{\"className\":\"Terrasoft.FilterGroup\",\"items\":{\"57362d03-275f-4203-86fe-8aa35c1e3d56\":{\"className\":\"Terrasoft.CompareFilter\",\"filterType\":1,\"comparisonType\":3,\"isEnabled\":true,\"trimDateTimeParameterToDate\":false,\"leftExpression\":{\"className\":\"Terrasoft.ColumnExpression\",\"expressionType\":0,\"columnPath\":\"Id\"},\"isAggregative\":false,\"key\":\"57362d03-275f-4203-86fe-8aa35c1e3d56\",\"dataValueType\":0,\"leftExpressionCaption\":\"Id\",\"rightExpression\":{\"className\":\"Terrasoft.ParameterExpression\",\"expressionType\":2,\"parameter\":{\"className\":\"Terrasoft.Parameter\",\"dataValueType\":0,\"value\":\""+row+"\"}}}},\"logicalOperation\":0,\"isEnabled\":true,\"filterType\":6,\"rootSchemaName\":\"GKIInstanceLicPackage\",\"key\":\"\"}",
				fontStyle: "big-indicator-font-size",
				format: {textDecorator: "{0}", type: 4, decimalPrecision: 0},
				primaryColumnValue: "b0dd2c71-d82a-4fff-90ad-6e7cf17cec60",
				sectionBindingColumn: "GKILicPackage",
				sectionId: "910de435-efa7-496d-a881-3c59b7cc95a7",
				style: "widget-dark-turquoise"
			}
			this.sandbox.loadModule("IndicatorModule",{
				id: moduleIndicatorCount+"_CardWidget",
				renderTo: "CardWidget"+moduleIndicatorCount,
				instanceConfig:{
					moduleConfig:parametersConfig
				}
			});
		},
		/**
		 *  Формирование и отрисовка дашбоарда "Активные лицензии по продукту"
		 * @param {*} row 
		 */
		generateDashboardActiveLic:function(row){
			var moduleIndicatorCount= "Indicator404a6372-9d68-400e-b286-02bffbff45b2";
			var esq = Ext.create("Terrasoft.EntitySchemaQuery", {
				rootSchemaName: "GKIInstanceLicPackage"
			});

			esq.addColumn("GKIInstance");

			esq.filters.add("GKIInstanceLicFilters",Terrasoft.createColumnFilterWithParameter(
				Terrasoft.ComparisonType.EQUAL,"Id",row));

			esq.getEntityCollection(function (res) {
				if(res.success == true){
				var instance = res.collection.collection.items[0].$GKIInstance;
				var parametersConfig = {
					aggregationType: 1,
					caption: "Активные лицензии по продукту",
					entitySchemaName: "GKILic",
					filterData: "{\"className\": \"Terrasoft.FilterGroup\",\"items\": {\"4e72fd77-9706-4a77-b013-db642e85152e\": {\"className\": \"Terrasoft.InFilter\",\"filterType\": 4,\"comparisonType\": 3,\"isEnabled\": true,\"trimDateTimeParameterToDate\": false,\"leftExpression\": {\"className\": \"Terrasoft.ColumnExpression\",\"expressionType\": 0,\"columnPath\": \"GKILicStatus\"},\"isAggregative\": false,\"key\": \"4e72fd77-9706-4a77-b013-db642e85152e\",\"dataValueType\": 10,\"leftExpressionCaption\": \"Статус\",\"referenceSchemaName\": \"GKILicStatus\",\"rightExpressions\": [{\"className\": \"Terrasoft.ParameterExpression\",\"expressionType\": 2,\"parameter\": {\"className\": \"Terrasoft.Parameter\",\"dataValueType\": 10,\"value\": {\"Name\": \"Активна\",\"Id\": \"672f84f8-45de-4383-8220-a805b30b745e\",\"value\": \"672f84f8-45de-4383-8220-a805b30b745e\",\"displayValue\": \"Активна\"}}}]},\"a00982d2-b999-436b-9bd0-5d2f94252a43\": {\"className\": \"Terrasoft.ExistsFilter\",\"filterType\": 5,\"comparisonType\": 15,\"isEnabled\": true,\"trimDateTimeParameterToDate\": false,\"leftExpression\": {\"className\": \"Terrasoft.ColumnExpression\",\"expressionType\": 0,\"columnPath\": \"[GKIInstanceLicense:GKILic].GKIInstance.Id\"},\"isAggregative\": true,\"key\": \"a00982d2-b999-436b-9bd0-5d2f94252a43\",\"dataValueType\": 4,\"leftExpressionCaption\": \"Лицензии экземпляра (по колонке Лицензия).Приложение\",\"subFilters\": {\"className\": \"Terrasoft.FilterGroup\",\"items\": {\"f6afe28d-acbc-48f9-86e2-712ff30be63f\": {\"className\": \"Terrasoft.InFilter\",\"filterType\": 4,\"comparisonType\": 3,\"isEnabled\": true,\"trimDateTimeParameterToDate\": false,\"leftExpression\": {\"className\": \"Terrasoft.ColumnExpression\",\"expressionType\": 0,\"columnPath\": \"GKIInstance\"},\"isAggregative\": false,\"key\": \"f6afe28d-acbc-48f9-86e2-712ff30be63f\",\"dataValueType\": 10,\"leftExpressionCaption\": \"Приложение\",\"referenceSchemaName\": \"GKIInstance\",\"rightExpressions\": [{\"className\": \"Terrasoft.ParameterExpression\",\"expressionType\": 2,\"parameter\": {\"className\": \"Terrasoft.Parameter\",\"dataValueType\": 10,\"value\": {\"GKIName\":\""+instance.displayValue+"\",\"Id\": \""+instance.value+"\",\"value\": \""+instance.value+"\",\"displayValue\":\""+ instance.displayValue+"\"}}}]}},\"logicalOperation\": 0,\"isEnabled\": true,\"filterType\": 6,\"rootSchemaName\": \"GKIInstanceLicense\",\"key\": \"6ce8d835-d101-42e0-8bf9-48473e408819\"}}},\"logicalOperation\": 0,\"isEnabled\": true,\"filterType\": 6,\"rootSchemaName\": \"GKILic\",\"key\": \"\"}",
					fontStyle: "default-indicator-font-size",
					format: {textDecorator: "{0}", thousandSeparator: " ", type: 5, dateFormat: "d.m.Y", decimalPrecision: 2},
					primaryColumnValue: "b26caeea-35a7-4371-a6b8-be96f336b661",
					sectionBindingColumn: "GKILicPackage",
					sectionId: "910de435-efa7-496d-a881-3c59b7cc95a7",
					style: "widget-coral"
				}
				this.sandbox.loadModule("IndicatorModule",{
					id: moduleIndicatorCount+"_CardWidget",
					renderTo: "CardWidget"+moduleIndicatorCount,
					instanceConfig:{
						moduleConfig:parametersConfig
					}
				});
			}
			},this);
		},
		/**
		 * Формирование и отрисовка дашбоарда "Количество пользователей, использующих продукт на экземпляре"
		 * @param {*} row 
		 */
		generateDashboardUserCountUsingProductOnInstance:function(row){
			var moduleIndicatorCount= "Chart80563b13-6a9e-4e7b-8ad6-97669ec7f8a8";
			var esq = Ext.create("Terrasoft.EntitySchemaQuery", {
				rootSchemaName: "GKIInstanceLicPackage"
			});

			esq.addColumn("GKIInstance");

			esq.filters.add("GKIInstanceLicPackageFilter",Terrasoft.createColumnFilterWithParameter(
				Terrasoft.ComparisonType.EQUAL,"Id",row));


			esq.getEntityCollection(function (res) {
				if(res.success == true){
				var instance = res.collection.collection.items[0].$GKIInstance;
				var parametersConfig = {
					YAxisCaption: "Количество пользователей",
					caption: "Количество пользователей, использующих продукт на экземпляре",
					filterData: "{\"className\":\"Terrasoft.FilterGroup\",\"items\":{\"58625d3c-ebc1-4108-a349-439a85dfc636\":{\"className\":\"Terrasoft.IsNullFilter\",\"filterType\":2,\"comparisonType\":2,\"isEnabled\":true,\"trimDateTimeParameterToDate\":false,\"leftExpression\":{\"className\":\"Terrasoft.ColumnExpression\",\"expressionType\":0,\"columnPath\":\"GKICountUsed\"},\"isAggregative\":false,\"key\":\"58625d3c-ebc1-4108-a349-439a85dfc636\",\"dataValueType\":4,\"leftExpressionCaption\":\"\u0420\u0430\u0441\u043f\u0440\u0435\u0434\u0435\u043b\u0435\u043d\u043e\",\"isNull\":false},\"74911c86-7529-4888-9eec-b4a2834f4013\":{\"className\":\"Terrasoft.InFilter\",\"filterType\":4,\"comparisonType\":3,\"isEnabled\":true,\"trimDateTimeParameterToDate\":false,\"leftExpression\":{\"className\":\"Terrasoft.ColumnExpression\",\"expressionType\":0,\"columnPath\":\"GKIInstance\"},\"isAggregative\":false,\"key\":\"74911c86-7529-4888-9eec-b4a2834f4013\",\"dataValueType\":10,\"leftExpressionCaption\":\"\u041f\u0440\u0438\u043b\u043e\u0436\u0435\u043d\u0438\u0435\",\"referenceSchemaName\":\"GKIInstance\",\"rightExpressions\":[{\"className\":\"Terrasoft.ParameterExpression\",\"expressionType\":2,\"parameter\":{\"className\":\"Terrasoft.Parameter\",\"dataValueType\":10,\"value\":{\"GKIName\":\""+instance.displayValue+"\",\"Id\":\""+instance.value+"\",\"value\":\""+instance.value+"\",\"displayValue\":\""+instance.displayValue+"\"}}}]}},\"logicalOperation\":0,\"isEnabled\":true,\"filterType\":6,\"rootSchemaName\":\"GKIInstanceLicense\",\"key\":\"\"}",
					format: {type: 4, decimalPrecision: 0},
					func: 2,
					isLegendEnabled: false,
					isStackedChart: false,
					orderBy: "GroupByField",
					orderDirection: "Ascending",
					primaryColumnName: "Id",
					primaryColumnValue: "b26caeea-35a7-4371-a6b8-be96f336b661",
					schemaName: "GKIInstanceLicense",
					sectionBindingColumn: "GKILic.GKILicPackage",
					sectionId: "910de435-efa7-496d-a881-3c59b7cc95a7",
					seriesConfig: [],
					styleColor: "widget-green",
					type: "column",
					useEmptyValue: false,
					xAxisColumn: "GKIInstance",
					xAxisDefaultCaption: null,
					yAxisColumn: "GKICountUsed",
					yAxisConfig: {position: 0},
					yAxisDefaultCaption: null
				}
				this.sandbox.loadModule("ChartModule",{
					id: moduleIndicatorCount+"_CardWidget",
					renderTo: "CardWidget"+moduleIndicatorCount,
					instanceConfig:{
						moduleConfig:parametersConfig
					}
				});
						}
			},this);
		}
	},
        dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
        diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "Indicator8b082c1a-378e-4fe8-80d7-06e180a5adac",
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
				"name": "Indicator404a6372-9d68-400e-b286-02bffbff45b2",
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
			}
		]/**SCHEMA_DIFF*/
    };
});
