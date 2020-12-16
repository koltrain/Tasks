define("MainHeaderSchema", ["MainHeaderSchemaResources", "IconHelper", "ConfigurationConstants", "GKIDashboardWidgetCustomization", "css!GKIGPNStyles"], 
function(resources,iconHelper, ConfigurationConstants) {

	return {
		attributes: {},
		messages: {},
		mixins: {},
		methods: {
			onHomeClick: function(){
				this.sandbox.publish("PushHistoryState", {hash: "IntroPage/GKIStartPageV2"});
			},
		},
		diff: [
			
			{
				"operation": "remove",
				"name": "logoImage",
			},
			
			//{
			//	"operation":"remove",
			//	"name":"RightLogoContainer",
			//},
			
			{
				"operation": "remove",
				"name": "MenuLogoImageContainer",
			},
			
			{
				"operation": "insert",
				"name": "LeftLogoContainer",
				"parentName": "MainHeaderContainer",
				"propertyName": "items",
				"values": {
					"id": "LeftLogoContainer",
					"selectors": {"wrapEl": "#LeftLogoContainer"},
					"itemType": Terrasoft.ViewItemType.CONTAINER,
					"wrapClass": ["left-logo-container"],
					"items": []
				},
				"index":0,
			},
			
			{
				"operation": "insert",
				"name": "LeftLogoTableContainer",
				"parentName": "LeftLogoContainer",
				"propertyName": "items",
				"values": {
					"id": "LeftLogoTableContainer",
					"selectors": {"wrapEl": "#LeftLogoTableContainer"},
					"itemType": Terrasoft.ViewItemType.CONTAINER,
					"wrapClass": ["left-logo-table-container"],
					"items": []
				},
				"index":0,
			},
			
			{
				"operation": "insert",
				"name": "LeftLogoTableCellContainer",
				"parentName": "LeftLogoTableContainer",
				"propertyName": "items",
				"values": {
					"id": "LeftLogoTableCellContainer",
					"selectors": {"wrapEl": "#LeftLogoTableCellContainer"},
					"itemType": Terrasoft.ViewItemType.CONTAINER,
					"wrapClass": ["left-logo-table-cell-container"],
					"items": []
				},
				"index":0,
			},
			
			{
				"operation": "insert",
				"name": "logoImage",
				"parentName": "LeftLogoTableCellContainer",
				"propertyName": "items",
				"values": {
					"id": "logoImage",
					"itemType": Terrasoft.ViewItemType.COMPONENT,
					"selectors": {
						"wrapEl": "#logoImage"
					},
					"hint": {"bindTo": "Resources.Strings.LogoHint"},
					"className": "Terrasoft.ImageView",
					"imageSrc": {"bindTo": "getLogoImageConfig"},
					"tag": "HeaderLogoImage",
					"click": {"bindTo": "onLogoClick"},
					"canExecute": {
						"bindTo": "canBeDestroyed"
					},
					"classes": {
						"wrapClass": ["main-header-logo-image", "cursor-pointer"]
					}
				}
			},

			{
                "operation": "insert",
                "name": "HomeButton",
                "parentName": "RightLogoContainer",
                "propertyName": "items",
                "values": {
                    "id": "HomeButtonId",
                    "tag": "HomeButton",
                    "itemType": Terrasoft.ViewItemType.BUTTON,
                    "style": Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                    "classes": {
                        "textClass": ["list-column-button", "home-icon"],
                        "wrapperClass": ["special-button-inner", "wrapper-home-icon"],
                        "imageClass": ["list-item-image", "img-home-icon"]
                    },
                    "iconAlign": this.Terrasoft.controls.ButtonEnums.iconAlign.LEFT,
                    "imageConfig": {
                        "bindTo": "Resources.Images.HomeIcon"
                    },
                    "hint": {"bindTo": "Resources.Strings.LogoHint"},
                    "click": {
                        "bindTo": "onHomeClick"
                    }
                },
                "index": 0
            },
			
			{
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
			},
			{
				"operation": "merge",
				"name": "RightButtonsContainer",
				"parentName": "RightHeaderContainer",
				"propertyName": "items",
				"values": {
					"id": "header-right-image-container",
					"itemType": Terrasoft.ViewItemType.CONTAINER,
					"wrapClass": ["context-right-image-class"],
					//"items": []
				},
				"index":3,
			},
			
			{
				"operation": "merge",
				"name": "RightLogoContainer",
				"values": {
					"visible": true,
				}
			},
			
		]
	};
});
