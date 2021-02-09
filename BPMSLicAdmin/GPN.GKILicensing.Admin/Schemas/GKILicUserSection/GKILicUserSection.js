define("GKILicUserSection", ["ProcessModuleUtilities", "BaseFiltersGenerateModule", "RightUtilities", 
"GKILicensingButtonsMixin", "css!GKICustomFixedFiltersCss"], 
	function(ProcessModuleUtilities, BaseFiltersGenerateModule, RightUtilities) {
	return {
		entitySchemaName: "GKILicUser",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		attributes: {
			"GKIIsToShowGKIActiveOnly": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: true
			},
			"GKIIsToShowGKIIsVIPOnly": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: false
			},
			"GKIIsToShowGKIMSADActiveOnly": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: false
			},
			"isGKIButtonsEnabled": {
				dataValueType: Terrasoft.DataValueType.BOOLEAN,
				type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				value: false
			},
		},
		messages: {
			"GKILicUserSyncCombinedMessage": {
				mode: Terrasoft.MessageMode.PTP,
				direction: Terrasoft.MessageDirectionType.PUBLISH
			}
		},
		mixins: {
			GKILicensingButtonsMixin: "Terrasoft.GKILicensingButtonsMixin"
		},
		methods: {
			/**
			 * @overridden
			 * @desc действия при инициализации
			 */
			init: function() {
				this.callParent(arguments);
				this.on("change:GKIIsToShowGKIActiveOnly", this.onGKIFiltersChanged, this);
				this.on("change:GKIIsToShowGKIIsVIPOnly", this.onGKIFiltersChanged, this);
				this.on("change:GKIIsToShowGKIMSADActiveOnly", this.onGKIFiltersChanged, this);
				this.isGKIButtonsEnabledMethod();
			},
			/**
			 * @overridden
			 * @desc получение фильтров
			 */
			getFilters: function() {
				var esq = this.callParent(arguments);
				esq = this.changeByFixedFilter(esq, "GKIIsToShowGKIActiveOnly");
				esq = this.changeByFixedFilter(esq, "GKIIsToShowGKIIsVIPOnly");
				esq = this.changeByFixedFilter(esq, "GKIIsToShowGKIMSADActiveOnly");
				return esq;
			},
			/**
			 * @overridden
			 * @desc удаление "Импорт данных" в меню "Действия"
			 */
			getDataImportMenuItemVisible: Terrasoft.emptyFn,
			/**
			 * @overridden
			 * @desc удаление "Удалить" в меню "Действия"
			 */
			isVisibleDeleteAction: Terrasoft.emptyFn,
			/**
			 * @public
			 * @param {object} esq 
			 * @param {string} name 
			 */
			changeByFixedFilter: function(esq, name) {
				var prefix = "GKIIsToShow";
				var value = this.get(name);
				var filter = name + "Filter";
				var column = name.substring(prefix.length, name.indexOf("Only"));
				if (value) {
					esq.add(filter,
						this.Terrasoft.createColumnFilterWithParameter(
						Terrasoft.ComparisonType.EQUAL, column,
							true));
				} else {
					esq.removeByKey(filter);
				}
				return esq;
			},
			/**
			 * @public
			 * @desc событие при смене значения фильтра
			 */
			onGKIFiltersChanged: function() {
				this.reloadGridData();
			},
			/**
			 * @public
			 * @desc вызывает процесс, который обновляет информацию со стендов
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
			 * @desc вызывает синхронизацию с LDAP
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
			 * @desc отправка в page для запуска сервиса
			 */
			onGKILicUserSyncCombinedButtonClick: function() {
				this.sandbox.publish("GKILicUserSyncCombinedMessage", null, [this.sandbox.id]);
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
		},
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "remove",
				"name": "SeparateModeAddRecordButton",
				"parentName": "SeparateModeActionButtonsContainer"
			},
			{
				"operation": "remove",
				"name": "CombinedModeAddRecordButton",
				"parentName": "CombinedModeActionButtonsContainer"
			},
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
				"name": "GKILicUserSyncButton",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKILicUserSyncButtonCaption"},
					"click": {"bindTo": "onGKILicUserSyncButtonClick"},
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
				"name": "GKILicenseSyncRegularButton",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKILicenseSyncRegularButtonCaption"},
					"click": {"bindTo": "onGKILicenseSyncRegularButtonClick"},
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
				"name": "GKILicSyncLDAPButton",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKILicSyncLDAPButtonCaption"},
					"click": {"bindTo": "onGKILicSyncLDAPButtonClick"},
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
				"name": "GKILicUserSyncCombinedButton",
				"parentName": "CombinedModeActionButtonsCardLeftContainer",
				"propertyName": "items",
				"values": {
					"itemType": Terrasoft.ViewItemType.BUTTON,
					"style": Terrasoft.controls.ButtonEnums.style.DEFAULT,
					"caption": {"bindTo": "Resources.Strings.GKILicUserSyncButtonCaption"},
					"click": {"bindTo": "onGKILicUserSyncCombinedButtonClick"},
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
				"name": "GKICustomFixedFilters",
				"parentName": "LeftGridUtilsContainer",
				"propertyName": "items",
				"index": 1,
				"values": {
					"id": "GKICustomFixedFilters",
					"itemType": this.Terrasoft.ViewItemType.CONTAINER,
					"items": [],
				}
			},
			{
				"operation": "insert",
				"parentName": "GKICustomFixedFilters",
				"propertyName": "items",
				"name": "GKIIsToShowGKIActiveOnly",
				"values": {
					"caption": {
						bindTo: "Resources.Strings.GKIIsToShowGKIActiveOnlyFilterCaption"
					},
					"bindTo": "GKIIsToShowGKIActiveOnly",
					"controlConfig": {
						"className": "Terrasoft.CheckBoxEdit",
						"checked": {
							"bindTo": "GKIIsToShowGKIActiveOnly"
						},
					},
					"layout": {
						"column": 0,
						"row": 0,
						"colSpan": 12
					}
				}
			},
			
			{
				"operation": "insert",
				"parentName": "GKICustomFixedFilters",
				"propertyName": "items",
				"name": "GKIIsToShowGKIIsVIPOnly",
				"values": {
					"caption": {
						bindTo: "Resources.Strings.GKIIsToShowGKIIsVIPOnlyFilterCaption"
					},
					"bindTo": "GKIIsToShowGKIIsVIPOnly",
					"controlConfig": {
						"className": "Terrasoft.CheckBoxEdit",
						"checked": {
							"bindTo": "GKIIsToShowGKIIsVIPOnly"
						},
					},
					"layout": {
						"column": 0,
						"row": 0,
						"colSpan": 12
					}
				}
			},
			{
				"operation": "insert",
				"parentName": "GKICustomFixedFilters",
				"propertyName": "items",
				"name": "GKIIsToShowGKIMSADActiveOnly",
				"values": {
					"caption": {
						bindTo: "Resources.Strings.GKIIsToShowGKIMSADActiveOnlyFilterCaption"
					},
					"bindTo": "GKIIsToShowGKIMSADActiveOnly",
					"controlConfig": {
						"className": "Terrasoft.CheckBoxEdit",
						"checked": {
							"bindTo": "GKIIsToShowGKIMSADActiveOnly"
						},
					},
					"layout": {
						"column": 0,
						"row": 0,
						"colSpan": 12
					}
				}
			}
		]/**SCHEMA_DIFF*/,
	};
});
