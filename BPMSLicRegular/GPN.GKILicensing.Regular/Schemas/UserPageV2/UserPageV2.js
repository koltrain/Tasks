define("UserPageV2", [],
	function() {
		return {
			entitySchemaName: "VwSysAdminUnit",
			messages: {},
			details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
			diff: /**SCHEMA_DIFF*/[
				{
					"operation": "merge",
					"parentName": "LicenseTab",
					"name": "LicenseControlGroup",
					"propertyName": "items",
					"values": {
						"visible": false,
					}
				},
			]/**SCHEMA_DIFF*/,
			attributes: {},
			rules: {},
			methods: {
				onEntityInitialized: function() {
					this.callParent(arguments);
					this.initTabsVisibility();
				},
				initTabsVisibility: function() {
					var licenseTab = this.$TabsCollection.get("LicenseTab"); 
					licenseTab.set("Visible", false);
				},
			}
		};
	});
