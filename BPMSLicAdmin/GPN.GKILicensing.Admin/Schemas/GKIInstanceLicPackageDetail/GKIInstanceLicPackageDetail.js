define("GKIInstanceLicPackageDetail", [], function() {
	return {
		entitySchemaName: "GKIInstanceLicPackage",
		attributes: {},
      	mixins: {
			ConfigurationGridUtilities: "Terrasoft.ConfigurationGridUtilities"
		},
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[]/**SCHEMA_DIFF*/,
		methods: {
			getCopyRecordMenuItem: Terrasoft.emptyFn,

			getDeleteRecordMenuItem: Terrasoft.emptyFn,

			getAddRecordMenuItem: Terrasoft.emptyFn,

			getEditRecordMenuItem: Terrasoft.emptyFn,

			getAddRecordButtonVisible: Terrasoft.emptyFn,
			
			editRecord: Ext.emptyFn
		}
	};
});
