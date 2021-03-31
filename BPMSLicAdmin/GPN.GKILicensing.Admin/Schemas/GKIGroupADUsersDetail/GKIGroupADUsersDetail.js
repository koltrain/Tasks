define("GKIGroupADUsersDetail", [], function() {
	return {
		entitySchemaName: "GKIGroupADUsers",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[
            {
                "operation": "insert",
                "parentName": "Detail",
                "propertyName": "tools",
                "name": "AddTypedRecordButton",
                "values": {
                    "itemType": Terrasoft.ViewItemType.INFORMATION_BUTTON,
                    "content": {"bindTo": "Resources.Strings.InfoButton" },
                    "controlConfig": {
                        "imageConfig": {
                            "bindTo": "Resources.Images.InfoIcon"
                        }
                    }
                }
            }
		]/**SCHEMA_DIFF*/,
		methods: {
			//region overridden методы убирающие кнопки детали
				getCopyRecordMenuItem: Terrasoft.emptyFn,

				getDeleteRecordMenuItem: Terrasoft.emptyFn,

				getAddRecordMenuItem: Terrasoft.emptyFn,

				getEditRecordMenuItem: Terrasoft.emptyFn,

				getAddRecordButtonVisible: Terrasoft.emptyFn,

				editRecord: Ext.emptyFn,

				getDataImportMenuItemVisible: Terrasoft.emptyFn
			//endregion
		}
	};
});
