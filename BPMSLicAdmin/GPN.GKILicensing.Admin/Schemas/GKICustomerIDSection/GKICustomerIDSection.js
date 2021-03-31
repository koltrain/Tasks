define("GKICustomerIDSection", ["RightUtilities"], function(RightUtilities) {
	return {
		entitySchemaName: "GKICustomerID",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		attributes: {
			"isGKIButtonsEnabled": {
				"dataValueType": this.Terrasoft.DataValueType.BOOLEAN,
				"type": this.Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				"value": false,
			},
		},
		messages: {
			"GKISectionButtonMessage": {
				mode: Terrasoft.MessageMode.PTP,
				direction: Terrasoft.MessageDirectionType.PUBLISH
			},
			"GKIPublishAttributesMsg": {
				mode: Terrasoft.MessageMode.PTP,
				direction: Terrasoft.MessageDirectionType.SUBSCRIBE
			}
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
			},
			{
				"operation": "insert",
				"name": "GKITlrRequestButton",
				"parentName": "CombinedModeActionButtonsCardLeftContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKITlrRequestButtonCaption"},
					"click": {"bindTo": "onGKISectionButtonClick"},
					"tag": "GKITlrRequestButton",
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
				"parentName": "CombinedModeActionButtonsCardLeftContainer",
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
				"parentName": "CombinedModeActionButtonsCardLeftContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKITlsInstallButtonCaption"},
					"click": {"bindTo": "onGKISectionButtonClick"},
					"tag": "GKITlsInstallButton",
					"visible": true,
					"enabled": {"bindTo": "isGKIButtonsEnabled"},
					"classes": {
						"textClass": ["actions-button-margin-right"],
						"wrapperClass": ["actions-button-margin-right"]
					},
				},
			}
		]/**SCHEMA_DIFF*/,
		methods: {
			/**
			 * @overridden
			 * @desc открывает страницу раздела, если она одна
			 */
			init: function() {
				var parentMethod = this.getParentMethod();
				var args = arguments;
				var esq = this.Ext.create("Terrasoft.EntitySchemaQuery", {
					rootSchemaName: "GKICustomerID"
				});
				esq.addColumn("Id");
				esq.getEntityCollection(function(response) {
					if (response.success && response.collection.getItems().length === 1 
						&& response.collection.getItems()[0]) {
						this.sandbox.publish("PushHistoryState", {
							hash: "CardModuleV2/GKICustomerIDPage/edit/" + 
								response.collection.getItems()[0].get("Id")
						});
					}
					else{
						parentMethod.apply(this, args);
					}
				}, this);
			},
			
			/**
			 * @overridden
			 * @desc подписка на сообщения
			 */
			subscribeSandboxEvents: function() {
				this.callParent(arguments);
				this.sandbox.subscribe("GKIPublishAttributesMsg", function(args) {
					this.GKIUpdateAttributeFromPage(args);
				}, this, [this.sandbox.id + "_CardModuleV2"]);
			},

			/**
			 * @overridden
			 * @desc удаление "Удалить" в меню "Действия"
			 */
			isVisibleDeleteAction: Terrasoft.emptyFn,

			/**
			 * 
			 * @public
			 * @desc синхронизирует атрибуты с карточкой 
			 */
			GKIUpdateAttributeFromPage: function(data) {
				data.forEach(function(item) {
					this.set(item.name, item.value);
				}, this);
			},
			
			/**
			 * @private
			 * @desc нажатие кнопки. тег = название функции в page
			 */
			onGKISectionButtonClick: function() {
				if (arguments && arguments.length) {
					var tag = arguments[arguments.length - 1];
					this.sandbox.publish("GKISectionButtonMessage", {buttonName: tag}, [this.sandbox.id]);
				}
			},

			/**
			 * @public
			 * @desc передача файла(ов) в карточку после загрузки
			 */
			onGKITlsDownloadButtonFilesLoaded: function(files) {
				this.sandbox.publish("GKISectionButtonMessage", {buttonName: "GKITlsDownloadButton", files: files}, [this.sandbox.id]);
			},
		}
	};
});
