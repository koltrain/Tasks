define("SysSettingPage", [], function() {
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
			 * @inheritdoc BasePageV2#onEntityInitialized
			 * @override
			 */
			onEntityInitialized: function() {
				this.callParent(arguments);
				this.checkSysSetting();
			},
			checkSysSetting: function() {
				var sysSettingId = this.get("Id");
				if(sysSettingId == "d84677da-8c01-48a5-987a-5e156b27764b") {
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
