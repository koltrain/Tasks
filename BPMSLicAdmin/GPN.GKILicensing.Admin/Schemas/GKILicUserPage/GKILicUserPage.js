define("GKILicUserPage", ["ServiceHelper", "ProcessModuleUtilities", "RightUtilities"], 
	function(ServiceHelper, ProcessModuleUtilities, RightUtilities) {
	return {
		entitySchemaName: "GKILicUser",
		attributes: {
			"isGKIButtonsEnabled": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: false
			},
		},
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
			//BPMS-203 скрыть деталь до выяснения необходимости ручного ввода
			/*"GKILicPackageUserDetail": {
				"schemaName": "GKILicPackageDetailWithEdit",
				"entitySchemaName": "GKILicPackageUser",
				"filter": {
					"detailColumn": "GKILicUser",
					"masterColumn": "Id"
				}
			}*/
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
			 * @overridden
			 */
			init: function() {
				this.callParent(arguments);
				this.isGKIButtonsEnabledMethod();
			},

			/**
			 * @public
			 * @desc вызывает процесс обновления лицензий для одного пользователя
			 */
			onGKILicUserSyncButtonClick: function() {
				var args = {
					sysProcessName: "GKILicensingLicUserSyncFilterProcess",
					parameters: {
						licUserId: this.get("Id")
					}
				};
				ProcessModuleUtilities.executeProcess(args);
				this.reloadEntity();
				this.showInformationDialog(this.get("Resources.Strings.GKISyncHasStarted"));
				return;
			},
			/**
			 * @private
			 * @desc определение видимости кнопок по правам
			 */
			isGKIButtonsEnabledMethod: function() {
				RightUtilities.checkCanExecuteOperation({
					operation: "GKICanManageLicensingSettings"
				}, function(result) {
					this.set("isGKIButtonsEnabled", result);
				}, this);
			}
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
				"propertyName": "items",
				"parentName": "LeftContainer",
				"values": {
					"hint": { "bindTo": "Resources.Strings.LicUserSyncButtonHint" },
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": { "bindTo": "Resources.Strings.GKILicUserSyncButtonCaption" },
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"click": { "bindTo": "onGKILicUserSyncButtonClick" },
					"visible": true,
					"enabled": {"bindTo": "isGKIButtonsEnabled"},
					"classes": {
						"textClass": ["actions-button-margin-right"],
						"wrapperClass": ["actions-button-margin-right"]
					},
				},
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
						"layoutName": "ProfileContainer"
					},
					"bindTo": "GKIMSADFullName",
					"enabled": false
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIName",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 1,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "GKIName",
					"enabled": false
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "GKIMSADPhone",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 2,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "GKIMSADPhone"
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "GKIMSADEMail",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 3,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "GKIMSADEMail"
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "GKIMSADJobTitle",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 4,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "GKIMSADJobTitle"
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "GKIPlatformActive",
				"values": {
                  	"hint" : {"bindTo" : "Resources.Strings.PlatformActiveHint"},
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 5,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "GKIPlatformActive",
					"enabled": false
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "insert",
				"name": "GKIActive",
				"values": {
                  	"hint" : {"bindTo" : "Resources.Strings.ActiveHint"},
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 6,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "GKIActive",
					"enabled": false
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 6
			},
			{
				"operation": "insert",
				"name": "GKIMSADActive",
				"values": {
                  	"hint" : {"bindTo" : "Resources.Strings.MSADActiveHint"},
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 7,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "GKIMSADActive",
					"enabled": false
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 7
			},
			{
				"operation": "insert",
				"name": "GKIIsVIP",
				"values": {
                  	"hint" : {"bindTo" : "Resources.Strings.IsVIPHint"},
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 8,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "GKIIsVIP",
					"enabled": false
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 8
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
			//BPMS-203 скрыть деталь до выяснения необходимости ручного ввода
			/*
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
			*/
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
			}
		]/**SCHEMA_DIFF*/
	};
});
