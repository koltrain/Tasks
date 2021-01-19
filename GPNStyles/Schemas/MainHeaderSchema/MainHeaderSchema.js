define("MainHeaderSchema", ["MainHeaderSchemaResources", "IconHelper", "ConfigurationConstants", "ServiceHelper", "GPNDashboardWidgetCustomization", "css!GPNGPNStyles"], 
function(resources,iconHelper, ConfigurationConstants, serviceHelper) {
	return {
		attributes: {
			"GPNHomeModule": {
				"dataValueType": this.Terrasoft.DataValueType.TEXT,
				"type": this.Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				"value": ""
			},
		},
		messages: {},
		mixins: {},
		methods: {
			/**
			 * @inheritdoc Terrasoft.BaseSchemaViewModel#init
			 * @overridden
			 */
			init: function() {
				this.callParent(arguments);
				this.GPNinitHomePage();
			},

			/**
			 * Sets homepage for current user.
			 * @public
			 */
			GPNinitHomePage: function() {
				this.initHomePage();
			},

			
			/**
			 * Opens home page by home button.
			 * @public
			 */
			onHomeClick: function(){
				this.openHomePage();
			},

			/**
			 * Opens home page.
			 * @overridden
			 * @protected
			 */
			openHomePage: function() {
				this.sandbox.publish("PushHistoryState", {
					hash: this.getHomePagePath(),
					stateObj: {forceNotInChain: true}
				});
			},
			
			/**
			 * Init code of the home page for the current user.
			 * @inheritDoc Terrasoft.configuration.GPNBaseViewModule
			 * @protected
			 */
			initHomePage: function() {
				serviceHelper.callService({
					serviceName: "CurrentUserService",
					methodName: "GetCurrentUserHomePage",
					callback: function(response, success) {
						if (success && response && response.homePage) {
							this.set("GPNHomeModule", response.homePage);
						} else {
							if (Terrasoft.isCurrentUserSsp()) {
								this.initHomePageFromSysSettings();
								return;
							}
							this.initHomePageFromSysSettingsAllEmployees();
							return;
						}
					},
					scope: this
				});
			},
	
			/**
			 * Initializes code of the home page for the current user from system settings.
			 * @inheritDoc Terrasoft.configuration.GPNBaseViewModule
			 * @protected
			 */
			initHomePageFromSysSettingsAllEmployees: function() {
				const sysSettingsCodes = ["GPNMainPage"];
				Terrasoft.SysSettings.querySysSettings(sysSettingsCodes, function (sysSettings) {
					if (sysSettings) {
						const sysSettingsGPNMainPage = sysSettings.GPNMainPage;
						const mainPageModuleId = sysSettingsGPNMainPage.value;
						Terrasoft.each(Terrasoft.configuration.ModuleStructure, function(module) {
							if (module.moduleId === mainPageModuleId) {
								this.set("GPNHomeModule", module.entitySchemaName);
								return false;
							}
							this.set("GPNHomeModule", ConfigurationConstants.DefaultHomeModule);
						}, this);
					} else {
						this.set("GPNHomeModule", ConfigurationConstants.DefaultHomeModule);
					}
				}, this);
			},
			/**
			 * Initializes code of the home page for the current user from system settings.
			 * @inheritDoc Terrasoft.configuration.BaseViewModule
			 * @protected
			 */
			initHomePageFromSysSettings: function() {
				const sysSettingsCodes = ["SSPMainPage"];
				Terrasoft.SysSettings.querySysSettings(sysSettingsCodes, function (sysSettings) {
					if (sysSettings) {
						const sysSettingsSSPMainPage = sysSettings.SSPMainPage;
						const mainPageModuleId = sysSettingsSSPMainPage.value;
						Terrasoft.each(Terrasoft.configuration.ModuleStructure, function(module) {
							if (module.moduleId === mainPageModuleId) {
								this.set("GPNHomeModule", module.entitySchemaName);
								return false;
							}
						}, this);
					} else {
						this.set("GPNHomeModule", ConfigurationConstants.DefaultHomeModule);
					}
				}, this);
			},
			/**
			 * Returns the path to the home page.
			 * @inheritDoc Terrasoft.configuration.BaseViewModule
			 * @protected
			 * @return {String}
			 */
			getHomePagePath: function() {
				const module = Terrasoft.configuration.ModuleStructure[this.get("GPNHomeModule")];
				const path = module
					? Terrasoft.combinePath(module.sectionModule, module.sectionSchema)
					: this.getHomeModulePath();
				return path;
			},
			/**
			 * @inheritDoc Terrasoft.configuration.BaseViewModule
			 * @public
			 */
			getHomeModulePath: function() {
				return this.Terrasoft.combinePath(ConfigurationConstants.DefaultHomeModule, Terrasoft.configuration.defaultIntroPageName);
			}
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
