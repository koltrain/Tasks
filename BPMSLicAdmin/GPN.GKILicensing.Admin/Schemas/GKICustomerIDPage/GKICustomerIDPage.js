define("GKICustomerIDPage", ["ServiceHelper"], function(ServiceHelper) {
	return {
		entitySchemaName: "GKICustomerID",
		attributes: {
			"GKITlsInstallButtonEnabled": {
				"dataValueType": this.Terrasoft.DataValueType.BOOLEAN,
				"type": this.Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				"value": false,
			},
			"GKITlsDownloadFixAttempt": {
				"dataValueType": this.Terrasoft.DataValueType.BOOLEAN,
				"type": this.Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				"value": false,
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
			 * @public
			 * @desc enabled и visible кнопок
			 */
			GKIButtonsValidating: function() {
				this.onGKITlsInstallButtonEnabled();
				this.GKIPublishAttributesMsg();
			},

			/**
			 * @public
			 * @desc отправляет значения атрибутов в раздел
			 */
			GKIPublishAttributesMsg: function() {
				this.sandbox.publish("GKIPublishAttributesMsg", [
					{
						name: "GKITlsInstallButtonEnabled",
						value: this.get("GKITlsInstallButtonEnabled")
					},
				], [this.sandbox.id]);
			},

			/**
			 * @public
			 * @desc формирование запроса на tlr-файл
			 */
			onGKITlrRequestButtonClick: function() {
				this.showBodyMask();
				var serviceData = {
					recordId: this.get("Id")
				};
				ServiceHelper.callService("GKILicensingAdminService", "GKITlrRequest",  function(response) {
					this.hideBodyMask();
					var responseMessage = "";
					if (!response.GKITlrRequestResult) {
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
						responseMessage = response.GKITlrRequestResult;
					}
					this.showInformationDialog(responseMessage);
				}, serviceData, this);
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
			 * @public
			 * @desc доступность кнопки установки лицензий
			 */
			onGKITlsInstallButtonEnabled: function() {
				this.set("GKITlsInstallButtonEnabled", true); //доступность установки теперь проверяется после нажатия кнопки для каждого инстанса
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
					var errMsg = "";
					var startSign = "#ExceptionStart#";
					var finishSign = "#ExceptionEnd#";
					if (xhr.responseText.indexOf(startSign) > 0 && xhr.responseText.indexOf(finishSign) > 0){
						errMsg = xhr.responseText.substring(xhr.responseText.indexOf(startSign)+startSign.length, 
							xhr.responseText.indexOf(finishSign));
						switch (errMsg) {
							case "Wrong CustomerId":
								errMsg = this.get("Resources.Strings.GKIWrongCustomerId");
								break;
							case "No suitable instances":
								errMsg = this.get("Resources.Strings.GKINoSuitableInstances");
								break;
							default:
								break;
						}
					}
					else{
						errMsg = error;
					}
					this.showInformationDialog(errMsg);
					console.log(xhr.responseText);
				}
				else{
					this.showInformationDialog(this.get("Resources.Strings.GKIFileHasBeenLoaded"));
				}
			},
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
					"enabled": true,
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
					"enabled": true,
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
					"enabled": {"bindTo": "GKITlsInstallButtonEnabled"},
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
