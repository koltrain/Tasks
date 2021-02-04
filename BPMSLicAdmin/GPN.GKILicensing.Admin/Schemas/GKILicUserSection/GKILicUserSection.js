define("GKILicUserSection", ["ServiceHelper", "ProcessModuleUtilities"], function(ServiceHelper, ProcessModuleUtilities) {
	return {
		entitySchemaName: "GKILicUser",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		messages: {
			"GKILicUserSyncCombinedMessage": {
				mode: Terrasoft.MessageMode.PTP,
				direction: Terrasoft.MessageDirectionType.PUBLISH
			}
		},
		methods: {
			/**
			 * @public
			 * @desc: вызывает процесс, который обновляет информацию со стендов
			 */
			onGKILicenseSyncRegularButtonClick: function() {
				var args = {
					name: "GKILicenseSyncRegularProcess",
					parameters: {
						instanceIdFilter: Terrasoft.GUID_EMPTY
					}
				};
				ProcessModuleUtilities.startBusinessProcess(args);
				this.showInformationDialog(this.get("Resources.Strings.GKILicenseSyncRegularProcessReminder"));
				return;
			},

			/**
			 * @public
			 * @desc: подтверждение вызова сервиса, который синхронизирует лицензии
			 */
			onGKILicUserSyncButtonClick: function() {
				var cfg = {
					style: Terrasoft.MessageBoxStyles.BLUE
				};
				this.showConfirmationDialog(this.get("Resources.Strings.GKILicUserSyncConfirmation"),
				function getSelectedButton(returnCode) {
					if (returnCode === Terrasoft.MessageBoxButtons.YES.returnCode) {
						this.GKILicUserSyncServiceCall();
					}
				}, [Terrasoft.MessageBoxButtons.YES, Terrasoft.MessageBoxButtons.NO], null, cfg);
			},

			/**
			 * @public
			 * @desc: вызывает бизнес-процесс, который синхронизирует лицензии
			 */
			GKILicUserSyncServiceCall: function() {
				var args = {
					name: "GKILicensingLicUserSyncProcess",
					parameters: null
				};
				ProcessModuleUtilities.startBusinessProcess(args);
				this.showInformationDialog(this.get("Resources.Strings.GKISyncIsInProcessReminder"));
				return;
			},

			/**
			 * @public
			 * @desc: вызывает синхронизацию с LDAP
			 */
			onGKILicSyncLDAPButtonClick: function() {
				var args = {
					name: "GKILicensingLDAPSyncProcess",
					parameters: null
				};
				ProcessModuleUtilities.startBusinessProcess(args);
				this.showInformationDialog(this.get("Resources.Strings.GKILdapSyncIsInProcessReminder"));
				return;
			},

			/**
			 * @public
			 * @desc: отправка в page для запуска сервиса
			 */
			onGKILicUserSyncCombinedButtonClick: function() {
				this.sandbox.publish("GKILicUserSyncCombinedMessage", null, [this.sandbox.id]);
			}
		},
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "GKILicUserSyncButton",
				"parentName": "SeparateModeActionButtonsContainer",
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
			},
			{
				"operation": "insert",
				"name": "GKILicenseSyncRegularButton",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKILicenseSyncRegularButtonCaption"},
					"click": {"bindTo": "onGKILicenseSyncRegularButtonClick"},
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
				"name": "GKILicSyncLDAPButton",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKILicSyncLDAPButtonCaption"},
					"click": {"bindTo": "onGKILicSyncLDAPButtonClick"},
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
				"name": "GKILicUserSyncCombinedButton",
				"parentName": "CombinedModeActionButtonsCardLeftContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKILicUserSyncButtonCaption"},
					"click": {"bindTo": "onGKILicUserSyncCombinedButtonClick"},
					"visible": true,
					"enabled": true,
					"classes": {
						"textClass": ["actions-button-margin-right"],
						"wrapperClass": ["actions-button-margin-right"]
					},
				},
			},
		]/**SCHEMA_DIFF*/,
	};
});
