define("GKILicDetail", [], function() {
	return {
		entitySchemaName: "GKILic",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[]/**SCHEMA_DIFF*/,
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
