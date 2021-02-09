define("GKILicensingSettingsPage", ["terrasoft", "GKILicensingSettingsPageResources", "ServiceHelper", 
	"BusinessRuleModule", "RightUtilities", "SecurityUtilities", "ContextHelpMixin", "css!DetailModuleV2"],
	function(Terrasoft, resources, ServiceHelper, BusinessRuleModule, RightUtilities) {
		return {
			messages: {
				/**
				 * @message ChangeHeaderCaption
				 * Notifies that the header caption was changed.
				 */
				"ChangeHeaderCaption": {
					mode: Terrasoft.MessageMode.PTP,
					direction: Terrasoft.MessageDirectionType.PUBLISH
				},

				/**
				 * @message BackHistoryState
				 * Notifies that state was returned to previous.
				 */
				"BackHistoryState": {
					mode: Terrasoft.MessageMode.BROADCAST,
					direction: Terrasoft.MessageDirectionType.PUBLISH
				},

				/**
				 * @message InitDataViews
				 * Notifies that the header caption was changed.
				 */
				"InitDataViews": {
					mode: Terrasoft.MessageMode.PTP,
					direction: Terrasoft.MessageDirectionType.PUBLISH
				},
			},
			mixins: {},
			attributes: {
				"GKICanManageLicensingSettings": {
					dataValueType: Terrasoft.DataValueType.BOOLEAN,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
					value: false
				}
			},
			methods: {
				/**
				 * ########## ## ########## ######.
				 * @protected
				 * @virtual
				 */
				cancel: function() {
					this.sandbox.publish("BackHistoryState");
				},

				/**
				 * Инициализирует страницу и параметры на ней
				 * @protected
				 * @overridden
				 */
				init: function(callback, scope) {
					this.callParent([function() {
						this.set("LDAPEnumFieldName", "LDAPAuthType");
						const headerConfig = {
							caption: resources.localizableStrings.PageCaption,
							isMainMenu: false,
						};
						this.sandbox.publish("ChangeHeaderCaption", headerConfig);
						this.sandbox.publish("InitDataViews", headerConfig);
						/*
						var sysSettingsKeys = this.getGKISettingKeys();
						Terrasoft.SysSettings.querySysSettings(sysSettingsKeys, this.onGetSysSettingValues, this);
						*/
						callback.call(scope);
					}, this]);
					
				},
				/**
				 * @private
				 * @desc проверяет права на изменение настроек службы лицензирования
				 */
				canManageLicensingSettings: function() {
					RightUtilities.checkCanExecuteOperation({
						operation: "GKICanManageLicensingSettings"
					}, function(result) {
						return result;
					}, this);
				},
				getImage: function(){
					return Terrasoft.ImageUrlBuilder.getUrl(resources.localizableImages.UnderConstruction);
				}
			},
			rules: {},
			diff: [{
				"operation": "insert",
				"name": "ServerSettingsContainerLDAP",
				"values": {
					"id": "ServerSettingsContainerLDAP",
					"selectors": {
						"wrapEl": "#ServerSettingsContainerLDAP"
					},
					"classes": {
						"textClass": "center-panel"
					},
					"itemType": Terrasoft.ViewItemType.CONTAINER,
					"items": []
				}
			}, 
			{
				"operation": "insert",
				"name": "UnderConstruction",
				"parentName": "ServerSettingsContainerLDAP",
				"propertyName": "items",
				"values": {
					"className": "Terrasoft.ImageEdit",
					"readonly": true,
					"classes": {
						"wrapClass": ["image-control"]
					},
					"getSrcMethod": "getImage",
					"generator": "ImageCustomGeneratorV2.generateSimpleCustomImage"
				},
				"index": 0
			},
			]
		};
	});
