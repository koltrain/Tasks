define("SysSettingPage", [], function() {
	return {
		entitySchemaName: "VwSysSetting",
		messages: {},
		mixins: {},
		attributes: {
			"LookupValue": {
				"dataValueType": Terrasoft.DataValueType.LOOKUP,
				"type": Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				"referenceSchemaName": "SysModule",
                "lookupListConfig": {
                    "filter": function() {
							var filterGroup = Ext.create("Terrasoft.FilterGroup");
							var code = this.get("Code");
							if (code === "SSPMainPage") {
								filterGroup.add("IsSysModule",
                                	Terrasoft.createColumnIsNotNullFilter("Id"));
                            	filterGroup.add("IsSystem",
                                	Terrasoft.createColumnFilterWithParameter(
                                    Terrasoft.ComparisonType.EQUAL,
                                    "IsSystem",
									false));
								return filterGroup;
							} else if (code === "GPNMainPage") {
								filterGroup.add("IsSysModule",
                                	Terrasoft.createColumnIsNotNullFilter("Id"));
                            	filterGroup.add("IsSystem",
                                	Terrasoft.createColumnFilterWithParameter(
                                    Terrasoft.ComparisonType.EQUAL,
                                    "IsSystem",
									true));
								return filterGroup;
							}
                            return filterGroup;
                    	}
				}
			}
		},
		rules: {},
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		methods: {},
		diff: /**SCHEMA_DIFF*/[]/**SCHEMA_DIFF*/
	};
});
