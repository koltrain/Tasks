define("GKILicUserInstanceLicPackageDeactivationOnlyDetail", [], function() {
	return {
		entitySchemaName: "GKILicUserInstanceLicPackage",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[]/**SCHEMA_DIFF*/,
		methods: {
			/**
			 * @overriden 
			 * @desc: убирает кнопки + и ... на детали
			 */
			getToolsVisible: function() {
				return false;
			}
		}
	};
});
