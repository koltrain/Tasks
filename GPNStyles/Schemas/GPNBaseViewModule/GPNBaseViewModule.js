define("GPNBaseViewModule", ["ServiceHelper"],
	function(serviceHelper) {
		Ext.define("Terrasoft.configuration.GPNBaseViewModule", {
			alternateClassName: "Terrasoft.GPNBaseViewModule",
			override: "Terrasoft.BaseViewModule",
 
			/**
			 * @overridden
			 * Init code of the home page for the current user.
			 * @protected
			 * @param {Function} callback The callback function.
			 * @param {Object} scope The scope of callback function.
			 */
			initHomePage: function(callback, scope) {
				serviceHelper.callService({
					serviceName: "CurrentUserService",
					methodName: "GetCurrentUserHomePage",
					callback: function(response, success) {
						if (success && response && response.homePage) {
							this.homeModule = response.homePage;
						} else {
							if (Terrasoft.isCurrentUserSsp()) {
								this.initHomePageFromSysSettings(callback, scope);
								return;
							}
							this.initHomePageFromSysSettingsAllEmployees(callback, scope);
							return;
						}
						Ext.callback(callback, scope || this);
					},
					scope: this
				});
			},
	
			/**
			 * Initializes code of the home page for the current user from system settings.
			 * @protected
			 * @param {Function} callback The callback function.
			 * @param {Object} scope The scope of callback function.
			 */
			initHomePageFromSysSettingsAllEmployees: function(callback, scope) {
				const sysSettingsCodes = ["GPNMainPage"];
				Terrasoft.SysSettings.querySysSettings(sysSettingsCodes, function (sysSettings) {
					if (sysSettings) {
						const sysSettingsGPNMainPage = sysSettings.GPNMainPage;
						const mainPageModuleId = sysSettingsGPNMainPage.value;
						Terrasoft.each(Terrasoft.configuration.ModuleStructure, function(module) {
							if (module.moduleId === mainPageModuleId) {
								this.homeModule = module.entitySchemaName;
								return false;
							}
							this.homeModule = this.defaultHomeModule;
						}, this);
					} else {
						this.homeModule = this.defaultHomeModule;
					}
					Ext.callback(callback, scope || this);
				}, this);
			},
			
		});
	});