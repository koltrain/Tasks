define("GKILicUserInstanceLicPackageDetailInfoButton", [], function() {
	return {
      	details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "InfoButton",
				"parentName": "Detail",
				"propertyName": "tools",
				"values": {
					"itemType": Terrasoft.ViewItemType.INFORMATION_BUTTON,
					"content": { "bindTo": "Resources.Strings.InfoButtonOverride" }
				}
			}
        ]/**SCHEMA_DIFF*/
	};
}); 