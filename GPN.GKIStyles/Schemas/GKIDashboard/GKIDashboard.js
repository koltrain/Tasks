 define("GKIDashboard",
    function(resources, FormatUtils, TSClientConstants, constants, ServiceHelper) {
        return {
            entitySchemaName: "GKIDashboard",
            attributes: {
                "isHead": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },

                "isContactCareer": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
            },
            modules: /**SCHEMA_MODULES*/ {} /**SCHEMA_MODULES*/,
            details: /**SCHEMA_DETAILS*/ {} /**SCHEMA_DETAILS*/,
            businessRules: /**SCHEMA_BUSINESS_RULES*/ {} /**SCHEMA_BUSINESS_RULES*/,
            messages: {},
            methods: {
                getPageHeaderCaption: function() {
                    return Terrasoft.SysValue.CURRENT_USER_CONTACT.displayValue;
                },
                
            },
            diff: /**SCHEMA_DIFF*/[
                {
                    "operation": "remove",
                    "name": "ESNTab"
                },
                {
				"operation": "insert",
				"name": "PageHeaderContainer",
				"parentName": "RightHeaderContainer",
				"propertyName": "items",
				"values": {
					"id": "PageHeaderContainer",
					"selectors": {"wrapEl": "#PageHeaderContainer"},
					"itemType": Terrasoft.ViewItemType.CONTAINER,
					"wrapClass": ["page-header-container"],
					"items": []
				}
					},
                {
                    "operation": "insert",
                    "parentName": "GeneralInfoTab",
                    "name": "GeneralControlGroup",
                    "propertyName": "items",
                    "values": {
                        "classes": {
                            "wrapClassName": ["tile-view-wrapper"]
                        },
                        itemType: Terrasoft.ViewItemType.CONTROL_GROUP,
                        items: [],
                        controlConfig: {
                            collapsed: false
                        }
                    },
                    "index": 1,
                },
                {
                    "operation": "insert",
                    "name": "MainView",
                    "values": {
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["tile-view-wrapper"]
                        },
                        "items": [],
                        "layout": {
                            "column": 0,
                            "row": 0,
                            "colSpan": 6
                        }
                    },
                    "parentName": "GeneralControlGroup",
                    "propertyName": "items",
                    "index": 1
                },
{
				"operation": "insert",
				"name": "PageHeaderCaption",
				"parentName": "PageHeaderContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.LABEL,
					"caption": {"bindTo": "PageHeaderCaption"},
					"markerValue": {
						"bindTo": "HeaderCaptionMarkerValue"
					}
				}
			},
		/*	{
				"operation": "merge",
				"name": "PageHeaderContainer",
				"parentName": "RightHeaderContainer",
				"propertyName": "items",
				"values": {
					"id": "PageHeaderContainer",
					"selectors": {"wrapEl": "#PageHeaderContainer"},
					"itemType": Terrasoft.ViewItemType.CONTAINER,
					"wrapClass": ["page-header-container"],
					//"items": []
				},
				"index":2,
			},*/
                {
                    "operation": "insert",
                    "name": "MainView2",
                    "values": {
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["tile-view-wrapper"]
                        },
                        "items": [],
                        "layout": {
                            "column": 0,
                            "row": 0,
                            "colSpan": 6
                        }
                    },
                    "parentName": "GeneralControlGroup",
                    "propertyName": "items",
                    "index": 2
                },
                {
                    "operation": "insert",
                    "name": "TileClear",
                    "values": {
                        "id": "",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["tile-clear"]
                        },
                        "items": [],
                    },
                    "parentName": "MainView",
                    "propertyName": "items"
                },
                

            ] /**SCHEMA_DIFF*/
        };
    });
