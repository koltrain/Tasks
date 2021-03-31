define("GKIGroupADInstanceLicensePage", [], function() {
	return {
		entitySchemaName: "GKIGroupADInstanceLicense",
		attributes: {
			"GKIGroupAD": {
				"dataValueType": Terrasoft.DataValueType.LOOKUP,
				"lookupListConfig": {
					"filter": function () {
						var filterGroup = Terrasoft.createFilterGroup();

						var detailInfo = this.sandbox.publish("GetDetailInfo", null, [this.sandbox.id]) || {};

						var esq = Ext.create("Terrasoft.EntitySchemaQuery", {
							rootSchemaName: "GKIInstanceGroupAD"
						});

						esq.addColumn("GKIGroupAD");
						esq.addColumn("GKIInstance");

						esq.filters.add("GKIInstanceIdFilter", Terrasoft.createColumnFilterWithParameter(
							Terrasoft.ComparisonType.EQUAL, "GKIInstance", detailInfo.masterRecordId
						));

						esq.filters.add("GKIGroupADIsNotNullFilter", Terrasoft.createIsNotNullFilter("GKIGroupAD"));

						esq.getEntityCollection(function (result) {
							var selectedIds = [];

							if (result.success) {
								result.collection.each(function (item) {
									selectedIds.push(item.get("GKIGroupAD").value);
								});
							}

							if (selectedIds[0] != undefined)
								filterGroup.add("GKIGroupADIds", Terrasoft.createColumnInFilterWithParameters("Id", selectedIds));
							else
								filterGroup.add("GKIGroupADIds", Terrasoft.createColumnIsNullFilter("Id"));
						}, this);

						return filterGroup;
                    }
                }
			},
			"GKILicPackage": {
				"dataValueType": Terrasoft.DataValueType.LOOKUP,
				"lookupListConfig": {
					"filter": function () {
						var filterGroup = Terrasoft.createFilterGroup();

						var detailInfo = this.sandbox.publish("GetDetailInfo", null, [this.sandbox.id]) || {};

						var esq = Ext.create("Terrasoft.EntitySchemaQuery", {
							rootSchemaName: "GKIInstanceLicense"
						});

						esq.addColumn("GKILic.GKILicPackage", "GKILicPackage");
						esq.addColumn("GKILic.GKILicStatus", "GKILicStatus");
						esq.addColumn("GKILic.GKILicType", "GKILicType");

						esq.filters.add("GKIInstanceIdFilter", Terrasoft.createColumnFilterWithParameter(
							Terrasoft.ComparisonType.EQUAL, "GKIInstance", detailInfo.masterRecordId
						));

						esq.filters.add("GKILicIsNotNullFilter", Terrasoft.createIsNotNullFilter("GKILic"));

						esq.filters.add("GKILicStatusFilter", Terrasoft.createColumnFilterWithParameter(
							Terrasoft.ComparisonType.EQUAL, "GKILic.GKILicStatus", "672f84f8-45de-4383-8220-a805b30b745e"
						));

						esq.filters.add("GKILicTypeFilter", Terrasoft.createColumnFilterWithParameter(
							Terrasoft.ComparisonType.EQUAL, "GKILic.GKILicType", "7cb7fff2-f050-41e0-b277-e2e1674c86a4"
						));

						esq.getEntityCollection(function (result) {
							var selectedIds = [];

							if (result.success) {
								result.collection.each(function (item) {
									selectedIds.push(item.get("GKILicPackage").value);
								});
							}

							if (selectedIds[0] != undefined)
								filterGroup.add("GKILicPackageIds", Terrasoft.createColumnInFilterWithParameters("Id", selectedIds));
							else
								filterGroup.add("GKILicPackageIds", Terrasoft.createColumnIsNullFilter("Id"));
						}, this);

						return filterGroup;
					}
				}
			}
		},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods: {},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[]/**SCHEMA_DIFF*/
	};
});
