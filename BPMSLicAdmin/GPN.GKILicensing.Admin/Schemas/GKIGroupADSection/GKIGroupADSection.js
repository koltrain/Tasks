define("GKIGroupADSection", [], function() {
	return {
		entitySchemaName: "GKIGroupAD",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		methods: {
			/**
			 * @overridden
			 * @desc делает кнопку Удалить невидимой
			 */
			isVisibleDeleteAction: Terrasoft.emptyFn
		},
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "remove",
				"name": "DataGridActiveRowDeleteAction",
				"parentName": "DataGrid",
			}
		]/**SCHEMA_DIFF*/
	};
});
