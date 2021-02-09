define("SystemDesigner", ["RightUtilities"], function(RightUtilities) {
	return {
		mixins: {},
		attributes: {},
		methods: {
			/**
			 * @public
			 * @desc проверяет права на чтение настроек службы лицензирования
			 */
			onGKILicensingLinkClick: function() {
				var canReadLicensingSettings = this.get("GKICanReadLicensingSettings");
				if (!this.Ext.isEmpty(canReadLicensingSettings)) {
					this.navigateToLicensingSettingsSettings();
				} else {
					RightUtilities.checkCanExecuteOperation({
						operation: "GKICanReadLicensingSettings"
					}, function(result) {
						this.set("GKICanReadLicensingSettings", result);
						this.navigateToLicensingSettingsSettings();
					}, this);
				}
				return false;
			},

			/**
			 * @private
			 * @desc отправляет на страницу настроек службы лицензирования
			 */
			navigateToLicensingSettingsSettings: function() {
				if (this.get("GKICanReadLicensingSettings") === true) {
					this.sandbox.publish("PushHistoryState", {
						hash: "ConfigurationModuleV2/GKILicensingSettingsPage/"
					});
				} else {
					this.showPermissionsErrorMessage("GKICanReadLicensingSettings");
				}
			}
		},
		diff: [
			{
				"operation": "remove",
				"propertyName": "items",
				"parentName": "UsersTile",
				"name": "LicenseManager",
			},
			{
				"operation": "insert",
				"propertyName": "items",
				"parentName": "IntegrationTile",
				"name": "GKILicensing",
				"values": {
					"itemType": Terrasoft.ViewItemType.LINK,
					"caption": {"bindTo": "Resources.Strings.GKILicensingLinkCaption"},
					"click": {"bindTo": "onGKILicensingLinkClick"}
				}
			}
		]
	};
});
