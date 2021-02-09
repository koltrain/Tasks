define("GKILicenseGetUsersToDeactivateProcessPage", ["GKILicensingButtonsMixin"], function() {
	return {
		entitySchemaName: "",
		attributes: {},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{
			"GKILicUserInstanceLicPackageDetail": {
				"schemaName": "GKILicUserInstanceLicPackageDeactivationOnlyDetail",
				"entitySchemaName": "GKILicUserInstanceLicPackage",
				"filter": {
					"detailColumn": "GKIDeactivatedBySync",
					"masterColumn": "GKIDeactivatedBySync"
				}
			}
		}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods: {},
		mixins: {
			GKILicensingButtonsMixin: "Terrasoft.GKILicensingButtonsMixin"
		},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "Button-6321f299d7b24c6db85632351de37155",
				"values": {
					"itemType": 5,
					"id": "96259935-683f-4566-bcce-e27ae65c8441",
					"style": "green",
					"tag": "Continue",
					"caption": {
						"bindTo": "getProcessActionButtonCaption"
					},
					"click": {
						"bindTo": "onProcessActionButtonClick"
					},
					"enabled": true
				},
				"parentName": "ProcessActionButtons",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKILicUserSyncButton",
				"parentName": "ProcessActionButtons",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKILicUserSyncButtonCaption"},
					"click": {"bindTo": "onGKILicUserSyncButtonClick"},
					"visible": true,
					"enabled": true,
					"classes": {
						"textClass": ["actions-button-margin-right"],
						"wrapperClass": ["actions-button-margin-right"]
					},
				},
				"index": 2
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
				"name": "GKILicUserInstanceLicPackageDetail",
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
			{
				"operation": "remove",
				"name": "Button-be6148b819154a0791eaee8f1635d859"
			}
		]/**SCHEMA_DIFF*/
	};
});
