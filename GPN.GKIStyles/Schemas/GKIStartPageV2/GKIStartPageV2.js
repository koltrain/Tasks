define("GKIStartPageV2", ["GKIStartPageV2Resources"],
    function(resources) {
        return {
            entitySchemaName: "Activity",
            attributes: {
                "isHead": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
                "isKOV": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
                "isTSCECComposition": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
                "isContactCareer": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
                "isITTargetRepository": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
                "isEvaluationOwnerButton": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
                "isKFIRequestButton": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
                "isSubordinatesList": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
                "IsKPIDetail": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
                "IsOwnerDetail": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
                "IsLeaderDetail": {
                    dataValueType: Terrasoft.DataValueType.BOOLEAN,
                    type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                    value: false,
                },
            },
            modules: /**SCHEMA_MODULES*/ {} /**SCHEMA_MODULES*/,
            details: /**SCHEMA_DETAILS*/ {} /**SCHEMA_DETAILS*/,
            businessRules: /**SCHEMA_BUSINESS_RULES*/ {} /**SCHEMA_BUSINESS_RULES*/,
            messages: {
                "getCardInfo": {
                    mode: this.Terrasoft.MessageMode.BROADCAST,
                    direction: this.Terrasoft.MessageDirectionType.PUBLISH
                }
            },
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
                    "operation": "remove",
                    "name": "DelayExecutionButton",
                },
                {
                    "operation": "remove",
                    "name": "RecommendationModuleContainer"
                },
                {
                    "operation": "remove",
                    "name": "Tabs"
                },
                {
                    "operation": "remove",
                    "name": "SaveButton",
                },
                {
                    "operation": "remove",
                    "name": "DiscardChangesButton",
                },
                {
                    "operation": "merge",
                    "name": "ActionButtonsContainer",
                    "values": {
                        visible: false
                    }
                },
                {
                    "operation": "merge",
                    "name": "actions",
                    "values": {
                        visible: false
                    }
                },
                {
                    "operation": "remove",
                    "name": "ViewOptionsButton",
                },
                {
                    "operation": "remove",
                    "name": "CloseButton",
                },
                {
                    "operation": "insert",
                    "name": "GeneralInfoTab",
                    "parentName": "CardContentContainer",
                    "propertyName": "items",
                    "values": {
                        itemType: Terrasoft.ViewItemType.CONTAINER,
                        items: [],
                        wrapClass: ["checkout-page-wrapper-container"],
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
                    "name": "LeftColumnContainer",
                    "values": {
                        "id": "",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["left-column"]
                        },
                        "items": [],
                        "layout": {
                            "column": 0,
                            "row": 0,
                            "colSpan": 6
                        }
                    },
                    "parentName": "MainView",
                    "propertyName": "items"
                },

                {
                    "operation": "insert",
                    "name": "LeftTopColumn",
                    "values": {
                        "id": "",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["list-column", "ts-sidebar-list", "ts-box-sizing", "x-unselectable"]
                        },
                        "items": [],
                        "layout": {
                            "column": 0,
                            "row": 0,
                            "colSpan": 23
                        }
                    },
                    "parentName": "LeftColumnContainer",
                    "propertyName": "items"
                },
/*
                {
                    "operation": "insert",
                    "name": "GoalButton",
                    "parentName": "LeftTopColumn",
                    "propertyName": "items",
                    "values": {
                        "id": "GoalButton",
                        "tag": "GoalButton",
                        "itemType": Terrasoft.ViewItemType.BUTTON,
                        "style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        "classes": {
                            "textClass": ["list-column-button"],
                            "wrapperClass": ["list-column-button"],
                            "imageClass": ["list-item-image"]
                        },
                        "caption": {
                            "bindTo": "Resources.Strings.GoalButtonCaption"
                        },
                        "iconAlign": this.Terrasoft.controls.ButtonEnums.iconAlign.LEFT,
                        "imageConfig": {
                            "bindTo": "Resources.Images.GoalButtonIcon"
                        },
                        "click": {
                            "bindTo": "onButtonClick"
                        }
                    },
                    "index": 0
                },
                {
                    "operation": "insert",
                    "name": "AssessmentButton",
                    "parentName": "LeftTopColumn",
                    "propertyName": "items",
                    "values": {
                        "id": "AssessmentButton",
                        "tag": "AssessmentButton",
                        "itemType": Terrasoft.ViewItemType.BUTTON,
                        "style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        "classes": {
                            "textClass": ["list-column-button"],
                            "wrapperClass": ["list-column-button"],
                            "imageClass": ["list-item-image"]
                        },
                        "caption": {
                            "bindTo": "Resources.Strings.AssessmentButtonCaption"
                        },
                        "imageConfig": {
                            "bindTo": "Resources.Images.AssessmentButtonIcon"
                        },
                        "click": {
                            "bindTo": "onButtonClick"
                        }
                    },
                    "index": 1
                },
                {
                    "operation": "insert",
                    "name": "ReportButton",
                    "parentName": "LeftTopColumn",
                    "propertyName": "items",
                    "values": {
                        "id": "ReportButton",
                        "tag": "ReportButton",
                        "itemType": Terrasoft.ViewItemType.BUTTON,
                        "style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        "classes": {
                            "textClass": ["list-column-button"],
                            "wrapperClass": ["list-column-button"],
                            "imageClass": ["list-item-image"]
                        },
                        "caption": {
                            "bindTo": "Resources.Strings.ReportButtonCaption"
                        },
                        "imageConfig": {
                            "bindTo": "Resources.Images.ReportButtonIcon"
                        },
                        "click": {
                            "bindTo": "onButtonClick"
                        }
                    },
                    "index": 2
                },
                {
                    "operation": "insert",
                    "name": "DivisionMapButton",
                    "parentName": "LeftTopColumn",
                    "propertyName": "items",
                    "values": {
                        "id": "DivisionMapButton",
                        "tag": "DivisionMapButton",
                        "itemType": Terrasoft.ViewItemType.BUTTON,
                        "style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        "classes": {
                            "textClass": ["list-column-button"],
                            "wrapperClass": ["list-column-button"],
                            "imageClass": ["list-item-image"]
                        },
                        "caption": {
                            "bindTo": "Resources.Strings.DivisionMapButtonCaption"
                        },
                        "imageConfig": {
                            "bindTo": "Resources.Images.DivisionMapButtonIcon"
                        },
                        "click": {
                            "bindTo": "onButtonClick"
                        }
                    },
                    "index": 3
                },
*/
                {
                    "operation": "insert",
                    "name": "LeftMiddleColumn",
                    "values": {
                        "visible": {"bindTo": "isSubordinatesList"},
                        "id": "",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["subject-list-column",
                                "ts-sidebar-list",
                                "ts-box-sizing",
                                "x-unselectable"]
                        },
                        "items": [],
                        "layout": {
                            "column": 0,
                            "row": 0,
                            "colSpan": 23
                        }
                    },
                    "parentName": "LeftColumnContainer",
                    "propertyName": "items"
                },
				/*
				{
                    "operation": "insert",
                    "name": "SubGoalButton",
                    "parentName": "LeftMiddleColumn",
                    "propertyName": "items",
                    "values": {
                        "id": "SubGoalButton",
                        "tag": "SubGoalButton",
                        "itemType": Terrasoft.ViewItemType.BUTTON,
                        "style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        "classes": {
                            "textClass": ["list-column-button"],
                            "wrapperClass": ["list-column-button"],
                            "imageClass": ["list-item-image"]
                        },
                        "caption": {
                            "bindTo": "Resources.Strings.SubGoalButtonCaption"
                        },
                        "iconAlign": this.Terrasoft.controls.ButtonEnums.iconAlign.LEFT,
                        "imageConfig": {
                            "bindTo": "Resources.Images.SubGoalButtonIcon"
                        },
                        "click": {
                            "bindTo": "onButtonClick"
                        }
                    },
                    "index": 0
                },
                {
                    "operation": "insert",
                    "name": "SubAssessmentButton",
                    "parentName": "LeftMiddleColumn",
                    "propertyName": "items",
                    "values": {
                        "id": "SubAssessmentButton",
                        "tag": "SubAssessmentButton",
                        "itemType": Terrasoft.ViewItemType.BUTTON,
                        "style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        "classes": {
                            "textClass": ["list-column-button"],
                            "wrapperClass": ["list-column-button"],
                            "imageClass": ["list-item-image"]
                        },
                        "caption": {
                            "bindTo": "Resources.Strings.SubAssessmentButtonCaption"
                        },
                        "imageConfig": {
                            "bindTo": "Resources.Images.SubAssessmentButtonIcon"
                        },
                        "click": {
                            "bindTo": "onButtonClick"
                        }
                    },
                    "index": 1
                },
*/
                {
                    "operation": "insert",
                    "name": "LeftBottomColumn",
                    "values": {
                        "id": "",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["contracts-column"]
                        },
                        "items": [],
                    },
                    "parentName": "LeftColumnContainer",
                    "propertyName": "items"
                },
                {
                    "operation": "insert",
                    "name": "SpecialContainer",
                    "values": {
                        "id": "",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["header-list-button", "title-link", "active-button"]
                        },
                        "items": [],
                    },
                    "parentName": "LeftBottomColumn",
                    "propertyName": "items"
				},
				/*
                {
                    "operation": "insert",
                    "name": "ProdContractButton",
                    "parentName": "SpecialContainer",
                    "propertyName": "items",
                    "values": {
                        "id": "ProdContractButtonId",
                        "tag": "ProdContractButton",
                        "itemType": Terrasoft.ViewItemType.BUTTON,
                        "style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        "classes": {
                            "textClass": ["list-column-button", "text-color"],
                            "wrapperClass": ["header-list-button-inner", "link-ref"],
                            "imageClass": ["list-item-image", "title-link"]
                        },
                        "caption": {
                            "bindTo": "Resources.Strings.ProdContractButtonCaption"
                        },
                        "iconAlign": this.Terrasoft.controls.ButtonEnums.iconAlign.LEFT,
                        "imageConfig": {
                            "bindTo": "Resources.Images.ProdContractButtonIcon"
                        },
                        "click": {
                            "bindTo": "onButtonClick"
                        }
                    },
                    "index": 0
                },
*/
                {
                    "operation": "insert",
                    "name": "SpecialContainerMR",
                    "values": {
                        "id": "",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["header-list-button", "title-link", "active-button"]
                        },
                        "items": [],
                    },
                    "parentName": "LeftBottomColumn",
                    "propertyName": "items"
				},
				/*
                {
                    "operation": "insert",
                    "name": "MonitoringResultButton",
                    "parentName": "SpecialContainerMR",
                    "propertyName": "items",
                    "values": {
                        "id": "MonitoringResultButtonId",
                        "tag": "MonitoringResultButton",
                        "itemType": Terrasoft.ViewItemType.BUTTON,
                        "style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        "classes": {
                            "textClass": ["list-column-button", "text-color"],
                            "wrapperClass": ["header-list-button-inner", "link-ref"],
                            "imageClass": ["list-item-image", "title-link"]
                        },
                        "caption": {
                            "bindTo": "Resources.Strings.TSMonitoringResultCaption"
                        },
                        "iconAlign": this.Terrasoft.controls.ButtonEnums.iconAlign.LEFT,
                        "imageConfig": {
                            "bindTo": "Resources.Images.ProdContractButtonIcon"
                        },
                        "click": {
                            "bindTo": "onButtonClick"
                        }
                    },
                    "index": 1
                },
*/
                {
                    "operation": "insert",
                    "name": "RightColumnContainer",
                    "values": {
                        "afterrender": {"bindTo": "renderChangesContainer"},
                        "id": "RightColumnContainer",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["right-column"]
                        },
                        "items": [],
                    },
                    "parentName": "MainView",
                    "propertyName": "items"
                },

                {
                    "operation": "insert",
                    "name": "RightLeftColumn",
                    "values": {
                        "id": "",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["middle-container"]
                        },
                        "items": [],
                    },
                    "parentName": "RightColumnContainer",
                    "propertyName": "items"
                },
                {
                    "operation": "insert",
                    "name": "RightLeftTopList",
                    "values": {
                        "id": "RightLeftTopList",
                        "afterrender": {"bindTo": "renderActionConfig"},
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["module-container", "header-list-button"]
                        },
                        "items": [],
                    },
                    "parentName": "RightLeftColumn",
                    "propertyName": "items"
                },

                {
                    "operation": "insert",
                    "name": "RightRightColumn",
                    "values": {
                        "id": "",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["right-list-container"]
                        },
                        "items": [],
                    },
                    "parentName": "RightColumnContainer",
                    "propertyName": "items"
                },
                {
                    "operation": "insert",
                    "name": "RightRightTopList",
                    "values": {
                        "id": "RightRightTopList",
                        "afterrender": {"bindTo": "renderActivity"},
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["module-container", "header-list-button"]
                        },
                        "items": [],
                    },
                    "parentName": "RightRightColumn",
                    "propertyName": "items"
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
                {
                    "operation": "insert",
                    "name": "NewsContainer",
                    "values": {
                        "id": "",
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "classes": {
                            "wrapClassName": ["header-list-button", "news-block"]
                        },
                        "items": [],
                    },
                    "parentName": "MainView2",
                    "propertyName": "items"
				},
				/*
                {
                    "operation": "insert",
                    "name": "NewsButton",
                    "parentName": "NewsContainer",
                    "propertyName": "items",
                    "values": {
                        "id": "NewsButtonId",
                        "tag": "NewsButton",
                        "itemType": Terrasoft.ViewItemType.BUTTON,
                        "style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        "classes": {
                            "textClass": ["list-column-button", "text-color"],
                            "wrapperClass": ["header-list-button-inner", "green-border", "link-ref"],
                            "imageClass": ["widget-icon-lightseagreen", "list-item-image"]
                        },
                        "caption": {"bindTo": "Resources.Strings.NewsCaption"},
                        "iconAlign": Terrasoft.controls.ButtonEnums.iconAlign.LEFT,
                        "imageConfig": {
                            "bindTo": "Resources.Images.NewsImage"
                        }
                    },
                    "index": 0
                },
                {
                    "operation": "insert",
                    "name": "NewsWrapperContainer",
                    "values": {
                        "id": "NewsWrapperContainer",
                        "selectors": {"wrapEl": "#NewsWrapperContainer"},
                        "itemType": Terrasoft.ViewItemType.CONTAINER,
                        "items": [],
                        "afterrender": {"bindTo": "newsContainerAfterRender"},
                    },
                    "parentName": "NewsContainer",
                    "propertyName": "items"
				}
				*/
            ] /**SCHEMA_DIFF*/
        };
    });
