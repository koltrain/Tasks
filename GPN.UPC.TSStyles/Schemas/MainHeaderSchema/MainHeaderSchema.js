define("MainHeaderSchema", ["MainHeaderSchemaResources","IconHelper", "ConfigurationConstants", "css!TSGPNStyles"], 
function(resources,iconHelper, ConfigurationConstants) {

	//Цвета графиков
	Terrasoft.DashboardEnums.WidgetColorSet = [
		"#023b79",
		"#00559c",
		"#0270bb",
		"#0995d8",
		"#0db2e9",
		"#16c3ef",
		"#80d8f6",
		"#a7e2f8",
		"#9aa6b4"
	];
	
	Terrasoft.DashboardEnums.StyleColors = {
		"widget-green": Terrasoft.DashboardEnums.WidgetColorSet[1],
		"widget-mustard": Terrasoft.DashboardEnums.WidgetColorSet[2],
		"widget-orange": Terrasoft.DashboardEnums.WidgetColorSet[3],
		"widget-coral": Terrasoft.DashboardEnums.WidgetColorSet[4],
		"widget-violet": Terrasoft.DashboardEnums.WidgetColorSet[5],
		"widget-navy": Terrasoft.DashboardEnums.WidgetColorSet[6],
		"widget-blue": Terrasoft.DashboardEnums.WidgetColorSet[0],
		"widget-turquoise": Terrasoft.DashboardEnums.WidgetColorSet[7],
		"widget-dark-turquoise": Terrasoft.DashboardEnums.WidgetColorSet[8]
	};
	
	//Описания цветов графиков
	Terrasoft.DashboardEnums.WidgetColor = {
		"widget-green": {
			value: "widget-green",
			displayValue: resources.localizableStrings.Style023b79,
			imageConfig: resources.localizableImages.Image023b79
		},
		"widget-mustard": {
			value: "widget-mustard",
			displayValue: resources.localizableStrings.Style00559c,
			imageConfig: resources.localizableImages.Image00559c
		},
		"widget-orange": {
			value: "widget-orange",
			displayValue: resources.localizableStrings.Style0270bb,
			imageConfig: resources.localizableImages.Image0270bb
		},
		"widget-coral": {
			value: "widget-coral",
			displayValue: resources.localizableStrings.Style0995d8,
			imageConfig: resources.localizableImages.Image0995d8
		},
		"widget-violet": {
			value: "widget-violet",
			displayValue: resources.localizableStrings.Style0db2e9,
			imageConfig: resources.localizableImages.Image0db2e9
		},
		"widget-navy": {
			value: "widget-navy",
			displayValue: resources.localizableStrings.Style16c3ef,
			imageConfig: resources.localizableImages.Image16c3ef
		},
		"widget-blue": {
			value: "widget-blue",
			displayValue: resources.localizableStrings.Style80d8f6,
			imageConfig: resources.localizableImages.Image80d8f6
		},
		"widget-dark-turquoise": {
			value: "widget-dark-turquoise",
			displayValue: resources.localizableStrings.Stylea7e2f8,
			imageConfig: resources.localizableImages.Imagea7e2f8
		},
		"widget-turquoise": {
			value: "widget-turquoise",
			displayValue: resources.localizableStrings.Style9aa6b4,
			imageConfig: resources.localizableImages.Image9aa6b4
		}
	};
	
	
	
	return {
		attributes: {},
		messages: {},
		mixins: {},
		methods: {
			onHomeClick: function(){
				this.sandbox.publish("PushHistoryState", {hash: "IntroPage/TSStartPageV2"});
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
                "parentName": "CommandLineContainer",
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
