define("GKIInstanceSection", [], function() {
	return {
		entitySchemaName: "GKIInstance",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		methods: {
			/**
			 * @overridden
			 * @desc удаление "Удалить" в меню "Действия"
			 */
			isVisibleDeleteAction: Terrasoft.emptyFn
		},
		diff: /**SCHEMA_DIFF*/[
			{ 
				"operation": "remove",
				"name": "DataGridActiveRowCopyAction",
				"parentName": "DataGrid",
			},
			{ 
				"operation": "remove",
				"name": "DataGridActiveRowDeleteAction",
				"parentName": "DataGrid",
			}
		]/**SCHEMA_DIFF*/
	};
});
