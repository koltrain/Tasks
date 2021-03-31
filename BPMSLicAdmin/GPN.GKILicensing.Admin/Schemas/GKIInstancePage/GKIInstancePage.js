define("GKIInstancePage", ["ServiceHelper", "GKILicensingConstantsJs"], function (ServiceHelper, constants) {
	return {
		entitySchemaName: "GKIInstance",
		attributes: {
			"GKIInsertPasswordVisible": {
				"dataValueType": this.Terrasoft.DataValueType.BOOLEAN,
				"value": false
			},
		},
		messages: {
			"GKIInstanceUpdateRow": {
				mode: Terrasoft.MessageMode.PTP,
				direction: Terrasoft.MessageDirectionType.PUBLISH
			}
		},
		modules: /**SCHEMA_MODULES*/{
			"Indicator81ddd525-e83e-4dff-bf03-a9adba819e86": {
				"moduleId": "Indicator81ddd525-e83e-4dff-bf03-a9adba819e86",
				"moduleName": "CardWidgetModule",
				"config": {
					"parameters": {
						"viewModelConfig": {
							"widgetKey": "Indicator81ddd525-e83e-4dff-bf03-a9adba819e86",
							"recordId": "971c0d63-3e46-4edd-98aa-4e29688416b4",
							"primaryColumnValue": {
								"getValueMethod": "getPrimaryColumnValue"
							}
						}
					}
				}
			},
			"Indicatorb1d303da-be3c-49ed-843b-c9939571e083": {
				"moduleId": "Indicatorb1d303da-be3c-49ed-843b-c9939571e083",
				"moduleName": "CardWidgetModule",
				"config": {
					"parameters": {
						"viewModelConfig": {
							"widgetKey": "Indicatorb1d303da-be3c-49ed-843b-c9939571e083",
							"recordId": "971c0d63-3e46-4edd-98aa-4e29688416b4",
							"primaryColumnValue": {
								"getValueMethod": "getPrimaryColumnValue"
							}
						}
					}
				}
			},
			"Indicator5b260bc0-9bb0-4a51-8cad-7cd0c7a790d8": {
				"moduleId": "Indicator5b260bc0-9bb0-4a51-8cad-7cd0c7a790d8",
				"moduleName": "CardWidgetModule",
				"config": {
					"parameters": {
						"viewModelConfig": {
							"widgetKey": "Indicator5b260bc0-9bb0-4a51-8cad-7cd0c7a790d8",
							"recordId": "971c0d63-3e46-4edd-98aa-4e29688416b4",
							"primaryColumnValue": {
								"getValueMethod": "getPrimaryColumnValue"
							}
						}
					}
				}
			}
		}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{
			"Files": {
				"schemaName": "FileDetailV2",
				"entitySchemaName": "GKIInstanceFile",
				"filter": {
					"masterColumn": "Id",
					"detailColumn": "GKIInstance"
				}
			},
			"GKIInstanceLicUserDetail": {
				"schemaName": "GKIInstanceLicUserDetail",
				"entitySchemaName": "GKIInstanceLicUser",
				"filter": {
					"detailColumn": "GKIInstance",
					"masterColumn": "Id"
				}
			},
			"GKIInstanceLicenseDetail": {
				"schemaName": "GKIInstanceLicenseDetail",
				"entitySchemaName": "GKIInstanceLicense",
				"filter": {
					"detailColumn": "GKIInstance",
					"masterColumn": "Id"
				},
				"filterMethod": "GKIInstanceLicenseDetailFilter"
			},
			"GKIInstanceGroupADDetail": {
				"schemaName": "GKIInstanceGroupADDetail",
				"entitySchemaName": "GKIInstanceGroupAD",
				"filter": {
					"detailColumn": "GKIInstance",
					"masterColumn": "Id"
				}
			},
			"GKIGroupADUsersDetail": {
				"schemaName": "GKIGroupADUsersDetail",
				"entitySchemaName": "GKIGroupADUsers",
				"filter": {
					"detailColumn": "GKIInstance",
					"masterColumn": "Id"
				}
			},
			"GKIGroupADInstanceLicenseDetail": {
				"schemaName": "GKIGroupADInstanceLicenseDetail",
				"entitySchemaName": "GKIGroupADInstanceLicense",
				"filter": {
					"detailColumn": "GKIInstance",
					"masterColumn": "Id"
				}
			}
		}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods: {
			/**
			 * @overridden
			 * @desc действия при инициализации
			 */
			init: function () {
				this.callParent(arguments);
				this.sandbox.registerMessages(this.messages);
			},

			/**
			 * Закрытие карточки без сохранения деталей
			 * @overridden
			 */
			onCloseClick: function() {
				this.onCloseCardButtonClick();
			},

			/**
			 * @public
			 * @desc Фильтр детали GKIInstanceLicenseDetail
			 */
			GKIInstanceLicenseDetailFilter: function () {
				var filterGroup = Terrasoft.createFilterGroup();

				filterGroup.add("GKILicTypeFilter", Terrasoft.createColumnFilterWithParameter(
					Terrasoft.ComparisonType.EQUAL, "GKILic.GKILicType", constants.GKILicType.Personal
				));
				filterGroup.add("GKILicStatusFilter", Terrasoft.createColumnFilterWithParameter(
					Terrasoft.ComparisonType.EQUAL, "GKILic.GKILicStatus", constants.GKILicStatus.Active
				));

				filterGroup.add("GKIInstanceIdFilter", Terrasoft.createColumnFilterWithParameter(
					Terrasoft.ComparisonType.EQUAL, "GKIInstance", this.get("Id")
				));

				return filterGroup;
			},

			/**
			 * @overridden
			 * действия после инициализации карточки
			 */
			onEntityInitialized: function () {
				this.callParent(arguments);
				if (!this.get("GKICustomerID")) {
					this.GKIsetCustomerID();
				}
				if (!this.get("GKIApplicationStatus")) {
					this.GKIsetGKIApplicationStatus();
				}
			},

			/**
			 * @overridden
			 * действия после сохранения карточки
			 */
			onSaved: function () {
				this.callParent(arguments);
				this.updateDetails();
			},

			/**
			 * @public
			 * @desc редактируемость поля Идентификатор клиента
			 * returns {bool}
			 */
			isGKICustomerIDEnabledMethod: function () {
				if (!this.get("GKICustomerID")) {
					return true;
				}
				return false;
			},

			/**
			 * @private
			 * @desc устанавливает CustomerID в поле GKICustomerID, если существует только один CustomerID
			 */
			GKIsetCustomerID: function () {
				var esq = this.Ext.create("Terrasoft.EntitySchemaQuery", {
					rootSchemaName: "GKICustomerID"
				});
				esq.addColumn("Id");
				esq.addColumn("Name");
				esq.getEntityCollection(function (response) {
					if (response.success && response.collection.getItems().length === 1
						&& response.collection.getItems()[0]) {
						this.set("GKICustomerID", {
							value: response.collection.getItems()[0].get("Id"),
							displayValue: response.collection.getItems()[0].get("Name")
						});
					}
				}, this);
			},

			/**
			 * @private
			 * @desc устанавливает GKIApplicationStatus на "Новый", если он пустой
			 */
			GKIsetGKIApplicationStatus: function () {
				this.loadLookupDisplayValue("GKIApplicationStatus", constants.GKIApplicationStatus.New,
					null, this);
			},
			
			/**
			 * @protected
			 * @desc oбработка кнопки "Сохранить пароль"
			 */
			onGKISavePasswordButtonClick: function () {
				if (this.isAddMode()) {
					this.save({ isSilent: true, callback: this.onGKISavePasswordButtonClickCallback });
				}
				else {
					this.onGKISavePasswordButtonClickCallback();
				}
			},

			/**
			 * @protected
			 * @desc коллбек кнопки "Сохранить пароль" после сохранения
			 */
			onGKISavePasswordButtonClickCallback: function () {
				var newPasswordTextEdit = this.Ext.getCmp("new-password");
				var newPasswordInputValue = newPasswordTextEdit.getTypedValue();
				this.Ext.getCmp("new-password").setValue("");
				var serviceData = {
					instanceId: this.get("Id"),
					password: newPasswordInputValue,
				};
				var method = "GKINewInstancePassword";
				ServiceHelper.callService("GKILicensingAdminService", method, function (response) {
					if (!response.hasOwnProperty(method + "Result")) {
						var responseMessage = this.GKIServiceErrorsHandling(response);
						this.showInformationDialog(responseMessage);
						return;
					}
					else {
						this.set("GKIInsertPasswordVisible", false);
						this.showInformationDialog(response[method + "Result"] || this.get("Resources.Strings.GKISavePasswordSuccess"));
					}
				}, serviceData, this);
				this.sandbox.publish("GKIInstanceUpdateRow", null, ["GKIInstanceUpdateRow"]);
			},

			/**
			 * @public
			 * @desc oбработка ошибок сервиса
			 * @param {Object} response ответ сервиса
			 * @returns {String} сообщение ошибки
			 */
			GKIServiceErrorsHandling: function (response) {
				var responseMessage;
				if (response.responseText) {
					console.log(response.responseText);
				}
				if (response.message) {
					responseMessage = response.message;
				}
				else if (response.statusText) {
					responseMessage = response.statusText;
				}
				else {
					responseMessage = "Unknown error";
				}
				return responseMessage;
			},

			/**
			 * @protected
			 * @desc видимость кнопки "Изменить пароль"
			 * @param {bool} value значение атрибута GKIInsertPasswordVisible
			 */
			GKIChangePasswordButtonVisibleConverter: function (value) {
				return !value;
			},

			/**
			 * @protected
			 * @desc клик на кнопку "Изменить пароль"
			 */
			onGKIChangePasswordButtonClick: function () {
				this.set("GKIInsertPasswordVisible", true);
			},
		},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "Indicator81ddd525-e83e-4dff-bf03-a9adba819e86",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 4,
						"column": 0,
						"row": 0,
						"layoutName": "ProfileContainer",
						"useFixedColumnHeight": true
					},
					"itemType": 4,
					"classes": {
						"wrapClassName": [
							"card-widget-grid-layout-item"
						]
					}
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "Indicatorb1d303da-be3c-49ed-843b-c9939571e083",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 4,
						"column": 0,
						"row": 4,
						"layoutName": "ProfileContainer",
						"useFixedColumnHeight": true
					},
					"itemType": 4,
					"classes": {
						"wrapClassName": [
							"card-widget-grid-layout-item"
						]
					}
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Indicator5b260bc0-9bb0-4a51-8cad-7cd0c7a790d8",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 4,
						"column": 0,
						"row": 8,
						"layoutName": "ProfileContainer",
						"useFixedColumnHeight": true
					},
					"itemType": 4,
					"classes": {
						"wrapClassName": [
							"card-widget-grid-layout-item"
						]
					}
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "GKIName",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 0,
						"layoutName": "Header"
					},
					"bindTo": "GKIName"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIUrl",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 1,
						"layoutName": "Header"
					},
					"bindTo": "GKIUrl"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "GKIWinNodeAddress",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 2,
						"layoutName": "Header"
					},
					"bindTo": "GKIWinNodeAddress"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "insert",
				"name": "GKIVersion",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 3,
						"layoutName": "Header"
					},
					"bindTo": "GKIVersion",
					"enabled": true
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "GKIApplicationStatus",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 3,
						"layoutName": "Header"
					},
					"bindTo": "GKIApplicationStatus",
					"enabled": false,
					"contentType": 3
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "GKICustomerID",
				"values": {
					"hint": { "bindTo": "Resources.Strings.CustomerIDHint" },
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 4,
						"layoutName": "Header"
					},
					"bindTo": "GKICustomerID",
					"enabled": {
						"bindTo": "isGKICustomerIDEnabledMethod"
					}
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "GKILogin",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 1,
						"layoutName": "GKILoginAdminControlGroup_GridLayout"
					},
					"bindTo": "GKILogin"
				},
				"parentName": "GKILoginAdminControlGroup_GridLayout",
				"propertyName": "items",
				"index": 6
			},
			{
				"operation": "insert",
				"name": "GKIChangePasswordButton",
				"propertyName": "items",
				"parentName": "GKILoginAdminControlGroup_GridLayout",
				"values": {
					"layout": {
						"column": 0,
						"row": 2,
						"layoutName": "GKILoginAdminControlGroup_GridLayout"
					},
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": { "bindTo": "Resources.Strings.GKIChangePasswordButtonCaption" },
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"click": { "bindTo": "onGKIChangePasswordButtonClick" },
					"visible": {
						"bindTo": "GKIInsertPasswordVisible",
						"bindConfig": {
							"converter": "GKIChangePasswordButtonVisibleConverter"
						}
					},
					"enabled": true
				},
			},
			{
				"operation": "insert",
				"name": "GKISavePasswordButton",
				"propertyName": "items",
				"parentName": "GKILoginAdminControlGroup_GridLayout",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 3,
						"layoutName": "GKILoginAdminControlGroup_GridLayout"
					},
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": { "bindTo": "Resources.Strings.GKISavePasswordButtonCaption" },
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"click": { "bindTo": "onGKISavePasswordButtonClick" },
					"visible": { "bindTo": "GKIInsertPasswordVisible" },
					"enabled": true,
					"classes": {
						"textClass": ["actions-button-margin-right"],
						"wrapperClass": ["actions-button-margin-right"]
					},
				},
			},
			{
				"operation": "insert",
				"name": "GKIInsertPassword",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 3,
						"layoutName": "GKILoginAdminControlGroup_GridLayout"
					},
					"id": "new-password",
					"selectors": { "wrapEl": "#new-password" },
					"dataValueType": this.Terrasoft.DataValueType.TEXT,
					"visible": { "bindTo": "GKIInsertPasswordVisible" },
					"caption": { "bindTo": "Resources.Strings.NewPasswordCaption" },
					"controlConfig": {
						"protect": true
					},
				},
				"parentName": "GKILoginAdminControlGroup_GridLayout",
				"propertyName": "items",
				"index": 7
			},

			{
				"operation": "insert",
				"name": "GKIAdminEmail",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 0,
						"layoutName": "GKILoginAdminControlGroup_GridLayout"
					},
					"bindTo": "GKIAdminEmail"
				},
				"parentName": "GKILoginAdminControlGroup_GridLayout",
				"propertyName": "items",
				"index": 8
			},
			{
				"operation": "insert",
				"name": "GKIIsLimitApllied",
				"values": {
					"hint": { "bindTo": "Resources.Strings.IsLimitAplliedHint" },
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 12,
						"row": 4,
						"layoutName": "Header"
					},
					"bindTo": "GKIIsLimitApllied"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 9
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
				"name": "GKIInstanceLicenseDetail",
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
				"name": "GKIInstanceLicUserDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "NotesAndFilesTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.NotesAndFilesTabCaption"
					},
					"items": [],
					"order": 2
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
				"name": "GKIADGroupsTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.GKIADGroupsTabCaption"
					},
					"items": [],
					"order": 1
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "GKILoginAdminTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.GKILoginAdminTabCaption"
					},
					"items": [],
					"order": 1
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "GKILoginAdminControlGroup",
				"values": {
					"itemType": 15,
					"caption": {
						"bindTo": "Resources.Strings.GKILoginAdminGroupCaption"
					},
					"items": []
				},
				"parentName": "GKILoginAdminTab",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKILoginAdminControlGroup_GridLayout",
				"values": {
					"itemType": this.Terrasoft.ViewItemType.GRID_LAYOUT,
					"collapseEmptyRow": true,
					"items": []
				},
				"parentName": "GKILoginAdminControlGroup",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIInstanceGroupADDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GKIADGroupsTab",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "GKIGroupADInstanceLicenseDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GKIADGroupsTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "GKIGroupADUsersDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GKIADGroupsTab",
				"propertyName": "items",
				"index": 2
			}
		]/**SCHEMA_DIFF*/
	};
});
