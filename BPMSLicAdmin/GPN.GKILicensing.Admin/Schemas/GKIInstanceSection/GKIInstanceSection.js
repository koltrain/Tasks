define("GKIInstanceSection", ["RightUtilities", "ProcessModuleUtilities", "GKIInstanceSectionResources"], function (RightUtilities, ProcessModuleUtilities, resources) {
	return {
		entitySchemaName: "GKIInstance",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		attributes: {
			"isGKIButtonsEnabled": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: false
			},
		},
		messages: {
			"GKIInstanceUpdateRow": {
				mode: Terrasoft.MessageMode.PTP,
				direction: Terrasoft.MessageDirectionType.SUBSCRIBE
			}
		},
		methods: {
			/**
			 * @overridden
			 * @desc действия при инициализации
			 */
			init: function () {
				this.callParent(arguments);
				this.isGKIButtonsEnabledMethod();
				this.sandbox.registerMessages(this.messages);
			},
			 /**
			 * @overridden
			 * @desc подписка на сообщения
			 */
			subscribeSandboxEvents: function () {
				this.callParent(arguments);
				this.sandbox.subscribe("GKIInstanceUpdateRow", this.updateGrid, this, ["GKIInstanceUpdateRow"]);
			},
			/**
			 * @public
			 * @desc обновление списка раздела
			 */
			updateGrid: function() {
				this.reloadEntity();
			},
			/**
			 * @overridden
			 * @desc удаление "Удалить" в меню "Действия"
			 */
			isVisibleDeleteAction: Terrasoft.emptyFn,
			/**
			 * @private
			 * @desc определение видимости кнопок по правам
			 */
			isGKIButtonsEnabledMethod: function () {
				RightUtilities.checkCanExecuteOperation({
					operation: "GKICanManageLicensingSettings"
				}, function (result) {
					this.set("isGKIButtonsEnabled", result);
				}, this);
			},
			/**
			 * @public
			 * @desc вызывает процесс, который обновляет лицензии по всем пользователям экземпляров
			 */
			onGKIInstanceUpdateButtonClick: function () {
				var cfg = {
					style: Terrasoft.MessageBoxStyles.BLUE
				};
				var selectedRow = this.$ActiveRow;
				var selectedRows = this.get("SelectedRows");
				if (selectedRows.length > 0) {
					this.showConfirmationDialog(resources.localizableStrings.GKIInstanceUpdateSelectedConfirmation,
						function getSelectedButton(returnCode) {
							if (returnCode === Terrasoft.MessageBoxButtons.YES.returnCode) {
								var args = {
									sysProcessName: "GKIInstanceUpdateSelectedProcess",
									parameters: {
										instanceIds: selectedRows,
									}
								};
								ProcessModuleUtilities.executeProcess(args);
								this.showInformationDialog(this.get("Resources.Strings.GKIInstanceUpdateProcessReminder"));
								return;
							}
						}, [Terrasoft.MessageBoxButtons.YES, Terrasoft.MessageBoxButtons.NO], null, cfg);
				} else if (selectedRow != null) {
					this.showConfirmationDialog(resources.localizableStrings.GKIInstanceUpdateSelectedConfirmation,
						function getSelectedButton(returnCode) {
							if (returnCode === Terrasoft.MessageBoxButtons.YES.returnCode) {
								var args = {
									sysProcessName: "GKIInstanceUpdateOneSelectedProcess",
									parameters: {
										instanceId: selectedRow
									}
								};
								ProcessModuleUtilities.executeProcess(args);
								this.showInformationDialog(this.get("Resources.Strings.GKIInstanceUpdateProcessReminder"));
								return;
							}
						}, [Terrasoft.MessageBoxButtons.YES, Terrasoft.MessageBoxButtons.NO], null, cfg);
				} else {
					this.showInformationDialog(this.get("Resources.Strings.GKIInstanceSelectedNull"));
					return;
				}
			},
			/**
			 * @public
			 * @desc вызывает процесс, который синхронизирует информацию со стендов
			 */
			onGKIInstanceSyncRegularButtonClick: function () {
				var selectedRow = this.$ActiveRow;
				var selectedRows = this.get("SelectedRows");

				if (selectedRows.length > 0) {
					var args = {
						sysProcessName: "GKIInstanceLicenseSelectedSyncProcess",
						parameters: {
							instanceIds: selectedRows
						}
					};
					ProcessModuleUtilities.executeProcess(args);
					this.showInformationDialog(this.get("Resources.Strings.GKIInstanceSyncProcessReminder"));
					return;
				} else if (selectedRow != null) {
					var args = {
						sysProcessName: "GKIInstanceLicenseOneSelectedSyncProcess",
						parameters: {
							instanceId: selectedRow
						}
					};
					ProcessModuleUtilities.executeProcess(args);
					this.showInformationDialog(this.get("Resources.Strings.GKIInstanceSyncProcessReminder"));
					return;
				} else {
					this.showInformationDialog(this.get("Resources.Strings.GKIInstanceSelectedNull"));
					return;
				}
			},
			/**
			 * @public
			 * @desc вызывает синхронизацию с LDAP
			 */
			onGKIInstanceSyncLDAPButtonClick: function () {
				var selectedRow = this.$ActiveRow;
				var selectedRows = this.get("SelectedRows");

				if (selectedRows.length > 0) {
					var args = {
						sysProcessName: "GKIInstanceLDAPSelectedSyncProcess",
						parameters: {
							instanceIds: selectedRows
						}
					};
					ProcessModuleUtilities.executeProcess(args);
					this.showInformationDialog(this.get("Resources.Strings.GKIInstanceSyncLDAPButtonReminder"));
					return;
				} else if (selectedRow != null) {
					var args = {
						sysProcessName: "GKIInstanceLDAPOneSelectedSyncProcess",
						parameters: {
							instanceId: selectedRow
						}
					};
					ProcessModuleUtilities.executeProcess(args);
					this.showInformationDialog(this.get("Resources.Strings.GKIInstanceSyncLDAPButtonReminder"));
					return;
				} else {
					this.showInformationDialog(this.get("Resources.Strings.GKIInstanceSelectedNull"));
					return;
				}
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
				"name": "GKIInstanceUpdateButton",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
				"values": {
					"hint": { "bindTo": "Resources.Strings.GKIInstanceUpdateButtonHint" },
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": { "bindTo": "Resources.Strings.GKIInstanceUpdateButtonCaption" },
					"click": { "bindTo": "onGKIInstanceUpdateButtonClick" },
					"visible": true,
					"enabled": { "bindTo": "isGKIButtonsEnabled" },
					"classes": {
						"textClass": ["actions-button-margin-right"],
						"wrapperClass": ["actions-button-margin-right"]
					},
				},
			},
			{
				"operation": "insert",
				"name": "GKIInstanceSyncRegularButton",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
				"values": {
					"hint": { "bindTo": "Resources.Strings.GKIInstanceSyncRegularButtonHint" },
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": { "bindTo": "Resources.Strings.GKIInstanceSyncRegularButtonCaption" },
					"click": { "bindTo": "onGKIInstanceSyncRegularButtonClick" },
					"visible": true,
					"enabled": { "bindTo": "isGKIButtonsEnabled" },
					"classes": {
						"textClass": ["actions-button-margin-right"],
						"wrapperClass": ["actions-button-margin-right"]
					},
				},
			},
			{
				"operation": "insert",
				"name": "GKIInstanceSyncLDAPButton",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
				"values": {
					"hint": { "bindTo": "Resources.Strings.GKIInstanceSyncLDAPButtonHint" },
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": { "bindTo": "Resources.Strings.GKIInstanceSyncLDAPButtonCaption" },
					"click": { "bindTo": "onGKIInstanceSyncLDAPButtonClick" },
					"visible": true,
					"enabled": { "bindTo": "isGKIButtonsEnabled" },
					"classes": {
						"textClass": ["actions-button-margin-right"],
						"wrapperClass": ["actions-button-margin-right"]
					},
				},
			}
		]/**SCHEMA_DIFF*/
	};
});
