define("SystemDesigner", ["ServiceHelper"], function(ServiceHelper) {
	return {
		mixins: {},
		attributes: {
			"GKIIsLicenseManagerVisible": {
				"dataValueType": this.Terrasoft.DataValueType.BOOLEAN,
				"type": this.Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				"value": false
			},
		},
		methods: {
			/**
			 * @overridden
			 */
			init: function() {
				this.callParent(arguments);
				this.getGKIIsLicenseManagerVisible();
			},
			/**
			 * Видимость кнопки Менеджер лицензий
			 * @protected
			 */
			getGKIIsLicenseManagerVisible: function() {
				ServiceHelper.callService("GKILicensingRegularService", "GKIIsTheSlaveFree",  function(response) {
					this.set("GKIIsLicenseManagerVisible", response);
				}, null, this);
			}
		},
		diff: [
			{
				"operation": "merge",
				"propertyName": "items",
				"parentName": "UsersTile",
				"name": "LicenseManager",
				"values": {
					"visible" : {
						"bindTo" : "GKIIsLicenseManagerVisible"
					}
				}
			},
		]
	};
});
