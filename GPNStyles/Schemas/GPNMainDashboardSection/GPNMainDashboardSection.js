define("GPNMainDashboardSection", [], function() {
	return {
		entitySchemaName: "GPNMainDashboard",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "remove",
				"name": "ActionButtonsContainer"
			},
			{
				"operation": "remove",
				"name": "DataViewsContainer"
			},
		]/**SCHEMA_DIFF*/,
		methods: {
			getDefaultGridDataViewCaption: function() {
				return Terrasoft.SysValue.CURRENT_USER_CONTACT.displayValue;
			},

			getDefaultDataViews: this.Terrasoft.emptyFn,

		}
	};
});
