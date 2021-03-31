define("GKICreatedGKILicUsersRegularPage", [], function() {
	return {
		entitySchemaName: "",
		attributes: {},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{
			"GKIInstanceLicUserDetail": {
				"schemaName": "GKIInstanceLicUserDetail",
				"entitySchemaName": "GKIInstanceLicUser",
				"filter": {
					"detailColumn": "GKILicSyncSource",
					"masterColumn": "GKILicSyncSource"
				},
				"filterMethod": "GKIInstanceLicUserDetailFilter"
			}
		}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods: {

			/**
			 * @protected
			 * @desc фильтр детали
			 * returns {Object} filters
			 */
			GKIInstanceLicUserDetailFilter: function() {
				const filters = Terrasoft.createFilterGroup();
				filters.add("CreatedOnFilter", Terrasoft.createColumnFilterWithParameter(
					Terrasoft.ComparisonType.GREATER, "CreatedOn", this.get("GKIDatetimeFrom")));
				return filters;
			}
		},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "merge",
				"name": "Button-be6148b819154a0791eaee8f1635d859",
				"values": {
					"enabled": true
				}
			},
			{
				"operation": "merge",
				"name": "Button-3a8ac667899d4aa68021a07eb1c7c49c",
				"values": {
					"style": "default",
					"enabled": true
				}
			},
			{
				"operation": "insert",
				"name": "GKIInstanceLicUserDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail",
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 0
					}
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 0
			},
		]/**SCHEMA_DIFF*/
	};
});
