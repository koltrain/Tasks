define("GKILicUserPage", ["ServiceHelper", "ProcessModuleUtilities"], function(ServiceHelper, ProcessModuleUtilities) {
	return {
		entitySchemaName: "GKILicUser",
		attributes: {},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		messages: {
			"GKILicUserSyncCombinedMessage": {
				mode: Terrasoft.MessageMode.PTP,
				direction: Terrasoft.MessageDirectionType.SUBSCRIBE
			}
		},
		details: /**SCHEMA_DETAILS*/{
			"Files": {
				"schemaName": "FileDetailV2",
				"entitySchemaName": "GKILicUserFile",
				"filter": {
					"masterColumn": "Id",
					"detailColumn": "GKILicUser"
				}
			},
			"GKILicUserInstanceLicPackageDetail": {
				"schemaName": "GKILicUserInstanceLicPackageDetail",
				"entitySchemaName": "GKILicUserInstanceLicPackage",
				"filter": {
					"detailColumn": "GKILicUser",
					"masterColumn": "Id"
				}
                //"filterMethod": "getGKILicUserInstanceLicPackageDetailFilter"
			},
			"GKIInstanceLicUserDetail": {
				"schemaName": "GKIInstanceLicUserDetail",
				"entitySchemaName": "GKIInstanceLicUser",
				"filter": {
					"detailColumn": "GKILicUser",
					"masterColumn": "Id"
				}
			},
			"GKILicPackageUserDetail":{
				"schemaName": "GKILicPackageDetailWithEdit",
				"entitySchemaName": "GKILicPackageUser",
				"filter": {
					"detailColumn": "GKILicUser",
					"masterColumn": "Id"
				}
			}
		}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods: {
			/**
			 * @overridden
			 */
			subscribeSandboxEvents: function() {
				this.callParent(arguments);
				this.sandbox.subscribe("GKILicUserSyncCombinedMessage", function(args) {
					this.onGKILicUserSyncButtonClick();
				}, this, [this.sandbox.id.substring(0, 
					this.sandbox.id.indexOf("_CardModuleV2") < 1 ? 
					this.sandbox.id.length : this.sandbox.id.indexOf("_CardModuleV2"))]);
			},

			/**
			 * @public
			 * @desc вызывает процесс обновления лицензий для одного пользователя
			 */
			onGKILicUserSyncButtonClick: function() {
				var args = {
					name: "GKILicensingLicUserSyncFilterProcess",
					parameters: {
						licUserId: this.get("Id")
					}
				};
				ProcessModuleUtilities.startBusinessProcess(args);
				this.showInformationDialog(this.get("Resources.Strings.GKISyncHasStarted"));
				return;
			},

			// getGKILicUserInstanceLicPackageDetailFilter: function() {
			// 	const licStatusActive = "672f84f8-45de-4383-8220-a805b30b745e";
			// 	var UserId = this.get("Id");
			// 	const filters = Terrasoft.createFilterGroup();
			// 	filters.add("UserFilter", Terrasoft.createColumnFilterWithParameter(
			// 		"GKILicUser", UserId));
            // 	filters.add("detailColumnFilter", Terrasoft.createColumnFilterWithParameter(
			// 	 	"GKIInstance.[GKIInstanceLicense:GKIInstance].GKILic.GKILicStatus", licStatusActive));
			// 	return filters;
				
			// }
		},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "GKILicUserSyncButton",
				"values": {
					"itemType": 5,
					"style": "default",
					"caption": {
						"bindTo": "Resources.Strings.GKILicUserSyncButtonCaption"
					},
					"click": {
						"bindTo": "onGKILicUserSyncButtonClick"
					},
					"visible": true,
					"enabled": true,
					"classes": {
						"textClass": [
							"actions-button-margin-right"
						],
						"wrapperClass": [ 
							"actions-button-margin-right"
						]
					}
				},
				"parentName": "LeftContainer",
				"propertyName": "items",
			},
			{
				"operation": "insert",
				"name": "LicensesTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.LicensesTabCaption"
					},
					"items": [],
					"order": 0
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKILicPackageUserDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "LicensesTab",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKILicUserInstanceLicPackageDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "LicensesTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "GKIInstanceLicUserDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "LicensesTab",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "NotesAndFilesTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.NotesAndFilesTabCaption"
					},
					"items": [],
					"order": 1
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Files",
				"values": {
					"itemType": 2
				},
				"parentName": "NotesAndFilesTab",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "NotesControlGroup",
				"values": {
					"itemType": 15,
					"caption": {
						"bindTo": "Resources.Strings.NotesGroupCaption"
					},
					"items": []
				},
				"parentName": "NotesAndFilesTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Notes",
				"values": {
					"bindTo": "GKINotes",
					"dataValueType": 1,
					"contentType": 4,
					"layout": {
						"column": 0,
						"row": 0,
						"colSpan": 24
					},
					"labelConfig": {
						"visible": false
					},
					"controlConfig": {
						"imageLoaded": {
							"bindTo": "insertImagesToNotes"
						},
						"images": {
							"bindTo": "NotesImagesCollection"
						}
					}
				},
				"parentName": "NotesControlGroup",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIMSADFullName",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 0,
						"layoutName": "Header"
					},
					"bindTo": "GKIMSADFullName"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIMSADLogin",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 1,
						"layoutName": "Header"
					},
					"bindTo": "GKIMSADLogin",
					"enabled": false
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "GKIName",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 1,
						"layoutName": "Header"
					},
					"bindTo": "GKIName",
					"enabled": false
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "GKIMSADEMail",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 2,
						"layoutName": "Header"
					},
					"bindTo": "GKIMSADEMail"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "GKIMSADJobTitle",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 2,
						"layoutName": "Header"
					},
					"bindTo": "GKIMSADJobTitle"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "GKIMSADPhone",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 3,
						"layoutName": "Header"
					},
					"bindTo": "GKIMSADPhone"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "insert",
				"name": "GKIPlatformActive",
				"values": {
					"layout": {
						"colSpan": 8,
						"rowSpan": 1,
						"column": 0,
						"row": 5,
						"layoutName": "Header"
					},
					"bindTo": "GKIPlatformActive",
					"enabled": false
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 6
			},
			{
				"operation": "insert",
				"name": "GKIActive",
				"values": {
					"layout": {
						"colSpan": 8,
						"rowSpan": 1,
						"column": 8,
						"row": 5,
						"layoutName": "Header"
					},
					"bindTo": "GKIActive",
					"enabled": false
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 7
			},
			{
				"operation": "insert",
				"name": "GKIMSADActive",
				"values": {
					"layout": {
						"colSpan": 8,
						"rowSpan": 1,
						"column": 16,
						"row": 5,
						"layoutName": "Header"
					},
					"bindTo": "GKIMSADActive",
					"enabled": false
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 8
			},
			{
				"operation": "merge",
				"name": "ESNTab",
				"values": {
					"order": 2
				}
			},
		]/**SCHEMA_DIFF*/
	};
});
