define("GKILicensingSettingsPage", ["terrasoft", "GKILicensingSettingsPageResources", "ServiceHelper", 
	"BusinessRuleModule", "RightUtilities", "SecurityUtilities", "ContextHelpMixin", "css!DetailModuleV2"],
	function(Terrasoft, resources, ServiceHelper, BusinessRuleModule, RightUtilities) {
		return {
			messages: {
				"ChangeHeaderCaption": {
					mode: Terrasoft.MessageMode.PTP,
					direction: Terrasoft.MessageDirectionType.PUBLISH
				},
				"BackHistoryState": {
					mode: Terrasoft.MessageMode.BROADCAST,
					direction: Terrasoft.MessageDirectionType.PUBLISH
				},
				"InitDataViews": {
					mode: Terrasoft.MessageMode.PTP,
					direction: Terrasoft.MessageDirectionType.PUBLISH
				},
			},
			mixins: {},
			attributes: {
				"EnumFieldName": {
					dataValueType: Terrasoft.DataValueType.TEXT,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKISysLicUserInactivenessControlTimespan": {
					dataValueType: Terrasoft.DataValueType.FLOAT,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKILastMasterCheckInWaitMinutes": {
					dataValueType: Terrasoft.DataValueType.INTEGER,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKILicensingAdminToRegularRequestTimer": {
					dataValueType: Terrasoft.DataValueType.INTEGER,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKILicensingLDAPEntryMaxModifiedOn": {
					dataValueType: Terrasoft.DataValueType.DATE_TIME,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKILicensingLDAPEntryModifiedOnAttribute": {
					dataValueType: Terrasoft.DataValueType.TEXT,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKILicensingLDAPUsersFilter": {
					dataValueType: Terrasoft.DataValueType.TEXT,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKILicensingMailBox": {
					dataValueType: Terrasoft.DataValueType.LOOKUP,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKILicensingTerrasoftSupportAddress": {
					dataValueType: Terrasoft.DataValueType.TEXT,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKILicensingWinInstanceLogin": {
					dataValueType: Terrasoft.DataValueType.TEXT,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKILicensingWinInstancePassword": {
					dataValueType: Terrasoft.DataValueType.TEXT,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKILicensingWinInstanceUrl": {
					dataValueType: Terrasoft.DataValueType.TEXT,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"GKISysLicUserInactivenessControlCooldown": {
					dataValueType: Terrasoft.DataValueType.FLOAT,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
				},
				"isGKICanManageLicensingSettings": {
					dataValueType: Terrasoft.DataValueType.BOOLEAN,
					type: Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
					value: false
				}
			},
			methods: {

				/**
				 * @protected
				 * @desc уведомляет о возвращении к предыдущему состоянию
				 * @virtual
				 */
				cancel: function() {
					this.sandbox.publish("BackHistoryState");
				},

				/**
				 * @protected
				 * @desc Инициализирует страницу и параметры на ней
				 * @overridden
				 */
				init: function(callback, scope) {
					this.callParent([function() {
						this.set("EnumFieldName", "GKILicensingMailBox");
						const headerConfig = {
							caption: resources.localizableStrings.PageCaption,
							isMainMenu: false,
						};
						this.sandbox.publish("ChangeHeaderCaption", headerConfig);
						this.sandbox.publish("InitDataViews", headerConfig);
						
						var sysSettingsKeys = this.getGKISettingKeys();
						Terrasoft.SysSettings.querySysSettings(sysSettingsKeys, this.onGetSysSettingValues, this);
						
						this.getIsGKICanManageLicensingSettings();
						callback.call(scope);
					}, this]);
					
				},

				/**
				 * @private
				 * @desc доступность кнопки Сохранить
				 */
				getIsGKICanManageLicensingSettings: function() {
					this.canManageLicensingSettings(function(value){
						this.set("isGKICanManageLicensingSettings", value);
					}, this);
				},

				/**
				 * @private
				 * @desc проверяет права на изменение настроек службы лицензирования
				 */
				canManageLicensingSettings: function(callback, scope) {
					RightUtilities.checkCanExecuteOperation({
						operation: "GKICanManageLicensingSettings"
					}, function(result) {
						if (callback){
							callback.call(scope || this, result);
						}
						return result;
					}, this);
				},
				
				/**
				 * @protected
				 * @desc получение списка системныз настроек
				 */
				getGKISettingKeys: function() {
					var columnsCollection = this.columns;
					var filteredColumnsCollection = this.filterForColumns(columnsCollection);
					var enumFieldName = this.get("EnumFieldName");
					var keys = [];
					this.Terrasoft.each(filteredColumnsCollection, function(column) {
						var key = column.name;
						if (key !== enumFieldName + "List") {
							keys.push(key);
						}
					}, this);
					return keys;
				},
				
				/**
				 * @protected
				 * @desc фильтрация полей на системные настройки
				 * @virtual
				 */
				filterForColumns: function(element) {
					var filter = "GKI";
					var filteredCollection = [];
					for (var el in element) {
						if (el.substring(0, 3) === filter) {
							filteredCollection.push(element[el]);
						}
					}
					return (filteredCollection);
				},
				
				/**
				 * @public
				 * @desc обработка ответа сервиса запроса системных настроек
				 * @param {Terrasoft.Collection} sysSettingsCollection коллекция системных настроек
				 */
				onGetSysSettingValues: function(sysSettingsCollection) {
					if (!sysSettingsCollection) {
						return;
					}
					var enumFieldName = this.get("EnumFieldName");
					this.Terrasoft.each(sysSettingsCollection, function(value, key) {
						if (key !== enumFieldName) {
							if (key === "GKILicensingWinInstancePassword") {
								value = null;
							}
							this.set(key, value);
						} else {
							this.getColumnByName(key).referenceSchemaName = "MailboxSyncSettings";
							var esq = this.getLookupQuery(null, key, false);
							esq.enablePrimaryColumnFilter(value.value);
							esq.getEntityCollection(function(result) {
								if (result.success && result.collection.getCount()) {
									var entity = result.collection.getByIndex(0);
									var enumConfig = {
										value: entity.values.value,
										displayValue: entity.values.displayValue
									};
									this.set(key, enumConfig);
								}
							}, this);
						}
					}, this);
				},

				 /**
				 * @protected
				 * @desc подготовка коллекции для справочника почтовых ящиков
				 * @param {String} filter фильтр для коллекции
				 */
				onPrepareMailboxEnum: function(filter) {
					var prepareListColumnName = this.get("EnumFieldName");
					this.set("PrepareListColumnName", prepareListColumnName);
					var esq = this.Ext.create("Terrasoft.EntitySchemaQuery", {
						rootSchemaName: "MailboxSyncSettings"
					});
					esq.addColumn("Id");
					esq.getEntityCollection(function(result) {
						var existsCollection = [];
						if (result.success) {
							result.collection.each(function(item) {
								existsCollection.push(item.get("Id"));
							});
						}
						var filtersCollection = this.Terrasoft.createFilterGroup();
						if (existsCollection.length > 0) {
							filtersCollection.add("existsFilter", this.Terrasoft.createColumnInFilterWithParameters(
								"Id", existsCollection));
						} else {
							filtersCollection.add("emptyFilter", this.Terrasoft.createColumnIsNullFilter("Id"));
						}
						this.set(prepareListColumnName + "Filters", filtersCollection);
						this.loadLookupData(filter, this.get(prepareListColumnName + "List"),
							prepareListColumnName, true);
					}, this);
				},

				/**
				 * @private
				 * @desc получение результата запроса справочного поля
				 * @overridden
				 * @param {String} filterValue ###### ### primaryDisplayColumn
				 * @param {String} columnName ### ####### ViewModel
				 * @param {Boolean} isLookup ####### ########### ####
				 * @return {Terrasoft.EntitySchemaQuery}
				 */
				getLookupQuery: function(filterValue, columnName, isLookup) {
					var prepareListColumnName = this.get("PrepareListColumnName");
					var prepareListFilters = this.get(prepareListColumnName + "Filters");
					var entitySchemaQuery = this.callParent([filterValue, columnName, isLookup]);
					if (columnName === prepareListColumnName && prepareListFilters) {
						entitySchemaQuery.filters.add(prepareListColumnName + "Filter", prepareListFilters);
					}
					return entitySchemaQuery;
				},

				/**
				 * Сохранение
				 * @protected
				 * @overridden
				 */
				save: function() {
					if (this.validate()) {
						ServiceHelper.callService({
							serviceName: "GKILicensingAdminService",
							methodName: "GKISaveLicensingSysSettings",
							data: {
								request: this.collectValuesOfSysSettings()
							},
							callback: this.saveCallBack,
							scope: this
						});
					}
				},

				/**
				 * Формирование json-файла с настройками для сохранения.
				 * @protected
				 * @virtual
				 */
				collectValuesOfSysSettings: function() {
					var sysSettingKeys = this.getGKISettingKeys();
					var sysEnumFieldName = this.get("EnumFieldName");
					var sysSettingsCollection = [];
					this.Terrasoft.each(sysSettingKeys, function(key) {
						var value = this.get(key);
						if (!value){
							return;
						}
						if (key === sysEnumFieldName) {
							value = value.value ? value.value : value;
						}
						var KeyValuePairs = {
							"Key": key,
							"Value": value
						};
						sysSettingsCollection.push(KeyValuePairs);
					}, this);
					return sysSettingsCollection;
				},
				/**
				 * Коллбек сохранения
				 * @protected
				 */
				saveCallBack: function(response) {
					if (response && response.success) {
						var message = resources.localizableStrings.SuccessMessage;
						this.showInformationDialog(message);
						this.sandbox.publish("BackHistoryState");
					} else {
						this.showInformationDialog(response.errorInfo.message);
					}
				}
			},
			rules: {},
			diff: [
				{
					"operation": "insert",
					"name": "SettingsContainer",
					"values": {
						"id": "SettingsContainer",
						"selectors": {
							"wrapEl": "#SettingsContainer"
						},
						"classes": {
							"textClass": "center-panel"
						},
						"itemType": Terrasoft.ViewItemType.GRID_LAYOUT,
						"items": [],
					}
				},
				{
					"operation": "insert",
					"name": "HeaderContainer",
					"parentName": "SettingsContainer",
					"propertyName": "items",
					"values": {
						"id": "HeaderContainer",
						"selectors": {
							"wrapEl": "#HeaderContainer"
						},
						"itemType": Terrasoft.ViewItemType.CONTAINER,
						"layout": {
							"column": 0,
							"row": 0,
							"colSpan": 24
						},
						"items": []
					}
				}, 
				{
					"operation": "insert",
					"parentName": "HeaderContainer",
					"propertyName": "items",
					"name": "SaveButton",
					"values": {
						"itemType": Terrasoft.ViewItemType.BUTTON,
						"caption": {
							"bindTo": "Resources.Strings.SaveButtonCaption"
						},
						"classes": {
							"textClass": "actions-button-margin-right"
						},
						"click": {
							"bindTo": "save"
						},
						"enabled": {
							"bindTo": "isGKICanManageLicensingSettings"
						},
						"style": "green",
						"layout": {
							"column": 0,
							"row": 0,
							"colSpan": 2
						}
					}
				}, 
				{
					"operation": "insert",
					"parentName": "HeaderContainer",
					"propertyName": "items",
					"name": "CancelButton",
					"values": {
						"itemType": Terrasoft.ViewItemType.BUTTON,
						"caption": {
							"bindTo": "Resources.Strings.CancelButtonCaption"
						},
						"classes": {
							"textClass": "actions-button-margin-right"
						},
						"click": {
							"bindTo": "cancel"
						},
						"style": "default",
						"layout": {
							"column": 4,
							"row": 0,
							"colSpan": 2
						}
					}
				},
				{
					"operation": "insert",
					"name": "SysSettingsContainer",
					"parentName": "SettingsContainer",
					"propertyName": "items",
					"values": {
						"id": "SettingsContainer",
						"selectors": {
							"wrapEl": "#SettingsContainer"
						},
						"itemType": Terrasoft.ViewItemType.CONTAINER,
						"layout": {
							"column": 0,
							"row": 2,
							"colSpan": 24
						},
						"items": []
					}
				},
				{
					"operation": "insert",
					"name": "CommonSettingsGroup",
					"parentName": "SysSettingsContainer",
					"propertyName": "items",
					"values": {
						"id": "CommonSettingsGroup",
						"selectors": {
							"wrapEl": "#CommonSettingsGroup"
						},
						"itemType": Terrasoft.ViewItemType.CONTROL_GROUP,
						"layout": {
							"column": 0,
							"row": 1,
							"colSpan": 24
						},
						"controlConfig": {
							"collapsed": false,
							"caption": {
								"bindTo": "Resources.Strings.CommonSettingsGroupLabel"
							}
						},
						"items": []
					}
				},
				{
					"operation": "insert",
					"name": "LDAPSettingsGroup",
					"parentName": "SysSettingsContainer",
					"propertyName": "items",
					"values": {
						"id": "LDAPSettingsGroup",
						"selectors": {
							"wrapEl": "#LDAPSettingsGroup"
						},
						"itemType": Terrasoft.ViewItemType.CONTROL_GROUP,
						"layout": {
							"column": 0,
							"row": 2,
							"colSpan": 24
						},
						"controlConfig": {
							"collapsed": false,
							"caption": {
								"bindTo": "Resources.Strings.LDAPSettingsGroupLabel"
							}
						},
						"items": []
					}
				},
				{
					"operation": "insert",
					"name": "DeactivationSettingsGroup",
					"parentName": "SysSettingsContainer",
					"propertyName": "items",
					"values": {
						"id": "DeactivationSettingsGroup",
						"selectors": {
							"wrapEl": "#DeactivationSettingsGroup"
						},
						"itemType": Terrasoft.ViewItemType.CONTROL_GROUP,
						"layout": {
							"column": 0,
							"row": 3,
							"colSpan": 24
						},
						"controlConfig": {
							"collapsed": false,
							"caption": {
								"bindTo": "Resources.Strings.DeactivationSettingsGroupLabel"
							}
						},
						"items": []
					}
				},
				{
					"operation": "insert",
					"name": "EmailSettingsGroup",
					"parentName": "SysSettingsContainer",
					"propertyName": "items",
					"values": {
						"id": "EmailSettingsGroup",
						"selectors": {
							"wrapEl": "#EmailSettingsGroup"
						},
						"itemType": Terrasoft.ViewItemType.CONTROL_GROUP,
						"layout": {
							"column": 0,
							"row": 4,
							"colSpan": 24
						},
						"controlConfig": {
							"collapsed": false,
							"caption": {
								"bindTo": "Resources.Strings.EmailSettingsGroupLabel"
							}
						},
						"items": []
					}
				},
				{
					"operation": "insert",
					"name": "CommonSettingsGroup_GridLayout",
					"values": {
						"itemType": this.Terrasoft.ViewItemType.GRID_LAYOUT,
						"items": []
					},
					"parentName": "CommonSettingsGroup",
					"propertyName": "items",
					"index": 0
				},
				{
					"operation": "insert",
					"name": "LDAPSettingsGroup_GridLayout",
					"values": {
						"itemType": this.Terrasoft.ViewItemType.GRID_LAYOUT,
						"items": []
					},
					"parentName": "LDAPSettingsGroup",
					"propertyName": "items",
					"index": 0
				},
				{
					"operation": "insert",
					"name": "DeactivationSettingsGroup_GridLayout",
					"values": {
						"itemType": this.Terrasoft.ViewItemType.GRID_LAYOUT,
						"items": []
					},
					"parentName": "DeactivationSettingsGroup",
					"propertyName": "items",
					"index": 0
				},
				{
					"operation": "insert",
					"name": "EmailSettingsGroup_GridLayout",
					"values": {
						"itemType": this.Terrasoft.ViewItemType.GRID_LAYOUT,
						"items": []
					},
					"parentName": "EmailSettingsGroup",
					"propertyName": "items",
					"index": 0
				},
				{
					"operation": "insert",
					"name": "GKILastMasterCheckInWaitMinutes",
					"parentName": "CommonSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 0,
							"row": 0,
							"colSpan": 8
						},
						"bindTo": "GKILastMasterCheckInWaitMinutes",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKILastMasterCheckInWaitMinutesCaption"
							}
						}
					}
				}, 
				{
					"operation": "insert",
					"name": "GKILicensingAdminToRegularRequestTimer",
					"parentName": "CommonSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 8,
							"row": 0,
							"colSpan": 8
						},
						"bindTo": "GKILicensingAdminToRegularRequestTimer",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKILicensingAdminToRegularRequestTimerCaption"
							}
						}
					}
				}, 
				{
					"operation": "insert",
					"name": "GKILicensingLDAPEntryMaxModifiedOn",
					"parentName": "LDAPSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 0,
							"row": 0,
							"colSpan": 8
						},
						"bindTo": "GKILicensingLDAPEntryMaxModifiedOn",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKILicensingLDAPEntryMaxModifiedOnCaption"
							}
						}
					}
				}, 
				{
					"operation": "insert",
					"name": "GKILicensingLDAPEntryModifiedOnAttribute",
					"parentName": "LDAPSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 8,
							"row": 0,
							"colSpan": 8
						},
						"bindTo": "GKILicensingLDAPEntryModifiedOnAttribute",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKILicensingLDAPEntryModifiedOnAttributeCaption"
							}
						}
					}
				}, 
				{
					"operation": "insert",
					"name": "GKILicensingLDAPUsersFilter",
					"parentName": "LDAPSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 0,
							"row": 1,
							"colSpan": 8
						},
						"bindTo": "GKILicensingLDAPUsersFilter",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKILicensingLDAPUsersFilterCaption"
							}
						}
					}
				}, 
				{
					"operation": "insert",
					"name": "GKILicensingMailBox",
					"parentName": "EmailSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 0,
							"row": 0,
							"colSpan": 8
						},
						"bindTo": "GKILicensingMailBox",
						"contentType": Terrasoft.ContentType.ENUM,
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKILicensingMailBoxCaption"
							}
						},
						"controlConfig": {
							"prepareList": {"bindTo": "onPrepareMailboxEnum"}
						}
					}
				},
				{
					"operation": "insert",
					"name": "GKILicensingTerrasoftSupportAddress",
					"parentName": "EmailSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 8,
							"row": 0,
							"colSpan": 8
						},
						"bindTo": "GKILicensingTerrasoftSupportAddress",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKILicensingTerrasoftSupportAddressCaption"
							}
						}
					}
				}, 
				{
					"operation": "insert",
					"name": "GKILicensingWinInstanceLogin",
					"parentName": "LDAPSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 0,
							"row": 2,
							"colSpan": 8
						},
						"bindTo": "GKILicensingWinInstanceLogin",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKILicensingWinInstanceLoginCaption"
							}
						}
					}
				}, 
				{
					"operation": "insert",
					"name": "GKILicensingWinInstancePassword",
					"parentName": "LDAPSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 8,
							"row": 2,
							"colSpan": 8
						},
						"bindTo": "GKILicensingWinInstancePassword",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKILicensingWinInstancePasswordCaption"
							}
						},
						"controlConfig": {
							"protect": true
						}
					}
				}, 
				{
					"operation": "insert",
					"name": "GKILicensingWinInstanceUrl",
					"parentName": "LDAPSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 0,
							"row": 3,
							"colSpan": 8
						},
						"bindTo": "GKILicensingWinInstanceUrl",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKILicensingWinInstanceUrlCaption"
							}
						}
					}
				}, 
				{
					"operation": "insert",
					"name": "GKISysLicUserInactivenessControlTimespan",
					"parentName": "DeactivationSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 0,
							"row": 0,
							"colSpan": 8
						},
						"bindTo": "GKISysLicUserInactivenessControlTimespan",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKISysLicUserInactivenessControlTimespanCaption"
							}
						}
					}
				}, 
				{
					"operation": "insert",
					"name": "GKISysLicUserInactivenessControlCooldown",
					"parentName": "DeactivationSettingsGroup_GridLayout",
					"propertyName": "items",
					"index": 0,
					"values": {
						"layout": {
							"column": 8,
							"row": 0,
							"colSpan": 8
						},
						"bindTo": "GKISysLicUserInactivenessControlCooldown",
						"labelConfig": {
							"visible": true,
							"caption": {
								"bindTo": "Resources.Strings.GKISysLicUserInactivenessControlCooldownCaption"
							}
						}
					}
				}
			]
		};
	});
