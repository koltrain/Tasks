define("SysSettingPage", ["GKILicensingConstantsJs"], function(constants) {
	return {
		entitySchemaName: "VwSysSetting",
		attributes: {
			"IsDefaultValueVisible": {
				value: true,
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN
			}
		},
		methods: {
			/**
			 * @overridden
			 * действия после инициализации карточки
			 */
			onEntityInitialized: function() {
				this.callParent(arguments);
				this.checkSysSetting();
			},
			/**
			 * @protected
			 * Невидимость системной настройки Раздачи лицензий при создании пользователей через LDAP
			 */
			checkSysSetting: function() {
				var sysSettingId = this.get("Id");
				if(sysSettingId == constants.SysSettings.LDAPAllowLicenseDistributionDuringLDAPSync) {
					this.set("IsDefaultValueVisible", false);
				}
			}
		},
		diff: [
			{
				"operation": "insert",
				"parentName": "ValueContainer",
				"propertyName": "items",
				"name": "BooleanValue",
				"values": {
					"bindTo": "BooleanValue",
					"labelConfig": {
						"caption": {
							"bindTo": "Resources.Strings.DefaultValueCaption"
						}
					},
					"visible": {
						bindTo: "IsDefaultValueVisible"
					}
				}
			}
		]
	};
});
