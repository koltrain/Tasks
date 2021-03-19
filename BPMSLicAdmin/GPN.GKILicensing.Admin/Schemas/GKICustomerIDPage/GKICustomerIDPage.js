define("GKICustomerIDPage", ["ServiceHelper", "ProcessModuleUtilities", "RightUtilities"],
	function(ServiceHelper, ProcessModuleUtilities, RightUtilities) {
	return {
		entitySchemaName: "GKICustomerID",
		attributes: {
			"GKITlsDownloadFixAttempt": {
				"dataValueType": this.Terrasoft.DataValueType.BOOLEAN,
				"type": this.Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				"value": false,
			},
			"isGKIButtonsEnabled": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: false
			},
		},
		messages: {
			"GKISectionButtonMessage": {
				mode: Terrasoft.MessageMode.PTP,
				direction: Terrasoft.MessageDirectionType.SUBSCRIBE
			},
			"GKIPublishAttributesMsg": {
				mode: Terrasoft.MessageMode.PTP,
				direction: Terrasoft.MessageDirectionType.PUBLISH
			}
		},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{
			"Files": {
				"schemaName": "FileDetailV2",
				"entitySchemaName": "GKICustomerIDFile",
				"filter": {
					"masterColumn": "Id",
					"detailColumn": "GKICustomerID"
				}
			},
			"GKILicDetail": {
				"schemaName": "GKILicDetail",
				"entitySchemaName": "GKILic",
				"filter": {
					"detailColumn": "GKICustomerID",
					"masterColumn": "Id"
				}
			},
			"GKIInstanceDetail": {
                "schemaName": "GKIInstanceDetail",
                "entitySchemaName": "GKIInstance",
                "filter": {
                    "detailColumn": "GKICustomerID",
                    "masterColumn": "Id"
                }
            }
		}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods: {
			/**
			 * @overridden
			 */
			onEntityInitialized: function() {
				this.callParent(arguments);
				this.GKIButtonsValidating();
			},
			
			/**
			 * @overridden
			 */
			subscribeSandboxEvents: function() {
				this.callParent(arguments);
				this.sandbox.subscribe("GKISectionButtonMessage", function(args) {
					switch (args.buttonName) {
						case "GKITlrRequestButton":
							this.onGKITlrRequestButtonClick();
							break;
						case "GKITlsDownloadButton":
							this.onGKITlsDownloadButtonFilesLoaded(args.files);
							break;
						case "GKITlsInstallButton":
							this.onGKITlsInstallButtonClick();
							break;
						default:
							break;
					}
				}, this, [this.sandbox.id.substring(0, 
					this.sandbox.id.indexOf("_CardModuleV2") < 1 ? 
					this.sandbox.id.length : this.sandbox.id.indexOf("_CardModuleV2"))]);
			},

			/**
			 * @overridden
			 */
			updateButtonsVisibility: function() {
				this.callParent(arguments);
				this.GKIShowCloseButton();
			},

			/**
			 * @public
			 * @desc enabled и visible кнопок
			 */
			GKIButtonsValidating: function() {
				this.isGKIButtonsEnabledMethod(this.GKIPublishAttributesMsg, this);
			},

			/**
			 * @public
			 * @desc отправляет значения атрибутов в раздел
			 */
			GKIPublishAttributesMsg: function() {
				this.sandbox.publish("GKIPublishAttributesMsg", [
					{
						name: "isGKIButtonsEnabled",
						value: this.get("isGKIButtonsEnabled")
					},
				], [this.sandbox.id]);
			},

			/**
			 * @public
			 * @desc формирование запроса на tlr-файл
			 */
			onGKITlrRequestButtonClick: function() {
				var args = {
					name: "GKITlrRequestProcess",
					parameters: {
						CustomerId: this.get("Id")
					}
				};
				ProcessModuleUtilities.startBusinessProcess(args);
				this.showInformationDialog(this.get("Resources.Strings.GKITlrRequestProcessReminder"));
				return;
			},
			
			/**
			* @public
			* @desc загрузка tls-файла
			*/
			onGKITlsDownloadButtonFilesLoaded: function(files) {
				if (files.length <= 0) {
					return;
				}
				if (files.length > 1) {
					this.showInformationDialog(this.get("Resources.Strings.GKITooManyFilesSelected"));
					return;
				}
				if (files[0].name.substring(files[0].name.length-4) !== ".tls") {
					this.showInformationDialog(this.get("Resources.Strings.GKIWrongFileFormat"));
					return;
				}
				var configPath = "GKILicensingAdminService/GKIImportTlsFile";
				//do not delete "Filler" data, it is just a filler, so an error won't be invoked
				var config = Ext.apply(this.GKIgetFileUploadConfig(files), {
					uploadWebServicePath: configPath,
					columnName: "Filler",
					parentColumnName: "Filler",
					parentColumnValue: this.get("Id")
				});
				this.Terrasoft.ConfigurationFileApi.upload(config, function() {
					this.reloadEntity();
				});
		   	},

		   /**
			 * @public
			 * @desc установка файла tls на стенды
			 */
			onGKITlsInstallButtonClick: function() {
				this.showBodyMask();
				var serviceData = {
					recordId: this.get("Id")
				};
				ServiceHelper.callService("GKILicensingAdminService", "GKITlsInstall",  function(response) {
					this.hideBodyMask();
					var responseMessage = "";
					if (!response.GKITlsInstallResult) {
						if (response.responseText) {
							console.log(response.responseText);
						}
						if (response.message){
							responseMessage = response.message;
						}
						else if (response.statusText) {
							responseMessage = response.statusText;
						}
						else {
							responseMessage = "Unknown error";
						}
						return;
					}
					else {
						responseMessage = response.GKITlsInstallResult;
					}
					this.showInformationDialog(responseMessage);
				}, serviceData, this);
			},
			/**
			 * @private
			 * @desc returns file config
			 */
			GKIgetFileUploadConfig: function(file) {
				return {
					scope: this,
					onUpload: Terrasoft.emptyFn,
					onComplete: Terrasoft.emptyFn,
					onFileComplete: this.onFileComplete,
					files: [file],
					fileName: file.name,
					isChunkedUpload: false
				};
			},
			/**
			 * @public
			 * @desc file methods
			 */
			onFileComplete: function(error, xhr, file, options) {
				if (error) {
					if (error.indexOf("FileExtensionsDenyList") > 0 && !this.get("GKITlsDownloadFixAttempt")){
						ServiceHelper.callService("GKILicensingAdminService", "GKITlsDownloadEnable",  function(response) {
							if (!response.GKITlsDownloadEnableResult) {
								this.showInformationDialog(error);
								this.set("GKITlsDownloadFixAttempt", true);
							}
							else {
								this.onGKITlsDownloadButtonFilesLoaded([file]);
							}
							return;
						}, null, this);
					}
				}
				if (xhr.responseText && xhr.responseText !== "\"\""){
					var errMsg;
					switch (xhr.responseText) {
						case "\"Wrong file content\"":
							errMsg = this.get("Resources.Strings.GKIWrongFileFormat");
							break;
						case "\"No suitable instances\"":
							errMsg = this.get("Resources.Strings.GKINoSuitableInstances");
							break;
						default:
							errMsg = xhr.responseText;
					}
					this.showInformationDialog(errMsg);
				}
				else{
					this.showInformationDialog(this.get("Resources.Strings.GKIFileHasBeenLoaded"));
				}
			},
			/**
			 * @private
			 * @desc определение видимости кнопок по правам
			 */
			isGKIButtonsEnabledMethod: function(callback, scope) {
				RightUtilities.checkCanExecuteOperation({
					operation: "GKICanManageLicensingSettings"
				}, function(result) {
					this.set("isGKIButtonsEnabled", true);
					if (callback){
						Ext.callback(callback, scope || this);
					}
				}, this);
			},
			/**
			 * @private
			 * @desc определение видимости кнопки закрытия
			 */
			GKIShowCloseButton: function() {
				this.set("ShowCloseButton", false);
				var esq = this.Ext.create("Terrasoft.EntitySchemaQuery", {
					rootSchemaName: "GKICustomerID"
				});
				esq.addColumn("Id");
				esq.getEntityCollection(function(response) {
					if (response.success && response.collection.getItems().length === 1 
						&& response.collection.getItems()[0]) {
							this.set("ShowCloseButton", false);
					}
					else{
						this.set("ShowCloseButton", true);
					}
				}, this);
			}
		},
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "GKITlrRequestButton",
				"parentName": "LeftContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKITlrRequestButtonCaption"},
					"click": {"bindTo": "onGKITlrRequestButtonClick"},
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
				"name": "GKITlsDownloadButton",
				"parentName": "LeftContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKITlsDownloadButtonCaption"},
					"fileUpload": true,
					"filesSelected": {
						"bindTo": "onGKITlsDownloadButtonFilesLoaded"
					},
					"fileTypeFilter": [".tls"],
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
				"name": "GKITlsInstallButton",
				"parentName": "LeftContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKITlsInstallButtonCaption"},
					"click": {"bindTo": "onGKITlsInstallButtonClick"},
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
				"name": "Name",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 0,
						"layoutName": "Header"
					},
					"bindTo": "Name"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 0,
				"name": "NotesAndFilesTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.NotesAndFilesTabCaption"
					},
					"items": []
				}
			},
			{
				"operation": "insert",
				"parentName": "NotesAndFilesTab",
				"propertyName": "items",
				"name": "Files",
				"values": {
					"itemType": Terrasoft.ViewItemType.DETAIL
				}
			},
			{
				"operation": "insert",
				"parentName": "NotesAndFilesTab",
				"propertyName": "items",
				"name": "NotesControlGroup",
				"values": {
					"itemType": Terrasoft.ViewItemType.CONTROL_GROUP,
					"caption": {
						"bindTo": "Resources.Strings.NotesGroupCaption"
					},
					"items": []
				}
			},
			{
				"operation": "insert",
				"parentName": "NotesControlGroup",
				"propertyName": "items",
				"name": "Notes",
				"values": {
					"bindTo": "GKINotes",
					"dataValueType": Terrasoft.DataValueType.TEXT,
					"contentType": Terrasoft.ContentType.RICH_TEXT,
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
				}
			},
			{
				"operation": "insert",
				"name": "GeneralInfoTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.GeneralInfoTabCaption"
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
				"name": "GKILicDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIInstanceDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 1
			},
		]/**SCHEMA_DIFF*/
	};
});
